namespace BattleEngine.Logic
{
    using View;
    using System.Collections.Generic;
    using BattleSystem.ProjectCore;
    using UnityEngine;
    using Neatly.Timer;

    public sealed class BattleManager : Singleton<BattleManager>, IUpdatable
    {
#region Var
        /// <summary>
        /// 战斗表现层演员管理
        /// </summary>
        public BattleActorManager ActorMgr;

        /// <summary>
        /// 大招管理
        /// </summary>
        public BattleSPSSkillManager SPSSkillMgr;

        /// <summary>
        /// 召唤管理
        /// </summary>
        public BattleSummonManager SummonManager;

        /// <summary>
        /// 动画速度
        /// </summary>
        private float animSpeed;

        public float AnimSpeed
        {
            get { return animSpeed; }
        }

        private Neatly.NeatlyBehaviour neatlyDrive;

        public BattleSenceRecord BattleInfoRecord;
#endregion

#region Method
        public void OnUpdate() { }

        public async void Init()
        {
            AIManager.Initialize();
            // -----------------------------------
            //              实体系统初始化
            // -----------------------------------
            MasterEntity.Create();
            Entity.Create<CombatContext>();
            // -----------------------------------
            //              构造角色头部数据
            // -----------------------------------
            if (NeatlyTimer.instance == null)
            {
                NeatlyTimer.Init();
            }
            NeatlyTimer.instance.timeStop = false;
            string battleKey = Battle.Instance.BattleResponse != null ? Battle.Instance.BattleResponse.id : "Test";
            BattleLogicManager.Instance.InitLogicManager(battleKey, Clock.Now.Millisecond, false);
            BattleLogicManager.Instance.IsDefAddSkillCD = Battle.Instance.mode.ModeType != BattleModeType.Arena;
            BattleLogicManager.Instance.IsReport = Battle.Instance.isReportBattle;
            if (ActorMgr == null)
            {
                ActorMgr = new BattleActorManager();
                ActorMgr.Init();
            }
            ActorMgr.Clear();
            SceneObjectManager.Instance.actMgr = ActorMgr;
            if (SPSSkillMgr == null)
                SPSSkillMgr = new BattleSPSSkillManager();
            if (SummonManager == null)
                SummonManager = new BattleSummonManager();
            int tempTime = (int)BattleUtil.GetGlobalK(GlobalK.NORMAL_BATTLETIME_35);
            var stageinfo = StaticData.StageTable.TryGet(Battle.Instance.CopyId);
            if (stageinfo != null)
            {
                tempTime = stageinfo.endTime;
            }
            BattleLogicManager.Instance.FinishFrame = Mathf.FloorToInt(tempTime * BattleLogicDefine.LogicSecFrame);
            BattleLogicManager.Instance.BattleData.finishFrame = BattleLogicManager.Instance.FinishFrame;
            BattleTimeManager.Instance.InitBattleTime(tempTime, SetBattleResultState);
            if (BattleInfoRecord == null)
            {
                BattleInfoRecord = new BattleSenceRecord();
            }
            BattleInfoRecord.InitData();
            BattleLogicManager.Instance.delegateUpdateBattleDamage += delegate(BDamageAction action)
            {
                ActorMgr.PlayHitActionState(action);
                DamageManager.Instance.ShowDamage(action);
            };
            BattleLogicManager.Instance.delegateUpdateHero = delegate(string uid) { HudManager.Instance.UpdateHero(uid); };
            BattleEventManager.Instance.ClearEvent();
            animSpeed = 1.0f;
        }

        public void BattleExecute()
        {
            BattleInfoRecord.CheckHelpHeroID(BattleLogicManager.Instance.BattleData);
            BattleTimeManager.Instance.BeginBattleTime();
            // ----------------------------- 切换时间驱动 -----------------------------
            if (neatlyDrive != null)
            {
                TimerHelper.Remove(neatlyDrive);
            }
            neatlyDrive = TimerHelper.AddFrame(BattleLogicManager.Instance.BattleData.battleKey, UpdateBattle, 1);
            // ----------------------------- 切换时间驱动 -----------------------------
            NeatlyTimer.instance.timeStop = false;
            RefreshBattleData();
        }

        public void RefreshBattleData()
        {
            BattleLogicManager.Instance.BattleData.ResetActorData();
            BattleLogicManager.Instance.BattleData.ResetInitOTNum();
        }

