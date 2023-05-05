namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class TargetTogetherTaskData : AbilityTaskData
    {
        public SKILL_TOGETHER_TYPE togetherType = SKILL_TOGETHER_TYPE.FREE;

        public Vector3 offsetPos;
        public Vector3 offsetRot;
        public float Hover_High = 0.0f;
        public int Hover_Up_Time = 0;
        public AnimationCurve Hover_verCureUp;
        public int Hover_Time = 0;
        public int Hover_Down_Time = 0;
        public AnimationCurve Hover_verCureDown;

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.TargetTogether;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.LOGIC;
        }

        public override void Init(SkillActionElementItem element)
        {
            base.Init(element);
            TargetTogetherActionElement actionElement = element as TargetTogetherActionElement;
            this.offsetPos = actionElement.offset;
            this.offsetRot = actionElement.angleOffset;
            this.togetherType = actionElement.togetherType;
            this.Hover_High = actionElement.Hover_High;
            this.Hover_Up_Time = actionElement.Hover_Up_Time;
            this.Hover_verCureUp = actionElement.Hover_verCureUp;
            this.Hover_Time = actionElement.Hover_Time;
            this.Hover_Down_Time = actionElement.Hover_Down_Time;
            this.Hover_verCureDown = actionElement.Hover_verCureDown;
        }
    }

    public sealed class SkillTargetTogetherTask : AbilityTask
    {
        private TargetTogetherTaskData _taskData;
        public TargetTogetherTaskData TaskData
        {
            get
            {
                if (_taskData == null)
                {
                    _taskData = taskInitData as TargetTogetherTaskData;
                }
                return _taskData;
            }
        }

        private CombatActorEntity Owner;
        private List<CombatActorEntity> targetLst = new List<CombatActorEntity>();
        private int frameOffset = 0;
        private Dictionary<string, List<Vector3>> moveDic = new Dictionary<string, List<Vector3>>();
        private float delta = 0.0f;

        private float Check_Y_Value = 0.0f;

        private Vector3 targetPos;

        public override void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            Owner = SkillAbilityExecution.OwnerEntity;
            targetPos = Owner.GetPosition() + Quaternion.Euler(Owner.GetEulerAngles() + TaskData.offsetRot) * TaskData.offsetPos;
            Check_Y_Value = Owner.GetPosition().y;
            delta = SkillAbilityExecution.SkillAbility.SkillFrameRate;
            moveDic.Clear();
            targetLst.Clear();
            if (SkillAbilityExecution.LastHitEntitysDic.Count > 0)
            {
                targetLst.AddRange(SkillAbilityExecution.LastHitEntitysDic[0]);
            }
            else
            {
                targetLst.AddRange(SkillAbilityExecution.AllTargetActorEntity);
            }
            if (targetLst.Count == 0)
            {
                targetLst.Add(BattleLogicManager.Instance.BattleData.GetActorEntity(Owner.targetKey));
            }
            for (int i = 0; i < targetLst.Count; i++)
            {
                if (targetLst[i] == null
                    || !BattleControlUtil.RefreshBuffControlState(targetLst[i], ACTION_CONTROL_TYPE.control_9)) //|| targetLst[i].IsCantSelect
                {
                    targetLst.RemoveAt(i);
                    i--;
                    continue;
                }
                CombatActorEntity targetEntity = targetLst[i];
                targetEntity.BreakCurentSkillImmediate();
                targetEntity.AttachBuff((int)BUFF_COMMON_CONFIG_ID.AIR_BORNE);
                targetEntity.SetActionState(ACTOR_ACTION_STATE.Floating);
                targetEntity.SetLifeState(ACTOR_LIFE_STATE.StopLogic);
                List<Vector3> moveList = new List<Vector3>();
                Vector3 moveDelta = Vector3.zero;
                Vector3 dir = (targetPos - targetEntity.GetPositionXZ()).normalized;
                float distance = Vector3.Distance(targetPos, targetEntity.GetPosition());
                if (distance - Owner.GetAiLocRadiu() - targetEntity.GetAiLocRadiu() < 2)
                {
                    distance = 2;
                }
                else
                {
                    distance -= Owner.GetAiLocRadiu() + targetEntity.GetAiLocRadiu();
                }
                if (TaskData.togetherType == SKILL_TOGETHER_TYPE.FREE)
                {
                    frameOffset = TaskData.endFrame - TaskData.startFrame;
                    moveDelta = dir * (float)distance / (float)frameOffset;
                    for (int j = 0; j < frameOffset; j++)
                    {
                        moveList.Add(moveDelta);
                    }
                    moveDic[targetEntity.UID] = moveList;
                }
                else if (TaskData.togetherType == SKILL_TOGETHER_TYPE.HOVER)
                {
                    int frameOffset = TaskData.Hover_Up_Time;
                    moveDelta = dir * (float)distance / (float)frameOffset;
                    for (int j = 0; j <= TaskData.Hover_Up_Time; j++)
                    {
                        float prehigh = TaskData.Hover_verCureUp.Evaluate(j * 1f / frameOffset) * TaskData.Hover_High;
                        float high = TaskData.Hover_verCureUp.Evaluate((j + 1) * 1f / frameOffset) * TaskData.Hover_High;
                        moveDelta.y = (high - prehigh);
                        moveList.Add(moveDelta);
                    }
                    for (int j = 0; j <= TaskData.Hover_Time; j++)
                    {
                        moveList.Add(Vector3.zero);
                    }
                    moveDelta = Vector3.zero;
                    frameOffset = TaskData.Hover_Down_Time;
                    for (int j = 0; j <= TaskData.Hover_Down_Time; j++)
                    {
                        float prehigh = TaskData.Hover_verCureDown.Evaluate(j * 1f / frameOffset) * TaskData.Hover_High;
                        float high = TaskData.Hover_verCureDown.Evaluate((j + 1) * 1f / frameOffset) * TaskData.Hover_High;
                        moveDelta.y = (high - prehigh);
                        moveList.Add(moveDelta);
                    }
                    moveDic[targetEntity.UID] = moveList;
                }
            }
        }

        private int curFrame = 0;
        private List<Vector3> tempLst = new List<Vector3>();

        public override void DoExecute(int frameIdx)
        {
            base.DoExecute(frameIdx);
            curFrame = frameIdx - TaskData.startFrame;
            for (int i = 0; i < targetLst.Count; i++)
            {
                if (!moveDic.ContainsKey(targetLst[i].UID))
                {
                    continue;
                }
                tempLst = moveDic[targetLst[i].UID];
                if (curFrame >= tempLst.Count)
                {
                    continue;
                }
                SkillCtr.SkillActorMove(targetLst[i], tempLst[curFrame], delta, Check_Y_Value);
#if !SERVER
                if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
                {
                    Creature targetActorCreature = BattleManager.Instance.ActorMgr.GetActor(targetLst[i].UID);
                    if (TaskData.togetherType == SKILL_TOGETHER_TYPE.FREE)
                    {
                        float floatTime = targetActorCreature.GetAnimClipTime("float");
                        if (floatTime > 0)
                        {
                            float speed = floatTime * BattleLogicDefine.LogicSecFrame / (TaskData.endFrame - TaskData.startFrame);
                            targetActorCreature.SetAnimSpeed(speed);
                        }
                        else
                        {
                            targetActorCreature.SetAnimSpeed(1.0f);
                        }
                        targetActorCreature.PlayAnim("float");
                    }
                    else if (TaskData.togetherType == SKILL_TOGETHER_TYPE.HOVER)
                    {
                        if (curFrame == 0)
                        {
                            targetActorCreature.PlayAnim("flyhit1");
                        }
                        else if (curFrame == TaskData.Hover_Up_Time + TaskData.Hover_Time)
                        {
                            targetActorCreature.PlayAnim("flyhit3");
                        }
                    }
                }
#endif
            }
        }

        public override void BreakExecute(int frameIdx)
        {
            for (int i = 0; i < targetLst.Count; i++)
            {
                TogetFinish(targetLst[i], true);
            }
            base.BreakExecute(frameIdx);
        }

        public override void EndExecute()
        {
            for (int i = 0; i < targetLst.Count; i++)
            {
                TogetFinish(targetLst[i], true);
            }
            base.EndExecute();
        }

        private void TogetFinish(CombatActorEntity targetEntity, bool isBreak = false)
        {
            if (targetEntity == null)
            {
                return;
            }
            Vector3 pos = targetEntity.GetPositionXZ();
            if (!BattleUtil.IsInMap(pos))
            {
                pos.y = BattleUtil.GetWalkablePos(targetEntity.GetPositionXZ()).y;
            }
            SkillCtr.SkillSetActorMove(targetEntity, pos);
            targetEntity.OnBuffRemove((int)BUFF_COMMON_CONFIG_ID.AIR_BORNE);
            targetEntity.AttachBuff((int)BUFF_COMMON_CONFIG_ID.Client_Effect_Control);
            float durantion = Mathf.Max(0, 2.0f - TaskData.Hover_Down_Time * 1.0f / SkillAbilityExecution.SkillAbility.SkillConfigObject.fps);
            LogicFrameTimerMgr.Instance.ScheduleTimer(durantion * BattleLogicDefine.LogicSecTime, delegate
                            {
                                if (!targetEntity.IsDead)
                                {
                                    targetEntity.SetLifeState(ACTOR_LIFE_STATE.Alive);
                                    targetEntity.SetActionState(ACTOR_ACTION_STATE.Idle);
                                }
                                targetEntity.OnBuffRemove((int)BUFF_COMMON_CONFIG_ID.Client_Effect_Control);
                            }
            );
#if !SERVER
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                Creature targetCreature = BattleManager.Instance.ActorMgr.GetActor(targetEntity.UID);
                if (!targetCreature.mData.IsDead)
                {
                    targetCreature.SetAnimSpeed(1.0f);
                    targetCreature.PlayAnim("flyhit4");   
                }
                else
                {
                    targetCreature.isNeedPlayDeadAnim = false;
                }
            }
#endif
        }
    }
}