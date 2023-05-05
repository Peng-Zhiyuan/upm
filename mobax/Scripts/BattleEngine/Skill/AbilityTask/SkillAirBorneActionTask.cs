using System.Linq;

namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;

#if !SERVER
    //using DG.Tweening;
#endif

    public sealed class AirBorneActionTaskData : AbilityTaskData
    {
        public SKILL_AIRBORNE_TYPE FlyType = SKILL_AIRBORNE_TYPE.FREE;
        public float Free_Distance = 0.0f;
        public AnimationCurve Free_horCure;
        public float Free_High = 0.0f;
        public AnimationCurve Free_verCure;

        public float Hover_High = 0.0f;
        public int Hover_Up_Time = 0;
        public AnimationCurve Hover_verCureUp;
        public int Hover_Time = 0;
        public int Hover_Down_Time = 0;
        public AnimationCurve Hover_verCureDown;

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.AirBorne;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.LOGIC;
        }

        public override void Init(SkillActionElementItem element)
        {
            base.Init(element);
            AirBorneElement actionElement = element as AirBorneElement;
            this.FlyType = actionElement.FlyType;
            this.Free_Distance = actionElement.Free_Distance;
            this.Free_horCure = actionElement.Free_horCure;
            this.Free_High = actionElement.Free_High;
            this.Free_verCure = actionElement.Free_verCure;
            this.Hover_High = actionElement.Hover_High;
            this.Hover_Up_Time = actionElement.Hover_Up_Time;
            this.Hover_verCureUp = actionElement.Hover_verCureUp;
            this.Hover_Time = actionElement.Hover_Time;
            this.Hover_Down_Time = actionElement.Hover_Down_Time;
            this.Hover_verCureDown = actionElement.Hover_verCureDown;
        }
    }

    public sealed class SkillAirBorneActionTask : AbilityTask
    {
        private AirBorneActionTaskData _taskData;
        public AirBorneActionTaskData TaskData
        {
            get
            {
                if (_taskData == null)
                {
                    _taskData = taskInitData as AirBorneActionTaskData;
                }
                return _taskData;
            }
        }
        private CombatActorEntity owner;
        private List<CombatActorEntity> targetLst = new List<CombatActorEntity>();
        private int frameOffset = 0;
        private Dictionary<string, List<Vector3>> moveDic = new Dictionary<string, List<Vector3>>();
        private Dictionary<string, Vector3> endMoveDic = new Dictionary<string, Vector3>();
        private float delta = 0.0f;
        private float Check_Y_Value = 0.0f;

        public override void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            delta = SkillAbilityExecution.SkillAbility.SkillFrameRate;
            owner = SkillAbilityExecution.OwnerEntity;
            Check_Y_Value = owner.GetPosition().y;
            moveDic.Clear();
            targetLst.Clear();
            if (SkillAbilityExecution.LastHitEntitysDic.Count > 0
                && SkillAbilityExecution.LastHitEntitysDic.ContainsKey(0))
            {
                //必须第一下命中才能加入
                targetLst.AddRange(SkillAbilityExecution.LastHitEntitysDic[0]);
            }
            else
            {
                targetLst.AddRange(SkillAbilityExecution.AllTargetActorEntity);
            }
            if (targetLst.Count == 0)
            {
                targetLst.Add(BattleLogicManager.Instance.BattleData.GetActorEntity(owner.targetKey));
            }
            frameOffset = TaskData.endFrame - TaskData.startFrame;
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
                if (TaskData.FlyType == SKILL_AIRBORNE_TYPE.FREE)
                {
                    Vector3 moveDelta = Vector3.zero;
                    Vector3 endPos = targetEntity.GetPosition();
                    Vector3 dir = (targetEntity.GetPositionXZ() - owner.GetPositionXZ()).normalized;
                    float prewidth, width, prehigh, high = 0;
                    for (int j = 0; j < frameOffset; j++)
                    {
                        prewidth = width = prehigh = high = 0;
                        if (TaskData.Free_horCure != null)
                        {
                            prewidth = TaskData.Free_horCure.Evaluate(j * 1f / frameOffset) * TaskData.Free_Distance;
                            width = TaskData.Free_horCure.Evaluate((j + 1) * 1f / frameOffset) * TaskData.Free_Distance;
                        }
                        if (TaskData.Free_verCure != null)
                        {
                            prehigh = TaskData.Free_verCure.Evaluate(j * 1f / frameOffset) * TaskData.Free_High;
                            high = TaskData.Free_verCure.Evaluate((j + 1) * 1f / frameOffset) * TaskData.Free_High;
                        }
                        moveDelta = dir * (width - prewidth);
                        moveDelta.y = (high - prehigh);
                        moveList.Add(moveDelta);
                        endPos += moveDelta;
                    }
                    moveDic[targetEntity.UID] = moveList;
                    endMoveDic[targetEntity.UID] = endPos;
                }
                else if (TaskData.FlyType == SKILL_AIRBORNE_TYPE.HOVER)
                {
                    Vector3 moveDelta = Vector3.zero;
                    Vector3 endPos = targetEntity.GetPosition();
                    int frameOffset = TaskData.Hover_Up_Time;
                    float prehigh, high = 0;
                    for (int j = 0; j < frameOffset; j++)
                    {
                        prehigh = high = 0;
                        if (TaskData.Hover_verCureUp != null)
                        {
                            prehigh = TaskData.Hover_verCureUp.Evaluate(j * 1f / TaskData.Hover_Up_Time) * TaskData.Hover_High;
                            high = TaskData.Hover_verCureUp.Evaluate((j + 1) * 1f / TaskData.Hover_Up_Time) * TaskData.Hover_High;
                        }
                        moveDelta.y = (high - prehigh);
                        moveList.Add(moveDelta);
                        endPos += moveDelta;
                    }
                    for (int j = 0; j <= TaskData.Hover_Time; j++)
                    {
                        moveList.Add(Vector3.zero);
                    }
                    frameOffset = TaskData.Hover_Down_Time;
                    for (int j = 0; j < frameOffset; j++)
                    {
                        prehigh = high = 0;
                        if (TaskData.Hover_verCureDown != null)
                        {
                            prehigh = TaskData.Hover_verCureDown.Evaluate(j * 1f / TaskData.Hover_Down_Time) * TaskData.Hover_High;
                            high = TaskData.Hover_verCureDown.Evaluate((j + 1) * 1f / TaskData.Hover_Down_Time) * TaskData.Hover_High;
                        }
                        moveDelta.y = (high - prehigh);
                        moveList.Add(moveDelta);
                        endPos += moveDelta;
                    }
                    moveDic[targetEntity.UID] = moveList;
                    endMoveDic[targetEntity.UID] = endPos;
                }
            }
        }

        private void FlyFinish(CombatActorEntity targetEntity, bool isBreak = false)
        {
            if (targetEntity == null)
            {
                return;
            }
            if (endMoveDic.ContainsKey(targetEntity.UID))
            {
                Vector3 endPos = endMoveDic[targetEntity.UID];
                if (!BattleUtil.IsInMap(endPos))
                {
                    endPos.y = BattleUtil.GetWalkablePos(endPos).y;
                }
                SkillCtr.SkillSetActorMove(targetEntity, endPos);
            }
            targetEntity.OnBuffRemove((int)BUFF_COMMON_CONFIG_ID.AIR_BORNE);
            targetEntity.AttachBuff((int)BUFF_COMMON_CONFIG_ID.Client_Effect_Control);
            float durantion = Mathf.Max(0, 2.0f - TaskData.Hover_Down_Time * 1.0f / SkillAbilityExecution.SkillAbility.SkillConfigObject.fps);
            LogicFrameTimerMgr.Instance.ScheduleTimer(durantion, delegate
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

        private int curFrame = 0;
        private List<Vector3> tempLst = new List<Vector3>();

        public override void DoExecute(int frameIdx)
        {
            base.DoExecute(frameIdx);
            curFrame = frameIdx - TaskData.startFrame;
            for (int i = 0; i < targetLst.Count; i++)
            {
                if (!moveDic.ContainsKey(targetLst[i].UID))
                    continue;
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
                    if (TaskData.FlyType == SKILL_AIRBORNE_TYPE.FREE)
                    {
                        if (curFrame == 0)
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
                            targetActorCreature.PlayAnim("float", true);
                        }
                    }
                    else if (TaskData.FlyType == SKILL_AIRBORNE_TYPE.HOVER)
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
                FlyFinish(targetLst[i], true);
            }
            base.BreakExecute(frameIdx);
        }

        public override void EndExecute()
        {
            for (int i = 0; i < targetLst.Count; i++)
            {
                FlyFinish(targetLst[i]);
            }
            base.EndExecute();
        }
    }
}