        /// <summary>
        /// 退出战斗
        /// </summary>
        public void QuitBattle()
        {
            ActorMgr.Clear();
            BattleLogicManager.DestroyInstance();
            Entity.Destroy(MasterEntity.Instance);
            MasterEntity.Destroy();
            BattleEventManager.DestroyInstance();
            BattleResManager.DestroyInstance();
            BattleTimeManager.Instance.EndBattleTime();
            BattleTimeManager.DestroyInstance();
            SkillPassiveAbilityManager.DestroyInstance();
            SkillCameraCtr.DestroyInstance();
            NeatlyTimer.Remove(neatlyDrive);
            TimerMgr.Instance.RemoveType(TimerType.Battle);
            LogicFrameTimerMgr.Instance.RemoveAll();
            neatlyDrive = null;
            if (NeatlyTimer.instance != null)
            {
                NeatlyTimer.instance.timeStop = true;
                GameObject.DestroyImmediate(NeatlyTimer.instance.gameObject);
            }
            BattleDataManager.Instance.TimeScale = 1.0f;
            isSummonCat = false;
            BattleResultManager.Instance.ClearData();
            SocializeManager.Stuff.ResetSocializeAssistAsync();
            EventManager.Instance.ClearAllListener();
            BattleResManager.Instance.releaseAll();
            DestroyInstance();
        }

        public void UpdateBattle(float dt)
        {
            if (BattleLogicManager.Instance.IsBattleEnd)
            {
                return;
            }
            LogicState result = BattleLogicManager.Instance.Update(dt);
            if (result == LogicState.Quick)
            {
                while (true)
                {
                    result = BattleLogicManager.Instance.Update(0);
                    if (result != LogicState.Quick)
                    {
                        break;
                    }
                }
            }
            if (result == LogicState.Playing)
            {
                MasterEntity.Instance.LogicUpdate(dt);
                ActorMgr.UpdateActorManager();
                BattleAutoFightManager.Instance.UpdateFrame(dt);
                BattleDataManager.Instance.UpdateFrame(dt);
            }
            else if (result == LogicState.End)
            {
                ExecuteLogicEnd();
                return;
            }
            //召唤猫猫
            CheckSummonCat();
        }

        private void ExecuteLogicEnd()
        {
            if (!string.IsNullOrEmpty(SPSSkillMgr.currentSPSkillUID))
            {
                SPSSkillMgr.FinishSPSkillPreEffect(SPSSkillMgr.currentSPSkillUID);
            }
            for (int i = 0; i < BattleLogicManager.Instance.BattleData.bulletList.Count; i++)
            {
                if (BattleLogicManager.Instance.BattleData.bulletList[i] == null)
                {
                    continue;
                }
                Entity.Destroy(BattleLogicManager.Instance.BattleData.bulletList[i]);
            }
            GameEventCenter.Broadcast(GameEvent.WaveEnd);
            EventManager.Instance.SendEvent("BattleWaveFinish", Battle.Instance.Wave);
            Battle.Instance.OnWaveEnd();
            List<Creature> exitActorObject = ActorMgr.GetAllActors();
            for (int i = 0; i < exitActorObject.Count; i++)
            {
                if (exitActorObject[i].mData.IsCantSelect)
                    continue;
                if (exitActorObject[i].mData.CurrentSkillExecution != null)
                {
                    exitActorObject[i].mData.CurrentSkillExecution.BreakActionsImmediate();
                }
                exitActorObject[i].ToIdleAnim();
                exitActorObject[i].PlayAnim("idle", true);
            }
            ActorMgr.UpdateActorManager();
            if (BattleResultManager.Instance.IsBattleWin
                && BattleStateManager.Instance.battle.NextWaveAvailable())
            {
                CheckBattleEndEffect(() =>
                                {
                                    BattleLog.LogWarning("[Battle Pipline] NextWaveAvailable");
                                    BattleEventManager.Instance.ClearEvent();
                                    BattleStateManager.Instance.ChangeState(eBattleState.Move);
                                }
                );
                return;
            }
            SetBattleResultState();
        }

        private void SetBattleResultState()
        {
            if (neatlyDrive != null)
            {
                TimerHelper.Remove(neatlyDrive);
            }
            BattleTimeManager.Instance.EndBattleTime();
            BattleResManager.Instance.releaseEffects();
            CheckBattleEndEffect(() =>
                            {
                                BattleLog.LogWarning("[Battle Pipline] CheckBattleEndEffect");
                                TimerMgr.Instance.PauseType(TimerType.Battle);
                                LogicFrameTimerMgr.Instance.PauseAll();
                                NeatlyTimer.instance.timeStop = true;
                                CameraManager.Instance.UpdateEnable = false;
                                CameraSetting.Ins.CloseNoise();
                                BattleStateManager.Instance.ChangeState(eBattleState.Settlement);
                            }
            );
        }

