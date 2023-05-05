namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// 技能执行体
    /// </summary>
    public partial class SkillAbilityExecution : AbilityExecution
    {
        private SkillAbility _skillAbility = null;

        public SkillAbility SkillAbility
        {
            get
            {
                if (_skillAbility == null)
                {
                    _skillAbility = AbilityEntity as SkillAbility;
                }
                return _skillAbility;
            }
        }

        public CombatActorEntity targetActorEntity { get; set; }

        /// <summary>
        public List<CombatActorEntity> AllTargetActorEntity { get; set; }

        /// 当前技能检测命中对象
        /// </summary>
        public Dictionary<int, List<CombatActorEntity>> LastHitEntitysDic = new Dictionary<int, List<CombatActorEntity>>();

        public Vector3 InputPoint { get; set; }
        public List<Vector3> warningPoints = new List<Vector3>();
        public float InputDirection { get; set; }
        public int curFrame;
        public int totalFrame;
        public int frameInputOffset = 0;
        public int frameInputIndex = 0;
        public SkillFlag SkillFlag;
        private List<SkillFlag> skillFlags;

        public List<SkillFlag> SkillFlags
        {
            get
            {
                if (skillFlags == null)
                {
                    skillFlags = new List<SkillFlag>();
                }
                if (skillFlags.Count <= 0)
                {
                    skillFlags.Add(SkillAbility.SkillConfigObject.StartUPFlag);
                    skillFlags.Add(SkillAbility.SkillConfigObject.ActiveFlag);
                    skillFlags.Add(SkillAbility.SkillConfigObject.FollowThroughFlag);
                    skillFlags.Add(SkillAbility.SkillConfigObject.RecoveryFlag);
                }
                return skillFlags;
            }
        }

        public float actionTime;
        private List<AbilityTask> actionLogicTasks = new List<AbilityTask>();
        private List<AbilityViewTask> actionViewTasks = new List<AbilityViewTask>();
        public List<CreatHitEffectTaskData> HitEffectTaskDataLst = new List<CreatHitEffectTaskData>();
        public int HitIndex = 0;
        public System.Action onOver;
        public System.Action onBreak;
        private SKILL_BREAK_CAUSE breakCause = SKILL_BREAK_CAUSE.None;
        public bool isNeedCalTotalFrame = false;

        public override void BeginExecute()
        {
            base.BeginExecute();
            OwnerEntity.ReadySkill = null;
            breakCause = SKILL_BREAK_CAUSE.None;
            SkillFlag = null;
            if (SkillAbility.SkillBaseConfig.skillType == (int)SKILL_TYPE.SPSKL)
            {
                OwnerEntity.CurrentMp.Minus(OwnerEntity.CurrentMp.MaxValue);
            }
            if (SkillAbility.SkillBaseConfig.skillType == (int)SKILL_TYPE.ATK)
            {
                OwnerEntity.ActionPointManageComponent.TriggerActionPoint(ACTION_POINT_TYPE.PostNormalAtk, null);
            }
            if (BattleControlUtil.IsForbidAttack(OwnerEntity, (SKILL_TYPE)SkillAbility.SkillBaseConfig.skillType))
            {
                EndExecute();
                return;
            }
            OwnerEntity.CurrentSkillExecution = this;
            OwnerEntity.SkillExecutions.Add(this);
            SkillAbility.Spelling = true;
            actionTime = 0;
            curFrame = -1;
            totalFrame = 0;
            if (SkillAbility.SkillConfigObject.totalFrame > 0
                && !isNeedCalTotalFrame)
            {
                totalFrame = SkillAbility.SkillConfigObject.totalFrame;
                isNeedCalTotalFrame = false;
            }
            else
            {
                isNeedCalTotalFrame = true;
            }
            HitEffectTaskDataLst.Clear();
            LastHitEntitysDic.Clear();
            HitIndex = 0;
            float speedScale = OwnerEntity.AttrData.Att_AttackSpeed * 1f / OwnerEntity.AttrData.GetBaseValue(AttrType.ATKSPEED);
            int startUpFrame = (int)(SkillAbility.SkillConfigObject.StartUpFrame / speedScale);
            actionLogicTasks.Clear();
            actionViewTasks.Clear();
            List<SkillActionElementItem> saeis = SkillAbility.SkillConfigObject.actionElements;
            int frameOffset = startUpFrame - SkillAbility.SkillConfigObject.StartUpFrame;
            for (int i = 0; i < saeis.Count; i++)
            {
                if (!saeis[i].Enabled)
                {
                    continue;
                }
                if (isNeedCalTotalFrame)
                    totalFrame = Mathf.Max(totalFrame, saeis[i].endFrame + frameOffset);
                saeis[i].initFrame = frameOffset;
                AbilityTaskData taskData = CreateAbilityTaskData(saeis[i].type);
                if (taskData != null)
                {
                    taskData.Init(saeis[i]);
                    var taskLogic = CreateAbilityLogicTask(taskData);
                    if (taskLogic != null)
                    {
                        actionLogicTasks.Add(taskLogic);
                    }
                    if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
                    {
                        var taskView = CreateAbilityViewTask(taskData);
                        if (taskView != null)
                        {
                            actionViewTasks.Add(taskView);
                        }
                    }
                }
            }
            if (frameInputOffset != 0)
            {
                for (int i = 0, len = actionLogicTasks.Count; i < len; i++)
                {
                    AbilityTaskData taskdata = actionLogicTasks[i].taskInitData as AbilityTaskData;
                    if (taskdata.endFrame == frameInputIndex)
                    {
                        taskdata.endFrame += frameInputOffset;
                    }
                    else if (taskdata.endFrame > frameInputIndex)
                    {
                        taskdata.startFrame += frameInputOffset;
                        taskdata.endFrame += frameInputOffset;
                    }
                }
                if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
                {
                    for (int i = 0, len = actionViewTasks.Count; i < len; i++)
                    {
                        AbilityTaskData taskdata = actionViewTasks[i].taskInitData as AbilityTaskData;
                        if (taskdata.endFrame == frameInputIndex)
                        {
                            taskdata.endFrame += frameInputOffset;
                        }
                        else if (taskdata.endFrame > frameInputIndex)
                        {
                            taskdata.startFrame += frameInputOffset;
                            taskdata.endFrame += frameInputOffset;
                        }
                    }
                }
            }
            float k = 1;
            if (!string.IsNullOrEmpty(OwnerEntity.targetKey))
            {
                var def = BattleLogicManager.Instance.BattleData.GetActorEntity(OwnerEntity.targetKey);
                if (def != null
                    && def.isAnitJob(OwnerEntity.Job))
                {
                    k = BattleUtil.GetGlobalK(GlobalK.ENERGYPER_32) * 0.001f;
                }
            }
            OwnerEntity.CurrentMp.Add(Mathf.FloorToInt(SkillAbility.SkillBaseConfig.Rage * k));
            EventManager.Instance.SendEvent<SkillAbilityExecution>("BattleSkillBeginExecution", this);
            OnUpdate(0);
#if !SERVER
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                Creature creature = BattleManager.Instance.ActorMgr.GetActor(SkillAbility.OwnerEntity.UID);
                var aa = StrBuild.Instance.ToStringAppend("skill_", SkillAbility.SkillBaseConfig.SkillID.ToString());
                WwiseEventManager.SendEvent(TransformTable.BeginSkillAbility, aa, creature.gameObject);
            }
#endif
        }

        public override void EndExecute()
        {
            for (int i = 0, len = actionLogicTasks.Count; i < len; i++)
            {
                if (actionLogicTasks[i] == null
                    || actionLogicTasks[i].taskInitData == null)
                {
                    continue;
                }
                actionLogicTasks[i].EndExecute();
            }
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                for (int i = 0, len = actionViewTasks.Count; i < len; i++)
                {
                    if (actionViewTasks[i] == null
                        || actionViewTasks[i].taskInitData == null)
                    {
                        continue;
                    }
                    actionViewTasks[i].EndExecute();
                }
            }
            actionTime = -1;
            EndSpellSkill();
            onOver?.Invoke();
            if (SkillAbility.SkillBaseConfig.skillType == (int)SKILL_TYPE.SPSKL
                || SkillAbility.SkillBaseConfig.skillType == (int)SKILL_TYPE.SSPMove)
            {
                EventManager.Instance.SendEvent<SkillAbilityExecution>("OnEndSkillPointPoint", this);
            }
