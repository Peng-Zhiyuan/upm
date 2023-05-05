namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;

    public class SkillComboAbility : AbilityEntity
    {
        public int SkillGroupId { get; protected set; }
        public UnitComboRow Config { get; private set; }
        public List<SkillAbility> Skills { get; private set; }
        public SkillAbility CurSkillAbility { get; private set; }
        public int CurSkillIdx { get; private set; }

        public override void Awake(object initData)
        {
            base.Awake(initData);
            Config = initData as UnitComboRow;
            if (Config == null)
            {
                Destroy(this);
            }
            CurSkillIdx = -1;
            Skills = new List<SkillAbility>();
            SetSkills();
        }

        private void SetSkills()
        {
            for (int i = 0; i < Config.Segments.Length; i++)
            {
                if (Config.Segments[i] == 0)
                    continue;
                if (OwnerEntity.SkillSlots.ContainsKey((uint)Config.Segments[i]))
                {
                    Skills.Add(OwnerEntity.SkillSlots[(uint)Config.Segments[i]]);
                }
            }
        }

        public override void ActivateAbility() { }

        public override AbilityExecution CreateExecution()
        {
            var execution = Entity.CreateWithParent<SkillComboAbilityExecution>(OwnerEntity, this);
            return execution;
        }

        public bool IsFilter()
        {
            return true;
        }

        public bool Finish
        {
            get { return CurSkillIdx >= Skills.Count; }
        }

        public SkillAbility GetCurSkill()
        {
            if (Skills.Count <= 0)
            {
                SetSkills();
            }
            if (Skills.Count <= 0)
            {
                return null;
            }
            CurSkillIdx += 1;
            if (Finish)
            {
                CurSkillIdx = 0;
            }
            return Skills[CurSkillIdx];
        }

        public void ComboSkill(int currentFrame)
        {
            if (Skills.Count <= 0)
            {
                SetSkills();
            }
            CombatActorEntity actorEntity = BattleLogicManager.Instance.BattleData.allActorDic[OwnerEntity.UID];
            CurSkillIdx += 1;
            if (Finish)
            {
                CurSkillIdx = 0;
            }
            actorEntity.ReadySkill = Skills[CurSkillIdx];

            //temp ,not for use again
            if (actorEntity.ReadySkill.SkillConfigObject.PreCheckTrigger)
            {
                Vector3 Center = actorEntity.GetPosition() + Quaternion.Euler(0, actorEntity.GetEulerAngles().y, 0) * actorEntity.ReadySkill.SkillConfigObject.PreCheckCenter;
                bool hasTrigger = false;
                var data = BattleLogicManager.Instance.BattleData.allActorDic.GetEnumerator();
                float triggerRange = actorEntity.ReadySkill.SkillConfigObject.PreCheckRadius * actorEntity.ReadySkill.SkillConfigObject.PreCheckRadius;
                while (data.MoveNext())
                {
                    if (data.Current.Value.isAtker == actorEntity.isAtker)
                    {
                        continue;
                    }
                    if (MathHelper.DoubleDistanceVect3(Center, data.Current.Value.GetPosition()) < triggerRange)
                    {
                        hasTrigger = true;
                        break;
                    }
                }
                if (!hasTrigger)
                {
                    actorEntity.ReadySkill = actorEntity.GetSkill(actorEntity.ReadySkill.SkillConfigObject.PreCheckFailSkill);
                }
            }
        }
    }
}