        /// <summary>
        /// 检测战斗结束标志,比如死亡动画等
        /// </summary>
        /// <param name="delegateEnd"></param>
        private void CheckBattleEndEffect(System.Action delegateEnd)
        {
            if (!Battle.HasInstance())
            {
                QuitBattle();
                return;
            }
            if (Battle.Instance.IsFight
                && Battle.Instance.param.mode == BattleModeType.Gold
                && BattleResultManager.Instance.IsBattleWin)
            {
                BattleLog.LogWarning("[Battle Pipline] WaitGOLD");
                TimerMgr.Instance.ScheduleTimer(4.0f, delegateEnd, false, "BattleManagerWaitGOLD");
                return;
            }
            List<Creature> allActor = ActorMgr.GetAllActors();
            for (int i = 0; i < allActor.Count; i++)
            {
                if (!BattleConst.IsFightPosIndex(allActor[i].mData.PosIndex))
                {
                    continue;
                }
                if (allActor[i].mData.CurrentHealth.Value <= 0
                    && !allActor[i].RemoveTag)
                {
                    BattleLog.LogWarning("[Battle Pipline] WaitDead");
                    TimerMgr.Instance.ScheduleTimer(2.0f, delegateEnd, false, "BattleManagerWaitDead");
                    return;
                }
            }
            BattleLog.LogWarning("[Battle Pipline] delegateEnd");
            delegateEnd?.Invoke();
        }

        public void BtnPause(bool isPause)
        {
            if (!isPause)
            {
                List<Creature> lst = ActorMgr.GetAllActors();
                for (int i = 0; i < lst.Count; i++)
                {
                    if (lst[i].mData.IsCantSelect)
                        continue;
                    lst[i].ToResumeAnim();
                }
                BattleTimeManager.Instance.BeginBattleTime();
                TimerMgr.Instance.ResumeType(TimerType.Battle);
                LogicFrameTimerMgr.Instance.ResumeAll();
                BattleResManager.Instance.ResumeALLUsingEffect();
                NeatlyTimer.instance.timeStop = false;
                CameraManager.Instance.UpdateEnable = true;
                CameraSetting.Ins.OpenNoise();
            }
            else
            {
                List<Creature> lst = ActorMgr.GetAllActors();
                for (int i = 0; i < lst.Count; i++)
                {
                    if (lst[i].mData.IsCantSelect)
                        continue;
                    lst[i].ToPauseAnim();
                }
                BattleTimeManager.Instance.PauseBattleTime();
                TimerMgr.Instance.PauseType(TimerType.Battle);
                LogicFrameTimerMgr.Instance.PauseAll();
                BattleResManager.Instance.PauseALLUsingEffect();
                NeatlyTimer.instance.timeStop = true;
                CameraManager.Instance.UpdateEnable = false;
                CameraSetting.Ins.CloseNoise();
            }
        }

        public void StopAllAI()
        {
            BattleTimeManager.Instance.PauseBattleTime();
            NeatlyTimer.instance.timeStop = true;
        }

        public void StartAllAI()
        {
            NeatlyTimer.instance.timeStop = false;
        }

        public void KillAllEnemy()
        {
            List<CombatActorEntity> enemyLst = BattleLogicManager.Instance.BattleData.defActorLst;
            for (int i = 0; i < enemyLst.Count; i++)
            {
                if (enemyLst[i].IsDead)
                {
                    continue;
                }
                enemyLst[i].AddHp(1 - enemyLst[i].CurrentHealth.Value);
            }
        }

        public void KillOneHero()
        {
            foreach (var VARIABLE in ActorMgr.GetCamp(0))
            {
                VARIABLE.mData.AddHp(1 - VARIABLE.mData.CurrentHealth.Value);
                break;
            }
        }
#endregion

#region 替补上场——功能暂时废弃
        /// <summary>
        /// 检测死亡触发替换上场
        /// </summary>
        public void CheckDeadSwitchHero(string _id)
        {
            CombatActorEntity damageActor = BattleLogicManager.Instance.BattleData.GetActorEntity(_id);
            if (damageActor == null
                || !damageActor.IsDead)
            {
                return;
            }
            CombatActorEntity joinActor = null;
            List<CombatActorEntity> teamLst = BattleLogicManager.Instance.BattleData.GetTeamList(damageActor.TeamKey);
            for (int i = 0; i < teamLst.Count; i++)
            {
                if (teamLst[i].IsDead
                    || teamLst[i].CurrentLifeState != ACTOR_LIFE_STATE.Substitut)
                {
                    continue;
                }
                joinActor = teamLst[i];
                break;
            }
            Creature leaveCreature = ActorMgr.GetActor(damageActor.UID);
            if (joinActor == null)
            {
                GameEventCenter.Broadcast(GameEvent.RoleDie, leaveCreature);
                return;
            }
            Creature joinCreature = ActorMgr.GetActor(joinActor.UID);
            SwitchHero(joinCreature, leaveCreature);
            if (damageActor.isAtker)
            {
                List<Creature> teamCreatureLst = ActorMgr.GetTeam(joinActor.TeamKey);
                foreach (var VARIABLE in ActorMgr.GetAllActors())
                {
                    if (!VARIABLE.mData.IsDead)
                        VARIABLE.Trigger(EmojiEvent.FriendDie);
                }
            }
        }