#if !SERVER
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                Creature creature = BattleManager.Instance.ActorMgr.GetActor(OwnerEntity.UID);
                var aa = StrBuild.Instance.ToStringAppend("skill_", SkillAbility.SkillBaseConfig.SkillID.ToString());
                WwiseEventManager.SendEvent(TransformTable.EndSkillAbility, aa, creature.gameObject);
            }
#endif
            OwnerEntity.RemoveSkillExecutions.Add(this);
            isNeedCalTotalFrame = true;
            base.EndExecute();
        }

        public void EndSpellSkill()
        {
            if (OwnerEntity.CurrentSkillExecution != null
                && OwnerEntity.CurrentSkillExecution.SkillAbility.SkillBaseConfig.SkillID == SkillAbility.SkillBaseConfig.SkillID)
            {
                OwnerEntity.CurrentSkillExecution = null;
                OwnerEntity.ATK = null;
            }
            SkillAbility.Spelling = false;
        }

        private int gameFrame = 0;

        public override void OnUpdate(int frame)
        {
            gameFrame = frame;
            int preFrame = curFrame;
            actionTime += BattleLogicDefine.LogicSecTime;
            curFrame = Mathf.RoundToInt(actionTime / (1f / SkillAbility.SkillConfigObject.fps)); //Mathf.FloorToInt(actionTime * BattleLogicDefine.LogicSecFrame);
            if (SkillAbility.SkillConfigObject.NoTargetNeedBreak
                && targetActorEntity.IsDied()
                && curFrame < SkillAbility.SkillConfigObject.ActiveFlag.flagFrame)
            {
                BreakActionsImmediate();
                return;
            }
            if (preFrame != curFrame)
            {
                for (; preFrame < curFrame && preFrame <= totalFrame; preFrame++)
                {
                    EnterFrame(preFrame);
                }
                if (curFrame == totalFrame)
                {
                    EnterFrame(curFrame);
                }
                if (curFrame >= totalFrame)
                {
                    EndExecute();
                }
            }
        }

        void EnterFrame(int frame)
        {
            if (OwnerEntity == null
                || OwnerEntity.CurrentHealth.Value <= 0)
            {
                ToBreakActions();
                return;
            }
            for (int i = 0; i < SkillFlags.Count; i++)
            {
                if (SkillFlags[i] == null || skillFlags[i].flagFrame == -1)
                {
                    continue;
                }
                if (frame == SkillFlags[i].flagFrame)
                {
                    SkillFlag = SkillFlags[i];
                    break;
                }
            }

            //外部中断技能时只针对当前技能
            if (OwnerEntity.CurrentSkillExecution != null
                && OwnerEntity.CurrentSkillExecution.Id == this.Id) { }
            if (SkillFlag != null
                && SkillFlag.flagCause.HasFlag(breakCause)
                && breakCause != SKILL_BREAK_CAUSE.None)
            {
                ToBreakActions();
                return;
            }
            // if (frame == SkillAbility.SkillConfigObject.RecoveryFlag.flagFrame
            //     && SkillAbility.SkillConfigObject.RecoveryFlagBreak
            //     && OwnerEntity.CurrentSkillExecution != null
            //     && OwnerEntity.CurrentSkillExecution.SkillAbility.SkillBaseConfig.SkillID == SkillAbility.SkillBaseConfig.SkillID)
            // {
            //     OwnerEntity.BreakCurentSkillImmediate();
            //     return;
            // }
            AbilityTaskData taskData = null;
            for (int i = 0; i < actionLogicTasks.Count; i++)
            {
                if (actionLogicTasks[i] == null)
                {
                    actionLogicTasks.RemoveAt(i);
                    i--;
                    continue;
                }
                taskData = actionLogicTasks[i].taskInitData as AbilityTaskData;
                if (taskData == null)
                {
                    actionLogicTasks[i].EndExecute();
                    actionLogicTasks.RemoveAt(i);
                    i--;
                    continue;
                }
                if (taskData.startFrame == frame)
                {
                    actionLogicTasks[i].BeginExecute(frame);
                }
                if (taskData.startFrame <= frame
                    && taskData.endFrame >= frame)
                {
                    if (i >= actionLogicTasks.Count)
                    {
                        BattleLog.LogError("The SkillAbility is out range for " + i + "  actionLogicTask " + actionLogicTasks.Count);
                        continue;
                    }
                    actionLogicTasks[i].DoExecute(frame);
                }
                if (taskData.endFrame <= frame)
                {
                    actionLogicTasks[i].EndExecute();
                    actionLogicTasks.RemoveAt(i);
                    i--;
                }
            }
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                for (int i = 0; i < actionViewTasks.Count; i++)
                {
                    if (actionViewTasks[i] == null)
                    {
                        actionViewTasks.RemoveAt(i);
                        i--;
                        continue;
                    }
                    taskData = actionViewTasks[i].taskInitData as AbilityTaskData;
                    if (taskData == null)
                    {
                        actionViewTasks[i].EndExecute();
                        actionViewTasks.RemoveAt(i);
                        i--;
                        continue;
                    }
                    if (taskData.startFrame == frame)
                    {
                        actionViewTasks[i].BeginExecute(frame);
                    }
                    if (taskData.startFrame <= frame
                        && taskData.endFrame >= frame)
                    {
                        if (i >= actionViewTasks.Count)
                        {
                            BattleLog.LogError("The SkillAbility is out range for " + i + "  actionViewTask " + actionViewTasks.Count);
                            continue;
                        }
                        actionViewTasks[i].DoExecute(frame);
                    }
                    if (taskData.endFrame <= frame)
                    {
                        actionViewTasks[i].EndExecute();
                        actionViewTasks.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        /// <summary>
        /// 是否强制中断技能
        /// </summary>
        /// <param name="immediate"></param>
        public void BreakActionsImmediate(System.Action _onBreak = null)
        {
            onBreak = _onBreak;
            ToBreakActions();
        }

        public void BreakActions(SKILL_BREAK_CAUSE bREAK_CAUSE, bool immediate = true, System.Action _onBreak = null, bool isBreakFromSkill = false)
        {
            onBreak = _onBreak;
            if (immediate)
            {
                ToBreakActions();
            }
            if (SkillFlag == null)
            {
                if (isBreakFromSkill)
                {
                    //如果是从技能打断，如果当前技能处于不可打断状态，会把传入的技能作为下次释放的技能
                    onBreak?.Invoke();
                    EndExecute();
                }
                return;
            }
            if (SkillFlag.flagCause.HasFlag(bREAK_CAUSE))
            {
                ToBreakActions();
            }
            else
            {
                breakCause = bREAK_CAUSE;
                onBreak?.Invoke();
                EndExecute();
            }
        }

        private void ToBreakActions()
        {
            if (OwnerEntity == null
                || SkillAbility == null)
            {
                onBreak?.Invoke();
                EndExecute();
                return;
            }
            for (int i = 0, len = actionLogicTasks.Count; i < len; i++)
            {
                if (actionLogicTasks[i] != null
                    && actionLogicTasks[i].taskInitData != null)
                    actionLogicTasks[i].BreakExecute(curFrame);
            }
            actionLogicTasks.Clear();
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                for (int i = 0, len = actionViewTasks.Count; i < len; i++)
                {
                    if (actionViewTasks[i] != null
                        && actionViewTasks[i].taskInitData != null)
                        actionViewTasks[i].BreakExecute(curFrame);
                }
                actionViewTasks.Clear();
            }
            onBreak?.Invoke();
            breakCause = SKILL_BREAK_CAUSE.None;
#if !SERVER
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                if (SkillAbility.SkillBaseConfig.skillType == (int)SKILL_TYPE.SPSKL
                    && OwnerEntity != null)
                {
                    BattleManager.Instance.SPSSkillMgr.FinishSPSkillPreEffect(OwnerEntity.UID, true);
                }
            }
#endif
            EndExecute();
        }

        public void PauseActions()
        {
            for (int i = 0, len = actionLogicTasks.Count; i < len; i++)
            {
                actionLogicTasks[i].PauseExecute(curFrame);
            }
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                for (int i = 0, len = actionViewTasks.Count; i < len; i++)
                {
                    actionViewTasks[i].PauseExecute(curFrame);
                }
            }
        }

        public bool CanSlide()
        {
            return SkillAbility.SkillConfigObject.SlideSkill;
        }

        public float SlideSpeed()
        {
            return SkillAbility.SkillConfigObject.SlideSpeed;
        }

        public Vector3 GetWarningPoint(int index)
        {
            if (warningPoints == null
                || warningPoints.Count <= 0
                || warningPoints.Count < index)
            {
                return Vector3.zero;
            }
            else
            {
                return warningPoints[index];
            }
        }

        int useWarningPointIndex = -1;

        public Vector3 GetWarningPoint()
        {
            useWarningPointIndex += 1;
            if (warningPoints == null
                || warningPoints.Count <= 0
                || warningPoints.Count <= useWarningPointIndex)
            {
                return Vector3.zero;
            }
            else
            {
                return warningPoints[useWarningPointIndex];
            }
        }

        public void ResetTarget(string targetKey)
        {
            targetActorEntity = BattleLogicManager.Instance.BattleData.allActorDic[targetKey];
        }
    }
}