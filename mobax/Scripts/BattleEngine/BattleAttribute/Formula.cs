namespace BattleEngine.Logic
{
    using UnityEngine;

    public static class Formula
    {
        public static bool ShowLog = false;

        public static void CacDamage(BehitData hit, CombatActorEntity caster, CombatActorEntity defender, SkillRow skillRow, float skillRatio = 1)
        {
            if (skillRow == null) return;
            float percentPer = 0.001f;
            hit.damage = 0;
            int damage = 0;

            //基础伤害
            int SkillDam1 = caster.AttrData.GetSkillValue(skillRow.SkillID, SKILL_ATTR_TYPE.DAM1) + skillRow.Dam1;
            int SkillDam2 = caster.AttrData.GetSkillValue(skillRow.SkillID, SKILL_ATTR_TYPE.DAM2) + skillRow.Dam2;
            int SkillDam4 = caster.AttrData.GetSkillValue(skillRow.SkillID, SKILL_ATTR_TYPE.DAM4) + skillRow.Dam4;
            int SkillDam5 = caster.AttrData.GetSkillValue(skillRow.SkillID, SKILL_ATTR_TYPE.DAM5) + skillRow.Dam5;
            int SkillDamage = Mathf.FloorToInt(caster.AttrData.GetValue(AttrType.ATK) * SkillDam1 * percentPer + SkillDam2 + caster.CurrentHealth.Value * skillRow.Dam3 * percentPer + (caster.CurrentHealth.MaxValue - caster.CurrentHealth.Value) * SkillDam4 * percentPer + caster.CurrentHealth.MaxValue * SkillDam5 * percentPer);

            //职业克制加成
            float fJobAdd = 1;
            if (caster.isAnitJob(defender.Job))
            {
                fJobAdd += BattleUtil.GetGlobalK(GlobalK.JobDamageAdd_5) * percentPer;
            }

            //元素克制
            float fManaK = 1;
            if (skillRow.damType == (int)DamageType.Mana)
            {
                if (skillRow.Element > 0)
                {
                    fManaK = 1 + (caster.AttrData.GetValue(AttrType.FIREATK - 1 + skillRow.Element) - defender.AttrData.GetValue(AttrType.FIREDEF - 1 + skillRow.Element)) * percentPer;
                    FlagsHelper.Set(ref hit.hitType, (HitType)(1 << (4 + skillRow.Element)));
                }
            }
            if (skillRow.damType == (int)DamageType.Cure)
            {
                //元素加成
                float fManaAdd = 1f;
                float fManaExt = 1f;
                if (skillRow.Element > 0)
                {
                    fManaAdd = Mathf.Max((1 + (caster.AttrData.GetValue(AttrType.FIREATK - 1 + skillRow.Element)) * percentPer), 0);
                    fManaExt = 1 + (caster.AttrData.GetValue(AttrType.FIREDEF - 1 + skillRow.Element)) * percentPer;
                    hit.damage = Mathf.FloorToInt(SkillDamage * fManaAdd * fManaExt);
                }
                FlagsHelper.Set(ref hit.hitType, HitType.Cure);
                return;
            }
            /*if (IsWeak(caster, defender))
            {
                FlagsHelper.Set(ref hit.hitType, HitType.Weak);
            }*/

            //格挡减免
            float fBlockK = 1f;
            if (skillRow.damType == (int)DamageType.Physic)
            {
                int fBlockPercent = Mathf.Max(defender.AttrData.GetValue(AttrType.BLOCK) - caster.AttrData.GetValue(AttrType.HIT), 0);
                int nBlock = Mathf.FloorToInt(1000 * (fBlockPercent / (fBlockPercent + BattleUtil.GetGlobalK(GlobalK.Percent_1000))));
                bool bIsBlock = BattleLogicManager.Instance.Rand.IsRange(nBlock);
                if (bIsBlock)
                {
                    fBlockK = 1 - BattleUtil.GetGlobalK(GlobalK.BLOCK_1) * percentPer;
                    hit.block = 1 - fBlockK;
                    hit.SetState(HitType.Block);
                }
            }

            //防御减免
            float fDefDamage = 1f;
            if (skillRow.damType != (int)DamageType.Real)
            {
                //防御免伤=Max(守方防御-攻方破甲，0)/[Max(守方防御-攻方破甲，0)+系数4]
                float defDamage = Mathf.Max(defender.AttrData.GetValue(AttrType.DEF) - caster.AttrData.GetValue(AttrType.BREAK), 0);
                fDefDamage = 1 - defDamage / (defDamage + BattleUtil.GetGlobalK(GlobalK.DEFENCE_4) + defender.battleItemInfo.lv * BattleUtil.GetGlobalK(GlobalK.DefenderParam_13));
            }

            //暴击
            float fCritK = 1;
            int SkillExtCrit = caster.AttrData.GetSkillValue(skillRow.SkillID, SKILL_ATTR_TYPE.EXTCRIT) + skillRow.extCrit;
            int nCritVal = Mathf.Max(caster.AttrData.GetValue(AttrType.CRIT) + SkillExtCrit - defender.AttrData.GetValue((AttrType.TENA)), 0); //技能暴击 + 0;
            bool isAnti = caster.isAnitJob(defender.Job);
            if (isAnti)
            {
                nCritVal = (nCritVal + (int)BattleUtil.GetGlobalK(GlobalK.JobAddCrit_12));
                FlagsHelper.Set(ref hit.hitType, HitType.Weak);
            }
            bool bCrit = BattleLogicManager.Instance.Rand.IsRange(nCritVal);
            if (bCrit)
            {
                FlagsHelper.Set(ref hit.hitType, HitType.Crit);
                int SkillExtRate = caster.AttrData.GetSkillValue(skillRow.SkillID, SKILL_ATTR_TYPE.EXTRATE) + skillRow.extRate;
                fCritK = 1.25f + Mathf.Max((caster.AttrData.GetValue(AttrType.RATE) + SkillExtRate) * percentPer, 0);
            }

            //额外加成
            float fAdd = 1f;
            if (skillRow.damType == (int)DamageType.Real)
            {
                fAdd = Mathf.Max(1 + caster.AttrData.GetValue(AttrType.HURT) * percentPer, 0);
                FlagsHelper.Set(ref hit.hitType, HitType.Real);
            }
            else
            {
                fAdd = Mathf.Max(1 + (caster.AttrData.GetValue(AttrType.HURT) - defender.AttrData.GetValue(AttrType.PARRY)) * percentPer, 0);
                if (skillRow.skillType == 1)
                {
                    hit.SetState(HitType.Normal);
                }
                else
                {
                    hit.SetState(HitType.Skill);
                }
            }

            //固定减伤
            int fixDamage = 0;
            if (skillRow.damType != (int)DamageType.Real)
            {
                fixDamage = defender.AttrData.GetValue(AttrType.WARD);
            }

            //固定伤害
            int fixDamageAdd = 0;
            fixDamageAdd = caster.AttrData.GetValue(AttrType.IMPAIR);

            //结算伤害
            damage = Mathf.FloorToInt(SkillDamage * fBlockK * fCritK * fDefDamage * fAdd * fJobAdd * fManaK);

            //最终伤害
            damage = Mathf.FloorToInt(Mathf.Max((damage * 1.0f), caster.AttrData.GetValue(AttrType.ATK) * BattleUtil.GetGlobalK(GlobalK.DAMAGEMIN_10) * percentPer)) - fixDamage + fixDamageAdd;
            if (ShowLog)
                BattleLog.LogWarning("总伤害：" + damage + "  基础伤害：" + SkillDamage + "  格挡加成：" + (1 - fBlockK) + "  暴击加成：" + fCritK + "  防御减免：" + (1 - fDefDamage) + "  额外加成：" + fAdd + " 职业克制加成 ：" + fJobAdd);
            hit.damage = Mathf.Max(1, damage);

            //破防后伤害倍率
            if (defender.BreakDefComponent.IsBreak)
            {
                hit.damage = Mathf.Max(1, Mathf.FloorToInt(hit.damage * defender.BreakDefComponent.DamageParam));
                hit.SetState(HitType.BreakDef);
            }

            if (skillRow.skillType == (int) SKILL_TYPE.SPSKL)
            {
                hit.SetState(HitType.SPSKLDamage);
            }
        }

        public static void CacBufferDamage(BehitData hit, CombatActorEntity caster, CombatActorEntity defender, MANA_TYPE manaType, BuffEffect buffEffect)
        {
            float percentPer = 0.001f;
            hit.damage = 0;
            int damage = 0;

            //基础伤害
            int SkillDamage = Mathf.Max(1, Mathf.FloorToInt(caster.AttrData.GetValue(AttrType.ATK) * buffEffect.Param1 * percentPer) + buffEffect.Param2);
            ///治疗加成
            if (buffEffect.Type == (int)BUFF_EFFECT_TYPE.CURE)
            {
                //元素加成
                float fManaAdd = 1f;
                float fManaExt = 1f;
                if (manaType != MANA_TYPE.NONE)
                {
                    fManaAdd = Mathf.Max((1 + (caster.AttrData.ATT_MANA_ATK_VALUE(manaType)) * percentPer), 0);
                    fManaExt = 1 + (caster.AttrData.ATT_MANA_ATK_VALUE(manaType)) * percentPer;
                    hit.damage = Mathf.Max(1, Mathf.FloorToInt(SkillDamage * fManaAdd * fManaExt));
                }
                else
                {
                    hit.damage = SkillDamage;
                }
                hit.SetState(HitType.Cure);
                return;
            }

            //元素克制
            float fManaK = 1;
            if (buffEffect.Type == (int)BUFF_EFFECT_TYPE.MANA
                && manaType != MANA_TYPE.NONE)
            {
                fManaK = 1 + (caster.AttrData.ATT_MANA_ATK_VALUE(manaType) - defender.AttrData.ATT_MANA_DEF_VALUE(manaType)) * percentPer;
                hit.SetState((HitType)(1 << (4 + (int)manaType)));
            }

            //防御减免
            float fDefDamage = 1f;
            if (buffEffect.Type != (int)BUFF_EFFECT_TYPE.REAL_DAMAGE)
            {
                //防御免伤=Max(守方防御-攻方破甲，0)/[Max(守方防御-攻方破甲，0)+系数4]
                float defDamage = Mathf.Max(defender.AttrData.GetValue(AttrType.DEF) - caster.AttrData.GetValue(AttrType.BREAK), 0);
                fDefDamage = 1 - defDamage / (defDamage + BattleUtil.GetGlobalK(GlobalK.DEFENCE_4));
                hit.SetState(HitType.Real);
            }

            //额外加成
            float fAdd = 1f;
            if (buffEffect.Type == (int)BUFF_EFFECT_TYPE.REAL_DAMAGE)
            {
                fAdd = Mathf.Max(1 + caster.AttrData.GetValue(AttrType.HURT) * percentPer, 0);
            }
            else
            {
                fAdd = Mathf.Max(1 + (caster.AttrData.GetValue(AttrType.HURT) - defender.AttrData.GetValue(AttrType.PARRY)) * percentPer, 0);
            }

            //结算伤害
            damage = Mathf.FloorToInt(SkillDamage * fManaK * fDefDamage * fAdd);

            //最终伤害
            damage = Mathf.FloorToInt(Mathf.Max((damage * 1.0f), caster.AttrData.GetValue(AttrType.ATK) * BattleUtil.GetGlobalK(GlobalK.DAMAGEMIN_10) * percentPer));
            if (ShowLog)
                BattleLog.LogWarning("总伤害：" + damage + "  基础伤害：" + SkillDamage + "  元素克制：" + fManaK + "  防御减免：" + (1 - fDefDamage) + "  额外加成：" + fAdd);
            hit.damage = Mathf.Max(1, damage);
        }
    }
}