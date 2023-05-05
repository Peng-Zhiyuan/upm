namespace BattleEngine.View
{
    using UnityEngine;
    using Logic;

    public sealed class ParabolaDirectionActionTaskData : AbilityTaskData
    {
        public float height = 1;
        public int samplingNum = 10;
        public string res = "";
        public Vector3 startOffset = Vector3.zero;
        public Vector3 endOffset = Vector3.zero;
        public string attachStartPoint = "";
        public string attachEndPoint = "";

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.ParabolaDirection;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.VIEW;
        }

        public override void Init(SkillActionElementItem element)
        {
            base.Init(element);
            ParabolaDirectionActionElement actionElement = element as ParabolaDirectionActionElement;
            samplingNum = actionElement.samplingNum;
            height = actionElement.height;
            if (!string.IsNullOrEmpty(actionElement.res))
            {
                res = actionElement.res.Split('.')[0];
            }
            startOffset = actionElement.startOffset;
            endOffset = actionElement.endOffset;
            attachStartPoint = actionElement.attachStartPoint;
            attachEndPoint = actionElement.attachEndPoint;
        }
    }

    public sealed class SkillParabolaDirectionActionViewTask : AbilityViewTask
    {
        private ParabolaDirectionActionTaskData _taskData;
        public ParabolaDirectionActionTaskData TaskData
        {
            get
            {
                if (_taskData == null)
                {
                    _taskData = taskInitData as ParabolaDirectionActionTaskData;
                }
                return _taskData;
            }
        }

        public override void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            CreatEffect(SkillAbilityExecution.OwnerEntity);
        }

        public override void DoExecute(int frameIdx)
        {
            base.DoExecute(frameIdx);
            if (ctr != null)
            {
                if (lastTargetId != SkillAbilityExecution.targetActorEntity.UID)
                {
                    lastTargetId = SkillAbilityExecution.targetActorEntity.UID;
                    Creature actorTarget = BattleManager.Instance.ActorMgr.GetActor(lastTargetId);
                    Transform actorTargetTrans = actorTarget.transform;
                    endTrans = GameObjectHelper.FindChild(actorTargetTrans, TaskData.attachEndPoint);
                }
                ctr.beignPoint = startTrans.position + TaskData.startOffset;
                ctr.endPoint = endTrans.position + TaskData.endOffset;
                ctr.RefreshParabola();
            }
        }

        ParabolaCtr ctr;
        Transform endTrans;
        Transform startTrans;
        string lastTargetId = "";

        public async void CreatEffect(CombatActorEntity attachEntity)
        {
            if (attachEntity == null
                || !BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                return;
            }
            Creature actor = BattleManager.Instance.ActorMgr.GetActor(attachEntity.UID);
            Transform actorTrans = actor.transform;
            startTrans = GameObjectHelper.FindChild(actorTrans, TaskData.attachStartPoint);
            GameObject fx = await BattleResManager.Instance.CreatorFx(TaskData.res);
            if (fx == null)
            {
                return;
            }
            fx.transform.localScale = Vector3.one;
            fx.transform.position = startTrans.position + TaskData.startOffset;
            fx.transform.localRotation = Quaternion.Euler(Vector3.zero);
            ctr = fx.GetComponent<ParabolaCtr>();
            ctr.height = TaskData.height;
            lastTargetId = SkillAbilityExecution.targetActorEntity.UID;
            Creature actorTarget = BattleManager.Instance.ActorMgr.GetActor(lastTargetId);
            Transform actorTargetTrans = actorTarget.transform;
            endTrans = GameObjectHelper.FindChild(actorTargetTrans, TaskData.attachEndPoint);
            ctr.beignPoint = startTrans.position + TaskData.startOffset;
            ctr.endPoint = endTrans.position + TaskData.endOffset;
            ctr.InitParabola();
            ctr.RefreshParabola();
            ctr.delegateAnimFinish = () => { };
        }

        public override void EndExecute()
        {
            if (ctr != null)
            {
                GameObject.DestroyImmediate(ctr.gameObject);
            }
            base.EndExecute();
        }
    }
}