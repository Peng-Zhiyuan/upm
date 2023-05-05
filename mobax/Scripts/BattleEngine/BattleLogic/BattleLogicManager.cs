namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class BattleLogicManager : Singleton<BattleLogicManager>
    {
        private BattleData battleData;
        public BattleData BattleData
        {
            get { return battleData; }
        }
        public ActorLogic ActorLogic { get; protected set; }
        public BattleRandomLogic Rand { get; protected set; }

        protected float LogicAddTime = 0f;
        public int CurrentFrame = 0; //当前帧
        public int FinishFrame = 0; //目标帧
        //是否战斗验证模式
        private bool isVerificationMode = false;
        public bool IsOpenBattleViewLayer
        {
            get
            {
#if !SERVER
                if (!isVerificationMode)
                {
                    return true;
                }
                return false;
#else
                return false;
#endif
            }
        }

        //是否防御添加CD
        public bool IsDefAddSkillCD = false;

        protected void SetCurrentFrame(int _currentFrame)
        {
            CurrentFrame = _currentFrame;
        }

        //是否回放
        private bool isReport = false;

        public bool IsReport
        {
            get { return isReport; }
            set { isReport = value; }
        }
        private bool isBattleFinish = false;

        public bool IsBattleEnd
        {
            get { return isBattleFinish; }
            set { isBattleFinish = value; }
        }
        public System.Action<string> delegateUpdateHero;
        public System.Action<BDamageAction> delegateUpdateBattleDamage;

        public override void Init()
        {
            EventManager.Instance.AddListener<int>("BattleWaveFinish", BattleWaveFinish);
            EventManager.Instance.AddListener<SkillAbilityExecution>("OnEndSkillPointPoint", SSPKillExecuteEnd);
        }

        public override void Dispose()
        {
            delegateUpdateBattleDamage = null;
            delegateUpdateHero = null;
            EventManager.Instance.RemoveListener<int>("BattleWaveFinish", BattleWaveFinish);
            EventManager.Instance.RemoveListener<SkillAbilityExecution>("OnEndSkillPointPoint", SSPKillExecuteEnd);
        }

        public void InitLogicManager(string _battleKey, int _seed, bool _isVerificationMode = false)
        {
            battleData = new BattleData();
            battleData.Init();
            battleData.battleKey = _battleKey;
            ActorLogic = new ActorLogic(battleData);
            SetCurrentFrame(1);
            LogicAddTime = 0f;
            isBattleFinish = false;
            isVerificationMode = _isVerificationMode;
            _finalBlowFrame = -1;
            _isAddFinalBlowFrame = false;
            Rand = new BattleRandomLogic(_seed);
            ExecuteLinkerLst.Clear();
        }

        public LogicState Update(float dt = 0)
        {
            // if (CurrentFrame > FinishFrame)
            // {
            //     return LogicState.End;
            // }
            if (dt >= 0)
            {
                LogicAddTime += dt;
                if (LogicAddTime < BattleLogicDefine.LogicSecTime)
                {
                    return LogicState.Nothing;
                }
                else
                {
                    LogicAddTime -= BattleLogicDefine.LogicSecTime;
                }
            }
            if (BattleResultManager.Instance.CheckBattleEnd()) //&& !CheckSkillDoing()
            {
                if (IsOpenBattleViewLayer && (!BattleResultManager.Instance.IsBattleWin || !CheckSkillDoing()))
                {
                    List<CombatActorEntity> actorLst = battleData.atkActorLst;
                    for (int i = 0; i < actorLst.Count; i++)
                    {
                        if (actorLst[i].IsCantSelect)
                        {
                            continue;
                        }
                        actorLst[i].BreakAllSkill();
                    }
                    isBattleFinish = true;
                    return LogicState.End;
                }
                else
                {
                    isBattleFinish = true;
                    return LogicState.End;
                }
            }
            LogicFrameTimerMgr.Instance.OnLogicUpdate();
            BattleEventManager.Instance.ExcuteInputEvent(CurrentFrame);
            ActorLogic.DoLogic(CurrentFrame);
            BattleEventManager.Instance.ExcuteAIEvent(CurrentFrame);
            BattleEventManager.Instance.ExcuteProcessEvent(CurrentFrame);
            CheckBattleFinalBlow();
            SetCurrentFrame(CurrentFrame + 1);
            if (LogicAddTime > BattleLogicDefine.LogicSecTime)
            {
                return LogicState.Quick;
            }
            return LogicState.Playing;
        }

        public bool CheckSkillDoing()
        {
            List<CombatActorEntity> actorLst = battleData.atkActorLst;
            for (int i = 0; i < actorLst.Count; i++)
            {
                if (actorLst[i].IsCantSelect)
                {
                    continue;
                }
                if (actorLst[i].isSpellingSkill())
                {
                    return true;
                }
            }
            return false;
        }

        private void BattleWaveFinish(int wave)
        {
            CheckFriendToFinish();
            CheckLinkerToFinish();
            ClearDebuff();
        }

        private void SSPKillExecuteEnd(SkillAbilityExecution skillAbilityExecution)
        {
            if (skillAbilityExecution.OwnerEntity != null
                && skillAbilityExecution.SkillAbility.SkillBaseConfig.skillType == (int)SKILL_TYPE.SSPMove)
            {
                if (skillAbilityExecution.OwnerEntity.PosIndex == BattleConst.SSPAssistPosIndexStart)
                {
                    BattleLog.LogWarning("Assist Hero is Attack Finish");
                }
                CheckLinkerToFinish(skillAbilityExecution.OwnerEntity.UID);
            }
            else if (skillAbilityExecution.OwnerEntity != null
                     && skillAbilityExecution.SkillAbility.SkillBaseConfig.skillType == (int)SKILL_TYPE.SPSKL)
            {
                if (skillAbilityExecution.OwnerEntity.isAtker)
                {
                    BattleAutoFightManager.Instance.CleanSSPCD();
                }
                else
                {
                    BattleAutoFightManager.Instance.CleanSSPCD_Def();
                }
            }
        }

        public void ClearDebuff()
        {
            List<CombatActorEntity> atkList = BattleData.atkActorLst;
            for (int i = 0; i < atkList.Count; i++)
            {
                atkList[i].OnClearDeBuff();
            }
        }

#region 战斗最后时间增加双方Buff
        /// <summary>
        /// 检测最后几秒,伤害加倍
        /// </summary>
        /// <param name="currentFrame"></param>
        private int _finalBlowFrame = -1;
        private bool _isAddFinalBlowFrame = false;

        public void CheckBattleFinalBlow()
        {
            if (FinishFrame <= 0)
            {
                return;
            }
            if (_finalBlowFrame == -1)
            {
                _finalBlowFrame = FinishFrame - StaticData.BaseTable["battleFinalBlowTime"] * BattleLogicDefine.LogicSecFrame;
            }
            if (CurrentFrame >= _finalBlowFrame
                && !_isAddFinalBlowFrame)
            {
                int addBuffID = StaticData.BaseTable["battleFinalBlowBuffID"];
                var data = battleData.allActorDic.GetEnumerator();
                while (data.MoveNext())
                {
                    if (data.Current.Value.IsCantSelect)
                    {
                        continue;
                    }
                    data.Current.Value.AttachBuff(addBuffID);
                }
                _isAddFinalBlowFrame = true;
            }
        }
#endregion

#region Event
        /// <summary>
        /// 集火
        /// </summary>
        /// <param name="selectTeamKey">自身队伍ID</param>
        /// <param name="_id">集火目标</param>
        public void AtkerFocusOnFiring(int selectTeamKey, string _id)
        {
            BattleInputOperaterData data = new BattleInputOperaterData();
            data.InitAtkFocusOnFiringEvent(_id, selectTeamKey);
            BattleEventManager.Instance.SendInputEvent(CurrentFrame + 1, data);
        }

        /// <summary>
        /// 保护队友
        /// </summary>
        /// <param name="_id">保护ID</param>
        public void DefendFocusOnFiring(string _id)
        {
            BattleInputOperaterData data = new BattleInputOperaterData();
            data.InitDefFocusOnFiringEvent(_id);
            BattleEventManager.Instance.SendInputEvent(CurrentFrame + 1, data);
        }

        /// <summary>
        /// 取消集火功能
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="targetKey"></param>
        public void ResetTargetKey(string _id, string targetKey)
        {
            BattleInputOperaterData data = new BattleInputOperaterData();
            data.InitResetTargetKeyEvent(_id, targetKey);
            BattleEventManager.Instance.SendInputEvent(CurrentFrame + 1, data);
        }

        public void CureHurt(int selectTeamKey, string _id)
        {
            BattleInputOperaterData data = new BattleInputOperaterData();
            data.InitCureHurtEvent(_id, selectTeamKey);
            BattleEventManager.Instance.SendInputEvent(CurrentFrame + 1, data);
        }

        public void MoveActorToPos(string _id, Vector3 targetPos)
        {
            if (!battleData.allActorDic.ContainsKey(_id))
            {
                return;
            }
            BattleInputOperaterData data = new BattleInputOperaterData();
            data.InitMoveToPosEvent(_id, targetPos);
            BattleEventManager.Instance.SendInputEvent(CurrentFrame + 1, data);
        }

        public void MoveTeam2Location(int teamKey, UnityEngine.Vector3 location)
        {
            List<CombatActorEntity> actors;
            int result = BattleUtil.SearchTeam(battleData, teamKey, out actors);
            for (int i = 0; i < actors.Count; i++)
            {
                MoveActorToPos(actors[i].UID, location);
            }
        }

        /// <summary>
        /// 释放大招事件
        /// </summary>
        /// <param name="_id">英雄唯一ID</param>
        public void ActorUseSPSkill(string _id)
        {
            if (!battleData.allActorDic.ContainsKey(_id))
            {
                return;
            }
            CombatActorEntity actorEntity = battleData.GetActorEntity(_id);
            if (actorEntity.CurrentSkillExecution != null
                && actorEntity.CurrentSkillExecution.SkillAbility.SkillBaseConfig.skillType == (int)SKILL_TYPE.SPSKL)
            {
                return;
            }
            BattleInputOperaterData data = new BattleInputOperaterData();
            data.InitApplySkillEvent(_id);
            BattleEventManager.Instance.SendInputEvent(CurrentFrame + 1, data);
        }

        /// <summary>
        /// 上场技能
        /// </summary>
        /// <param name="_id"></param>
        public void ActorTurnOnSkill(string _id, Vector3 repacePos)
        {
            if (!battleData.allActorDic.ContainsKey(_id))
            {
                return;
            }
            BattleInputOperaterData inputData = new BattleInputOperaterData();
            inputData.InitApplyMoveSkillEvent(_id, repacePos);
            BattleEventManager.Instance.SendInputEvent(CurrentFrame + 1, inputData);
        }

        /// <summary>
        /// 释放主角技能
        /// </summary>
        /// <param name="chooseActorUID">当前选中英雄</param>
        /// <param name="skillID">道具技能ID</param>
        public void ActorUseItemSkill(string chooseActorUID, int skillID)
        {
            CombatActorEntity roleActor = null;
            for (int i = 0; i < battleData.atkActorLst.Count; i++)
            {
                if (battleData.atkActorLst[i].PosIndex == BattleConst.PlayerPosIndex)
                {
                    roleActor = battleData.atkActorLst[i];
                    break;
                }
            }
            if (roleActor == null)
            {
                return;
            }
            BattleInputOperaterData data = new BattleInputOperaterData();
            data.InitApplyItemSkillEvent(roleActor.UID, chooseActorUID, skillID);
            BattleEventManager.Instance.SendInputEvent(CurrentFrame + 1, data);
        }

        /// <summary>
        /// 手动触发位移技能
        /// </summary>
        /// <param name="_id"></param>
        public void ActorMoveSkill(string _id, string _targetID)
        {
            if (!battleData.allActorDic.ContainsKey(_id))
            {
                return;
            }
            BattleInputOperaterData data = new BattleInputOperaterData();
            data.InitApplyMoveSkillEvent(_id, _targetID);
            BattleEventManager.Instance.SendInputEvent(CurrentFrame + 1, data);
        }

        public void AttackPartsEvent(int selectTeamKey, string _id, string _parentNode)
        {
            CombatActorEntity enemyData = battleData.allActorDic[_id];
            if (enemyData.IsCantSelect)
            {
                AtkerFocusOnFiring(selectTeamKey, _id);
                return;
            }
            BattleInputOperaterData data = new BattleInputOperaterData();
            data.InitAttackPartsEvent(selectTeamKey, _id, _parentNode);
            BattleEventManager.Instance.SendInputEvent(CurrentFrame + 1, data);
        }

        public void SendSwitchTurnOnInputEvent(string leaveID, string joinID, Vector3 initPos)
        {
            if (!battleData.allActorDic.ContainsKey(leaveID))
            {
                return;
            }
            BattleInputOperaterData data = new BattleInputOperaterData();
            data.InitSwitchTurnOnEvent(leaveID, joinID, initPos);
            BattleEventManager.Instance.SendInputEvent(CurrentFrame + 1, data);
        }

        public void SendSwitchTurnOffInputEvent(string leaveID)
        {
            if (!battleData.allActorDic.ContainsKey(leaveID))
            {
                return;
            }
            BattleInputOperaterData data = new BattleInputOperaterData();
            data.InitSwitchTurnOffEvent(leaveID);
            BattleEventManager.Instance.SendInputEvent(CurrentFrame + 1, data);
        }
#endregion

#region 好友助战
        private string currendFreindUID = "";

        public void SendFriendToBattleInputEvent(string friendUID, string chooseUID)
        {
            if (!battleData.allActorDic.ContainsKey(friendUID)
                || !battleData.allActorDic.ContainsKey(chooseUID))
            {
                return;
            }
            BattleInputOperaterData data = new BattleInputOperaterData();
            data.InitFriendToBattleEvent(friendUID, chooseUID);
            BattleEventManager.Instance.SendInputEvent(CurrentFrame + 1, data);
            currendFreindUID = friendUID;
        }

        public void SendFriendQuitBattleInputEvent(string friendUID)
        {
            BattleInputOperaterData data = new BattleInputOperaterData();
            data.InitFriendQuitBattleEvent(friendUID);
            BattleEventManager.Instance.SendInputEvent(CurrentFrame + 1, data);
#if !SERVER
            if (IsOpenBattleViewLayer)
            {
                BattleManager.Instance.ViewExecuteFriendQuitBattle(friendUID);
            }
#endif
            currendFreindUID = "";
        }

        private void CheckFriendToFinish()
        {
            if (string.IsNullOrEmpty(currendFreindUID))
            {
                return;
            }
            SendFriendQuitBattleInputEvent(currendFreindUID);
        }
#endregion

#region Linker Battle
        private List<string> ExecuteLinkerLst = new List<string>();

        public void ExecuteLinkerToBattle(string SSPSkillUseUID)
        {
            CombatActorEntity useActor = BattleData.GetActorEntity(SSPSkillUseUID);
            for (int i = 0; i < useActor.LinkerUIDLst.Count; i++)
            {
                if (string.IsNullOrEmpty(useActor.LinkerUIDLst[i]))
                {
                    continue;
                }
                SendLinkerToBattleInputEvent(useActor.LinkerUIDLst[i], SSPSkillUseUID, i);
            }
#if !SERVER
            if (IsOpenBattleViewLayer)
            {
                BattleManager.Instance.ViewExecuteLinkerToBattle(SSPSkillUseUID);
            }
#endif
        }

        private void CheckLinkerToFinish(string linkUID = "")
        {
            if (string.IsNullOrEmpty(linkUID))
            {
                for (int i = 0; i < ExecuteLinkerLst.Count; i++)
                {
                    SendLinkerQuitBattleInputEvent(ExecuteLinkerLst[i]);
                }
                ExecuteLinkerLst.Clear();
            }
            else
            {
                if (ExecuteLinkerLst.Count > 0
                    && ExecuteLinkerLst.Contains(linkUID))
                {
                    SendLinkerQuitBattleInputEvent(linkUID);
                    ExecuteLinkerLst.Remove(linkUID);
                }
            }
        }

        public void SendLinkerToBattleInputEvent(string linkerUID, string createUID, int linkerIndex)
        {
            if (!battleData.allActorDic.ContainsKey(linkerUID)
                || !battleData.allActorDic.ContainsKey(createUID))
            {
                return;
            }
            BattleLinkerManager.Instance.ExcuteLinkerToBattle(battleData.GetActorEntity(linkerUID), battleData.GetActorEntity(createUID), linkerIndex);
            if (!ExecuteLinkerLst.Contains(linkerUID))
            {
                ExecuteLinkerLst.Add(linkerUID);
            }
        }

        public void SendLinkerQuitBattleInputEvent(string linkerUID)
        {
            if (!battleData.allActorDic.ContainsKey(linkerUID))
            {
                return;
            }
            BattleLinkerManager.Instance.ExcuteLinkerQuitBattle(battleData.GetActorEntity(linkerUID));
#if !SERVER
            if (IsOpenBattleViewLayer)
            {
                BattleManager.Instance.ViewExecuteLinkerQuitBattle(linkerUID);
            }
#endif
        }
#endregion
    }
}