namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;

    public class SkillPassiveAbilityData
    {
        public SkillRow skillRow;
        public BuffTrigger buffTrigger;
    }

    public abstract class SkillPassiveAbility : AbilityEntity
    {
        private CombatActorEntity selfActorEntity;
        public CombatActorEntity SelfActorEntity
        {
            get
            {
                if (selfActorEntity == null)
                {
                    selfActorEntity = GetParent<CombatActorEntity>();
                }
                return selfActorEntity;
            }
        }

        protected SkillRow skillRow;
        protected BuffTrigger buffTrigger;
        private bool isOpen;
        public bool Enable
        {
            set { isOpen = value; }
            get { return isOpen; }
        }

        public SkillRow SkillRowInfo
        {
            get { return skillRow; }
        }

        public GameTimer CooldownTimer { get; set; }
        public float CDTime
        {
            get
            {
                float cdTime = skillRow.Cd * 0.001f / Mathf.Max(0.5f, (1 + OwnerEntity.AttrData.GetValue(AttrType.HASTE) / BattleUtil.GetGlobalK(GlobalK.FAST_8)));
                return cdTime;
            }
        }

        public abstract BUFF_TRIGGER_TYPE GetPassiveType();

        public override async void Awake(object _initData)
        {
            if (_initData == null)
            {
                return;
            }
            SkillPassiveAbilityData data = _initData as SkillPassiveAbilityData;
            skillRow = data.skillRow;
            buffTrigger = data.buffTrigger;
            Enable = true;
            CooldownTimer = new GameTimer(CDTime);
            CooldownTimer.SetTime(CDTime);
            RegisterPassiveEvent();
        }

        protected virtual void RegisterPassiveEvent() { }

        protected virtual void UnRgisterPassiveEvent() { }

        public override void EndAbility()
        {
            UnRgisterPassiveEvent();
            base.EndAbility();
        }

        /// <summary>
        /// 刷新技能CD
        /// </summary>
        public void ResetSkillPassiveCD()
        {
            if (CooldownTimer == null)
            {
                return;
            }
            CooldownTimer.MaxTime = CDTime;
            CooldownTimer.Reset();
        }

        public bool IsStagePassiveSkill()
        {
            return skillRow.skillType == (int)SKILL_TYPE.STAGE_PASSIVE_SKILL;
        }

#region Event
        protected void TriggerAddBuff(CombatActorEntity DefineTarget = null)
        {
            if (DefineTarget != null)
            {
                ApplyBuffAbility(DefineTarget);
            }
            else if (buffTrigger.buffTarget == (int)BUFF_EFFECT_TARGET.ALL_ENEMY)
            {
                List<CombatActorEntity> allEnemy = BattleLogicManager.Instance.BattleData.GetEnemyLst(OwnerEntity);
                for (int j = 0; j < allEnemy.Count; j++)
                {
                    if (allEnemy[j].IsCantSelect)
                    {
                        continue;
                    }
                    ApplyBuffAbility(allEnemy[j]);
                }
            }
            else if (buffTrigger.buffTarget == (int)BUFF_EFFECT_TARGET.ALL_SELF_TEAM)
            {
                List<CombatActorEntity> SelfTeam = BattleLogicManager.Instance.BattleData.GetTeamList(OwnerEntity.TeamKey);
                for (int j = 0; j < SelfTeam.Count; j++)
                {
                    if (SelfTeam[j].IsCantSelect)
                    {
                        continue;
                    }
                    ApplyBuffAbility(SelfTeam[j]);
                }
            }
            else if (buffTrigger.buffTarget >= (int)BUFF_EFFECT_TARGET.ELEMENT_FIRE_SELF_TEAM
                     && buffTrigger.buffTarget <= (int)BUFF_EFFECT_TARGET.ELEMENT_DARK_SELF_TEAM)
            {
                List<CombatActorEntity> SelfTeam = BattleLogicManager.Instance.BattleData.GetTeamList(OwnerEntity.TeamKey);
                HeroRow row = null;
                int manaType = buffTrigger.buffTarget - (int)BUFF_EFFECT_TARGET.ELEMENT_FIRE_SELF_TEAM + 1;
                for (int j = 0; j < SelfTeam.Count; j++)
                {
                    if (SelfTeam[j].IsCantSelect)
                    {
                        continue;
                    }
                    row = SelfTeam[j].battleItemInfo.GetHeroRow();
                    if (row != null
                        && row.Element == manaType)
                    {
                        ApplyBuffAbility(SelfTeam[j]);
                    }
                }
            }
            else
            {
                CombatActorEntity currentTargetEntity = null;
                if (!string.IsNullOrEmpty(SelfActorEntity.targetKey))
                {
                    currentTargetEntity = BattleLogicManager.Instance.BattleData.GetActorEntity(selfActorEntity.targetKey);
                }
                CombatActorEntity buffEntity = BattleUtil.GetBuffTarget(SelfActorEntity, currentTargetEntity, (BUFF_EFFECT_TARGET)buffTrigger.buffTarget);
                if (buffEntity == null)
                {
                    BattleLog.LogError("Cant find the Buff Entity");
                    return;
                }
                ApplyBuffAbility(buffEntity);
            }
            ResetSkillPassiveCD();
        }

        private void ApplyBuffAbility(CombatActorEntity targetEntity)
        {
            if (targetEntity.IsCantSelect)
            {
                BattleLog.LogWarning("The actor " + targetEntity.ConfigID + "  cant add buff");
                return;
            }
            if (targetEntity.AssignEffectActionAbility.TryCreateAction(out var action))
            {
                action.Creator = SelfActorEntity;
                action.Target = targetEntity;
                BuffRow row = BuffUtil.GetBuffRow(buffTrigger.buffId, buffTrigger.buffLv);
                if (row == null)
                {
                    BattleLog.LogError("Cant find buffid " + buffTrigger.buffId + "  buffLv " + buffTrigger.buffLv);
                }
                else
                {
                    action.Effect = new AddBuffEffect(row, BUFF_EFFECT_TARGET.CURRENT_TARGET, buffTrigger);
                }
                action.ApplyAssignEffect();
                BattleLog.LogWarning("[PASSIVE_SKILL]  Passive Skill Trigger Buff " + buffTrigger.buffId + " " + LocalizationManager.Stuff.GetText(row.Name));
            }
        }

        protected bool isTriggerChance()
        {
            if (buffTrigger == null
                || SelfActorEntity.IsDead)
            {
                return false;
            }
            if (buffTrigger.Chance >= 1000)
            {
                return true;
            }
            int per = BattleLogicManager.Instance.Rand.RandomVaule(0, 1000);
            if (per <= buffTrigger.Chance)
            {
                return true;
            }
            return false;
        }
#endregion
    }
}