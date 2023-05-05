namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;

    public partial class SkillAbility : AbilityEntity
    {
        private List<string> listenEventLst = new List<string>();

        private void RegisterActiviteBuffEvent()
        {
            listenEventLst.Clear();
            for (int i = 0; i < buffEffectLst.Count; i++)
            {
                switch ((BUFF_TRIGGER_TYPE)buffEffectLst[i].buffTrigger.Trigger)
                {
                    case BUFF_TRIGGER_TYPE.Activity_SELF_HP_RANGE:
                        if (!listenEventLst.Contains("BattleSkillBeginExecution"))
                        {
                            EventManager.Instance.AddListener<SkillAbilityExecution>("BattleSkillBeginExecution", OnSpellSkillPoint);
                            listenEventLst.Add("BattleSkillBeginExecution");
                        }
                        break;
                    case BUFF_TRIGGER_TYPE.Activity_TARGET_HP_RANGE:
                    case BUFF_TRIGGER_TYPE.Activity_CAUSE_CRIT:
                    case BUFF_TRIGGER_TYPE.Activity_HAVE_BUFF_ID:
                    case BUFF_TRIGGER_TYPE.Activity_HAVE_BUFF_CONTROL_TYPE:
                        if (!listenEventLst.Contains("PostCauseDamage"))
                        {
                            OwnerEntity.ListenActionPoint(ACTION_POINT_TYPE.PostCauseDamage, OnPostCauseDamage);
                            OwnerEntity.ListenActionPoint(ACTION_POINT_TYPE.PostGiveCure, OnPostCauseDamage);
                            listenEventLst.Add("PostCauseDamage");
                        }
                        break;
                }
            }
        }

        private void UnRegisterActiviteBuffEvent()
        {
            for (int i = 0; i < buffEffectLst.Count; i++)
            {
                switch ((BUFF_TRIGGER_TYPE)buffEffectLst[i].buffTrigger.Trigger)
                {
                    case BUFF_TRIGGER_TYPE.Activity_SELF_HP_RANGE:
                        if (listenEventLst.Contains("BattleSkillBeginExecution"))
                        {
                            EventManager.Instance.RemoveListener<SkillAbilityExecution>("BattleSkillBeginExecution", OnSpellSkillPoint);
                            listenEventLst.Remove("BattleSkillBeginExecution");
                        }
                        break;
                    case BUFF_TRIGGER_TYPE.Activity_TARGET_HP_RANGE:
                    case BUFF_TRIGGER_TYPE.Activity_CAUSE_CRIT:
                    case BUFF_TRIGGER_TYPE.Activity_HAVE_BUFF_ID:
                    case BUFF_TRIGGER_TYPE.Activity_HAVE_BUFF_CONTROL_TYPE:
                        if (listenEventLst.Contains("PostCauseDamage"))
                        {
                            OwnerEntity.UnListenActionPoint(ACTION_POINT_TYPE.PostCauseDamage, OnPostCauseDamage);
                            OwnerEntity.UnListenActionPoint(ACTION_POINT_TYPE.PostGiveCure, OnPostCauseDamage);
                            listenEventLst.Remove("PostCauseDamage");
                        }
                        break;
                }
            }
        }

        //使用该技能且自身生命介于X~Y%时触发
        public void OnSpellSkillPoint(SkillAbilityExecution combatAction)
        {
            if (combatAction == null
                || OwnerEntity == null
                || OwnerEntity.IsDead
                || combatAction.OwnerEntity.UID != OwnerEntity.UID
                || combatAction.SkillAbility.SkillBaseConfig.SkillID != SkillBaseConfig.SkillID)
            {
                return;
            }
            for (int i = 0; i < buffEffectLst.Count; i++)
            {
                if (buffEffectLst[i].buffTrigger.Trigger == (int)BUFF_TRIGGER_TYPE.Activity_SELF_HP_RANGE)
                {
                    int HPPer = Mathf.FloorToInt(OwnerEntity.CurrentHealth.Percent() * 1000);
                    if (HPPer >= buffEffectLst[i].buffTrigger.trigRate1
                        && HPPer <= buffEffectLst[i].buffTrigger.trigRate2
                        && isTriggerChance(buffEffectLst[i].buffTrigger))
                    {
                        TriggerAddBuff(buffEffectLst[i].buffTrigger);
                    }
                }
            }
        }

        /// 使用该技能命中生命介于X~Y%的敌人时
        /// 使用该技能造成暴击时触发
        /// 使用该技能命中有指定ID buff的敌人时触发
        /// 使用该技能命中有指定类型buff的敌人时触发
        public void OnPostCauseDamage(ActionExecution combatAction)
        {
            if (combatAction == null
                || OwnerEntity == null
                || OwnerEntity.IsDead)
            {
                return;
            }
            CombatActorEntity targetEntity = null;
            if (combatAction is DamageAction)
            {
                DamageAction damageAction = combatAction as DamageAction;
                if (damageAction.DamageSkillRow == null
                    || damageAction.DamageSkillRow.SkillID != SkillBaseConfig.SkillID
                    || damageAction.Creator.UID != OwnerEntity.UID)
                {
                    return;
                }
                targetEntity = damageAction.Target;
            }
            else if (combatAction is CureAction)
            {
                CureAction curAction = combatAction as CureAction;
                if (curAction.cureSkillRow == null
                    || curAction.cureSkillRow.SkillID != SkillBaseConfig.SkillID
                    || curAction.Creator.UID != OwnerEntity.UID)
                {
                    return;
                }
                targetEntity = curAction.Target;
            }
            for (int i = 0; i < buffEffectLst.Count; i++)
            {
                if (buffEffectLst[i].buffTrigger.Trigger == (int)BUFF_TRIGGER_TYPE.Activity_TARGET_HP_RANGE
                    && targetEntity != null)
                {
                    int HPPer = Mathf.FloorToInt(targetEntity.CurrentHealth.Percent() * 1000);
                    if (HPPer >= buffEffectLst[i].buffTrigger.trigRate1
                        && HPPer <= buffEffectLst[i].buffTrigger.trigRate2
                        && isTriggerChance(buffEffectLst[i].buffTrigger))
                    {
                        TriggerAddBuff(buffEffectLst[i].buffTrigger, targetEntity);
                    }
                }
                else if (buffEffectLst[i].buffTrigger.Trigger == (int)BUFF_TRIGGER_TYPE.Activity_CAUSE_CRIT
                         && isTriggerChance(buffEffectLst[i].buffTrigger))
                {
                    if (combatAction is DamageAction
                        && (combatAction as DamageAction).behitData.HasState(HitType.Crit))
                        TriggerAddBuff(buffEffectLst[i].buffTrigger, targetEntity);
                }
                else if (buffEffectLst[i].buffTrigger.Trigger == (int)BUFF_TRIGGER_TYPE.Activity_HAVE_BUFF_ID
                         && isTriggerChance(buffEffectLst[i].buffTrigger))
                {
                    if (combatAction is DamageAction
                        && IsTriggerTargetBuffID(buffEffectLst[i].buffTrigger, combatAction as DamageAction))
                        TriggerAddBuff(buffEffectLst[i].buffTrigger, targetEntity);
                }
                else if (buffEffectLst[i].buffTrigger.Trigger == (int)BUFF_TRIGGER_TYPE.Activity_HAVE_BUFF_CONTROL_TYPE
                         && isTriggerChance(buffEffectLst[i].buffTrigger))
                {
                    if (combatAction is DamageAction
                        && IsTriggerTargetBuffControlType(buffEffectLst[i].buffTrigger, combatAction as DamageAction))
                        TriggerAddBuff(buffEffectLst[i].buffTrigger, targetEntity);
                }
            }
        }

        private bool isTriggerChance(BuffTrigger buffTrigger)
        {
            if (buffTrigger == null
                || OwnerEntity.IsDead)
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

        private bool IsTriggerTargetBuffID(BuffTrigger buffTrigger, DamageAction action)
        {
            CombatActorEntity targetEntity = action.Target;
            if (targetEntity == null
                || targetEntity.UID == OwnerEntity.UID)
            {
                return false;
            }
            return targetEntity.HasBuff(buffTrigger.trigRate1);
        }

        private bool IsTriggerTargetBuffControlType(BuffTrigger buffTrigger, DamageAction action)
        {
            CombatActorEntity targetEntity = action.Target;
            if (targetEntity == null
                || targetEntity.UID == OwnerEntity.UID)
            {
                return false;
            }
            return targetEntity.HasBuffControlType(buffTrigger.trigRate1);
        }

        private void TriggerAddBuff(BuffTrigger buffTrigger, CombatActorEntity currentTargetEntity = null)
        {
            if (buffTrigger.buffTarget == (int)BUFF_EFFECT_TARGET.ALL_ENEMY)
            {
                List<CombatActorEntity> allEnemy = BattleLogicManager.Instance.BattleData.GetEnemyLst(OwnerEntity);
                for (int j = 0; j < allEnemy.Count; j++)
                {
                    if (allEnemy[j].IsCantSelect)
                    {
                        continue;
                    }
                    ApplyBuffAbility(buffTrigger, allEnemy[j]);
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
                    ApplyBuffAbility(buffTrigger, SelfTeam[j]);
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
                    row = SelfTeam[j].battleItemInfo.GetHeroRow();
                    if (row != null
                        && row.Element == manaType)
                    {
                        ApplyBuffAbility(buffTrigger, SelfTeam[j]);
                    }
                }
            }
            else
            {
                if (currentTargetEntity == null
                    && !string.IsNullOrEmpty(OwnerEntity.targetKey))
                {
                    currentTargetEntity = BattleLogicManager.Instance.BattleData.GetActorEntity(OwnerEntity.targetKey);
                }
                CombatActorEntity buffEntity = BattleUtil.GetBuffTarget(OwnerEntity, currentTargetEntity, (BUFF_EFFECT_TARGET)buffTrigger.buffTarget);
                if (buffEntity == null)
                {
                    BattleLog.LogError("Cant find the Buff Entity");
                    return;
                }
                ApplyBuffAbility(buffTrigger, buffEntity);
            }
        }

        private void ApplyBuffAbility(BuffTrigger buffTrigger, CombatActorEntity targetEntity)
        {
            if (targetEntity.IsCantSelect)
            {
                BattleLog.LogWarning("The actor " + targetEntity.ConfigID + "  cant add buff");
                return;
            }
            if (targetEntity.AssignEffectActionAbility.TryCreateAction(out var action))
            {
                action.Creator = OwnerEntity;
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
                BattleLog.LogWarning("[PASSIVE_SKILL] " + OwnerEntity.ConfigID + " Passive Skill Trigger Buff " + buffTrigger.buffId + " " + LocalizationManager.Stuff.GetText(row.Name));
            }
        }
    }
}