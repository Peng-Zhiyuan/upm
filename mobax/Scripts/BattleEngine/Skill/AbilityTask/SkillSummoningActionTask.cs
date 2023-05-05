namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// 技能召唤操作
    /// 目前不用
    /// </summary>
    public sealed class SummoningActionTaskData : AbilityTaskData
    {
        public int summoningActorId = 1;
        public int summoningActorLv = 1;
        public List<Vector3> summoningActorPos = new List<Vector3>();
        public int summoningActorLifeTime = -1;

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.Summoning;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.LOGIC;
        }

        public override void Init(SkillActionElementItem element)
        {
            base.Init(element);
            SummoningActionElement actionElement = element as SummoningActionElement;
            summoningActorId = actionElement.summoningActorId;
            summoningActorLifeTime = actionElement.summoningActorLifeTime;
            summoningActorPos = actionElement.summoningActorPos;
            summoningActorLv = actionElement.summoningActorLv;
        }
    }

    public sealed class SkillSummoningActionTask : AbilityTask
    {
        private SummoningActionTaskData _taskData;
        public SummoningActionTaskData TaskData
        {
            get
            {
                if (_taskData == null)
                {
                    _taskData = taskInitData as SummoningActionTaskData;
                }
                return _taskData;
            }
        }

        public override void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            CombatActorEntity actorTrans = SkillAbilityExecution.OwnerEntity;
            for (int i = 0; i < TaskData.summoningActorPos.Count; i++)
            {
                BattleSummonData data = new BattleSummonData();
                Vector3 pos = actorTrans.GetPosition() + Quaternion.Euler(actorTrans.GetEulerAngles()) * TaskData.summoningActorPos[i];
                data.SetMonsterData(TaskData.summoningActorId, pos, actorTrans.GetForward());
#if !SERVER
                //召唤技能
                //BattleManager.Instance.SummonMgr.CreateActorLst(actorTrans, new List<BattleSummonData>() { data }, () => { });
#endif
            }
        }

        public override void DoExecute(int frameIdx)
        {
            base.DoExecute(frameIdx);
        }

        public override void PauseExecute(int frameIdx)
        {
            base.PauseExecute(frameIdx);
        }

        public override void BreakExecute(int frameIdx)
        {
            base.BreakExecute(frameIdx);
        }

        public override void EndExecute()
        {
            base.EndExecute();
        }
    }
}