        public void SendHeroTurnOn(string leaveUID, string joinUID, Vector3 initPos)
        {
            BattleLogicManager.Instance.SendSwitchTurnOnInputEvent(leaveUID, joinUID, initPos);
        }

        public void SendHeroTurnOff(string leaveUID)
        {
            BattleLogicManager.Instance.SendSwitchTurnOffInputEvent(leaveUID);
        }

        /// <summary>
        /// 替换队员
        /// </summary>
        /// <param name="join"></param>
        /// <param name="leave"></param>
        public void SwitchHero(Creature join, Creature leave)
        {
            if (join._cc_Turn.IsTurning
                || join.mData.CurrentLifeState != ACTOR_LIFE_STATE.Substitut)
            {
                BattleLog.Log("SwitchHero Join hero IsTurning" + join._cc_Turn.IsTurning + "  lifeState " + join.mData.CurrentLifeState.ToString());
                return;
            }
            join._cc_Turn.IsTurning = true;
            if (leave.mData.CurrentSkillExecution != null)
            {
                leave.mData.BreakCurentSkillImmediate(() => { ExecuteSwitchHero(join, leave); });
            }
            else
            {
                ExecuteSwitchHero(join, leave);
            }
        }

        private void ExecuteSwitchHero(Creature join, Creature leave)
        {
            var joinInfo = Battle.Instance.GetSubJoinPosition();
            var dir = (leave.SelfTrans.position - joinInfo.Pos).normalized;
            join._cc_Turn.TurnOn(joinInfo.Pos, dir, leave);
            if (!leave.mData.IsDead)
            {
                var leavepos = joinInfo.Pos + Quaternion.AngleAxis(StageBattleUtil.GetMapRotationY(joinInfo), Vector3.up) * Vector3.left * 2f;
                leave._cc_Turn.TurnOff(leavepos);
            }
        }
#endregion

#region 好友助战
        public void ExecuteFreindToBattle()
        {
            CombatActorEntity friendActor = null;
            List<CombatActorEntity> teamList = BattleLogicManager.Instance.BattleData.GetTeamList(BattleConst.ATKTeamID);
            for (int i = 0; i < teamList.Count; i++)
            {
                if (teamList[i].PosIndex != BattleConst.FriendPosIndex
                    || teamList[i].IsDead)
                {
                    continue;
                }
                friendActor = teamList[i];
            }
            if (friendActor == null
                || SceneObjectManager.Instance.GetSelectPlayer() == null)
            {
                return;
            }
            BattleLogicManager.Instance.SendFriendToBattleInputEvent(friendActor.UID, SceneObjectManager.Instance.GetSelectPlayer().mData.UID);
            Vector3 targetPos = Vector3.zero;
            if (!string.IsNullOrEmpty(SceneObjectManager.Instance.GetSelectPlayer().mData.targetKey))
            {
                CombatActorEntity targetActor = BattleLogicManager.Instance.BattleData.GetActorEntity(SceneObjectManager.Instance.GetSelectPlayer().mData.targetKey);
                targetPos = targetActor.GetPosition();
            }
            else
            {
                targetPos = BattleUtil.GetCheckTargetPos(BattleLogicManager.Instance.BattleData, friendActor, SceneObjectManager.Instance.GetSelectPlayer().mData, friendActor.SPSKL.SkillBaseConfig.Range * 0.01f);
            }
            if (!BattleUtil.IsInMap(targetPos))
            {
                Vector3 recpacePos = BattleUtil.GetWalkablePos(targetPos);
                targetPos.y = recpacePos.y;
            }
            Creature friendCreature = ActorMgr.GetActor(friendActor.UID);
            friendCreature._cc_Friend.FriendToBattle(targetPos);
            BattleInfoRecord.IsUseHelpHero = true;
        }

        public void ViewExecuteFriendQuitBattle(string friendUID)
        {
            Creature creature = ActorMgr.GetActor(friendUID);
            if (creature != null)
            {
                creature._cc_Friend.FriendQuitBattle();
            }
        }
#endregion

#region 链接者助战
        public void ViewExecuteLinkerToBattle(string SSPSkillUseUID)
        {
            CombatActorEntity useActor = BattleLogicManager.Instance.BattleData.GetActorEntity(SSPSkillUseUID);
            for (int i = 0; i < useActor.LinkerUIDLst.Count; i++)
            {
                if (string.IsNullOrEmpty(useActor.LinkerUIDLst[i]))
                {
                    continue;
                }
                Creature creature = ActorMgr.GetActor(useActor.LinkerUIDLst[i]);
                creature._cc_Linker.LinkerToBattle(SSPSkillUseUID);
            }
        }

        public void ViewExecuteLinkerQuitBattle(string linkerUID)
        {
            Creature creature = ActorMgr.GetActor(linkerUID);
            creature._cc_Linker.LinkerQuitBattle();
        }
#endregion

#region Event
        /// <summary>
        /// 发送释放大招
        /// </summary>
        /// <param name="_id">用户ID</param>
        public void SendSpendSPSkill(string _id)
        {
            BattleInfoRecord.SetHeroSPSkillUse(_id);
            BattleLogicManager.Instance.ActorUseSPSkill(_id);
        }

        /// <summary>
        /// 发送释放道具技能
        /// </summary>
        /// <param name="chooseActorUID">当前选中英雄</param>
        /// <param name="skillID">道具技能ID</param>
        public void SendSpendItemSkill(string chooseActorUID, int skillID)
        {
            Creature playCreature = ActorMgr.GetPlayer();
            playCreature.GetModelObject.SetActive(true);
            GameEventCenter.Broadcast(GameEvent.ShowSubCamera, true);
            TimerMgr.Instance.ScheduleTimer(2f, delegate
                            {
                                GameEventCenter.Broadcast(GameEvent.ShowSubCamera, false);
                                playCreature.GetModelObject.SetActive(false);
                            }, false, "WaitSendSpendItemSkill"
            );
            BattleLogicManager.Instance.ActorUseItemSkill(chooseActorUID, skillID);
        }

        /// <summary>
        /// 集火指令
        /// </summary>
        /// <param name="_id"></param>
        public void AtkerFocusOnFiring(string _id)
        {
            BattleInfoRecord.AddForcusNum();
            BattleLogicManager.Instance.AtkerFocusOnFiring(BattleConst.ATKCampID * 1000 + BattleConst.ATKTeamID, _id);
        }

        /// <summary>
        /// 保护指令
        /// </summary>
        /// <param name="_id"></param>
        public void DefendFocusOnFiring(string _id)
        {
            BattleInfoRecord.AddDefenceNum();
            BattleLogicManager.Instance.DefendFocusOnFiring(_id);
        }

        /// <summary>
        /// 指定移动到目标点
        /// </summary>
        /// <param name="_id">全局ID</param>
        /// <param name="targetPos">目标位置</param>
        public void SendActorMoveToPos(string _id, Vector3 targetPos)
        {
            BattleLogicManager.Instance.MoveActorToPos(_id, targetPos);
        }

        public void KillEvent(CombatActorEntity caster)
        {
            if (!caster.isAtker)
                return;
            Creature actor = ActorMgr.GetActor(caster.UID);
            actor.Trigger(EmojiEvent.Kill);
        }
#endregion

#region 召唤猫猫
        /// <summary>
        /// 召唤猫猫
        /// </summary>
        private bool isSummonCat = false;

        private float SummonCatTime = 0;
        private float SummonCatTimeMax = 2.0f;

        public async void CheckSummonCat()
        {
            if (isSummonCat)
            {
                return;
            }
            if (SummonCatTime < SummonCatTimeMax)
            {
                SummonCatTime += Time.deltaTime;
                return;
            }
            SummonCatTime = 0.0f;
            if (BattleTimeManager.Instance.CurrentBattleTime < BattleUtil.GetGlobalK(GlobalK.SUMMON_CAT_36))
            {
                return;
            }
            int catPer = BattleLogicManager.Instance.Rand.RandomVaule(0, 100);
            if (catPer < BattleUtil.GetGlobalK(GlobalK.SUMMON_CAT_PER_37))
            {
                SummonManager.SummonCatToSence();
                TimerMgr.Instance.BattleSchedulerTimer(8f, () => { SummonManager.mapCatView.Clean(); }, false, "WaitCheckSummonCat");
                GameEventCenter.Broadcast(GameEvent.CatEnter);
            }
            isSummonCat = true;
        }
#endregion
    }
}