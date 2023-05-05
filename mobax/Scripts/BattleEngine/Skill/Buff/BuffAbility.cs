namespace BattleEngine.Logic
{
    using System.Collections.Generic;

    public class BuffAbility : AbilityEntity
    {
        public BuffRow buffRow = null;

        public int StackNum = 1;

        /// <summary>
        /// 投放者、施术者
        /// </summary>
        public CombatActorEntity Caster { get; set; }

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

        public void LogicUpdate(float deltaTime)
        {
            if (BuffLifeTime != null)
            {
                BuffLifeTime.LogicUpdate(deltaTime);
            }
            if (LogicIntervalTrigger != null)
            {
                LogicIntervalTrigger.LogicUpdate(deltaTime);
            }
        }

        /// <summary>
        /// buff效果执行队列
        /// </summary>
        public List<BuffEffectLogic> BuffEffectLogicLst = new List<BuffEffectLogic>();

        public override void Awake(object initData)
        {
            base.Awake(initData);
            BuffEffectLogicLst.Clear();
            buffRow = initData as BuffRow;
            if (buffRow == null)
            {
                BattleLog.LogError("initData is not BuffRow");
                return;
            }
            Name = buffRow.Name;
        }

        private BuffEffectLogic CreateBuffEffectLogic(BuffEffect effect)
        {
            switch (effect.Type)
            {
                case (int)BUFF_EFFECT_TYPE.ATTR:
                    BuffChangeAttrLogic attrEffect = new BuffChangeAttrLogic();
                    attrEffect.InitData(this, effect);
                    return attrEffect;
                case (int)BUFF_EFFECT_TYPE.PHYSIC:
                case (int)BUFF_EFFECT_TYPE.MANA:
                case (int)BUFF_EFFECT_TYPE.REAL_DAMAGE:
                case (int)BUFF_EFFECT_TYPE.CURE:
                    BuffDamageValueLogic damageValueLogic = new BuffDamageValueLogic();
                    damageValueLogic.InitData(this, effect);
                    return damageValueLogic;
                case (int)BUFF_EFFECT_TYPE.PROTECTION:
                    BuffHurnDamageLogic hurnDamageLogic = new BuffHurnDamageLogic();
                    hurnDamageLogic.InitData(this, effect);
                    return hurnDamageLogic;
                case (int)BUFF_EFFECT_TYPE.SUCK:
                    BuffSuckLogic suckLogic = new BuffSuckLogic();
                    suckLogic.InitData(this, effect);
                    return suckLogic;
                case (int)BUFF_EFFECT_TYPE.REATTACK:
                    BuffReAttackLogic reAttackLogic = new BuffReAttackLogic();
                    reAttackLogic.InitData(this, effect);
                    return reAttackLogic;
                case (int)BUFF_EFFECT_TYPE.CURE_HALO:
                case (int)BUFF_EFFECT_TYPE.CURE_DAMAGE_HALO:
                    BuffCureHaloLogic cureHaloLogic = new BuffCureHaloLogic();
                    cureHaloLogic.InitData(this, effect);
                    return cureHaloLogic;
                case (int)BUFF_EFFECT_TYPE.ADD_SKILL_DAMAGE_12:
                case (int)BUFF_EFFECT_TYPE.ADD_SKILL_DAMAGE_45:
                case (int)BUFF_EFFECT_TYPE.ADD_SKILL_CRIT_DAMAGE:
                    BuffSkillDamageLogic skillDamageLogic = new BuffSkillDamageLogic();
                    skillDamageLogic.InitData(this, effect);
                    return skillDamageLogic;
                case (int)BUFF_EFFECT_TYPE.CLEAR_DEBUFF:
                    BuffClearDebuffLogic clearDebuffLogic = new BuffClearDebuffLogic();
                    clearDebuffLogic.InitData(this, effect);
                    return clearDebuffLogic;
                case (int)BUFF_EFFECT_TYPE.FORBID_DEBUFF:
                    BuffForbidDebuffLogic forbidDebuffLogic = new BuffForbidDebuffLogic();
                    forbidDebuffLogic.InitData(this, effect);
                    return forbidDebuffLogic;
                case (int)BUFF_EFFECT_TYPE.BE_REBORN:
                    BuffBeRebornLogic rebornLogic = new BuffBeRebornLogic();
                    rebornLogic.InitData(this, effect);
                    return rebornLogic;
                case (int)BUFF_EFFECT_TYPE.DAMAGE_TOGETHER:
                    BuffDamageTogetherLogic shareDamageLogic = new BuffDamageTogetherLogic();
                    shareDamageLogic.InitData(this, effect);
                    return shareDamageLogic;
                case (int)BUFF_EFFECT_TYPE.KILL_OWN:
                    BuffKillOwnerLogic killOwnerLogic = new BuffKillOwnerLogic();
                    killOwnerLogic.InitData(this, effect);
                    return killOwnerLogic;
                case (int)BUFF_EFFECT_TYPE.MAX_HP_DAMAGE:
                case (int)BUFF_EFFECT_TYPE.CURRENT_HP_DAMAGE:
                    BuffAttributeToDamageLogic attributeToDamage = new BuffAttributeToDamageLogic();
                    attributeToDamage.InitData(this, effect);
                    return attributeToDamage;
                case (int)BUFF_EFFECT_TYPE.CLEAR_CONTROL_TYPE_BUFF:
                    BuffClearControlTypeLogic clearControlTypeLogic = new BuffClearControlTypeLogic();
                    clearControlTypeLogic.InitData(this, effect);
                    return clearControlTypeLogic;
                case (int)BUFF_EFFECT_TYPE.FORCE_DAMGE_VALUE:
                    BuffForceDamageValueLogic forceDamageValueLogic = new BuffForceDamageValueLogic();
                    forceDamageValueLogic.InitData(this, effect);
                    return forceDamageValueLogic;
                case (int)BUFF_EFFECT_TYPE.REPLACE_SKILL:
                    BuffReplaceSkillAbilityLogic buffReplaceSkillAbilityLogic = new BuffReplaceSkillAbilityLogic();
                    buffReplaceSkillAbilityLogic.InitData(this, effect);
                    return buffReplaceSkillAbilityLogic;
                case (int)BUFF_EFFECT_TYPE.NOT_CURE:
                    BuffForbidCureLogic buffForbidCureLogic = new BuffForbidCureLogic();
                    buffForbidCureLogic.InitData(this, effect);
                    return buffForbidCureLogic;
                case (int)BUFF_EFFECT_TYPE.CHANGE_AVATAR:
                    BuffChangeAvatarLogic buffChangeAvatarLogic = new BuffChangeAvatarLogic();
                    buffChangeAvatarLogic.InitData(this, effect);
                    return buffChangeAvatarLogic;
            }
            return null;
        }

        //激活
        public override void ActivateAbility()
        {
            base.ActivateAbility();
            //逻辑触发
            ExecuteLogicTrigger();
            //嘲讽特殊处理
            ExecuteOTModify();
            EventManager.Instance.SendEvent<BuffAbility>("ActivateBuffAbility", this);
        }

        /// <summary>
        /// BUFF逻辑触发
        /// </summary>
        protected void ExecuteLogicTrigger()
        {
            if (buffRow == null
                || buffRow.Effects == null
                || buffRow.Effects.Count == 0)
            {
                BattleLog.LogError("ExecuteLogicTrigger Buff Effect is null");
                return;
            }
            if (BuffEffectLogicLst.Count > 0)
            {
                for (int i = 0; i < BuffEffectLogicLst.Count; i++)
                {
                    BuffEffectLogicLst[i].EndLogic();
                }
                BuffEffectLogicLst.Clear();
            }
            for (int i = 0; i < buffRow.Effects.Count; i++)
            {
                if (buffRow.Effects[i].Type == 0)
                {
                    continue;
                }
                var effectLoigc = CreateBuffEffectLogic(buffRow.Effects[i]);
                if (effectLoigc != null)
                {
                    BuffEffectLogicLst.Add(effectLoigc);
                }
                else
                {
                    BattleLog.LogError("Cant find Buff Effect Type Logic !!!!!");
                }
            }
            if (buffRow.Time == 0)
            {
                ApplyAbilityEffectsTo(OwnerEntity);
                EndAbility();
            }
            else if (buffRow.Step == 0)
            {
                ApplyAbilityEffectsTo(OwnerEntity);
            }
            else
            {
                if (_logicEntity == null)
                {
                    _logicEntity = CreateWithParent<LogicEntity>(this, buffRow);
                    _logicIntervalTrigger = _logicEntity.AddComponent<LogicIntervalTriggerComponent>();
                }
            }
            SetBuffTime(buffRow.Time * 0.001f);
            SetBuffIntervalTime(buffRow.Step * 0.001f);
        }

        /// <summary>
        /// 嘲讽特殊处理 
        /// </summary>
        protected void ExecuteOTModify()
        {
            if (buffRow.controlType != (int)ACTION_CONTROL_TYPE.control_7
                || SelfActorEntity == null
                || SelfActorEntity.CurrentHealth.Value <= 0)
            {
                return;
            }
            if (SelfActorEntity != null)
            {
                BattleUtil.TriggerTaunted(SelfActorEntity.UID);
            }
        }

        /// <summary>
        /// BUFF结束
        /// </summary>
        public override void EndAbility()
        {
            if (buffRow != null
                && !SelfActorEntity.IsDead)
            {
                for (int i = 0; i < BuffEffectLogicLst.Count; i++)
                {
                    BuffEffectLogicLst[i].EndLogic();
                }
                //嘲讽特殊处理
                if (buffRow.controlType == (int)ACTION_CONTROL_TYPE.control_7
                    && SelfActorEntity != null)
                {
                    BattleUtil.EndTaunted(SelfActorEntity.UID);
                }
                BuffEffectLogicLst.Clear();
            }
            SelfActorEntity.OnBuffRemove(this);
            BattleLog.LogWarning("[Buff] " + LocalizationManager.Stuff.GetText(buffRow.Name) + " End Effect ~~~~~~~~~~~~~~~~~~~~~~~");
            base.EndAbility();
        }

        /// <summary>
        /// BUFF效果实现
        /// </summary>
        /// <param name="targetEntity"></param>
        /// <param name="partEntity"></param>
        /// <param name="hurtRatio"></param>
        /// <param name="isOneHitKill"></param>
        public override void ApplyAbilityEffectsTo(CombatActorEntity targetEntity, CombatUnitEntity partEntity = null, float hurtRatio = 1, bool isOneHitKill = false)
        {
            if (buffRow == null)
            {
                BattleLog.LogError("Cant find BuffRow");
                return;
            }
            for (int i = 0; i < BuffEffectLogicLst.Count; i++)
            {
                if (!BuffUtil.isTriggerChance(BuffEffectLogicLst[i].GetBuffEffect))
                {
                    continue;
                }
                BuffEffectLogicLst[i].ExecuteLogic();
            }
            BattleLog.LogWarning("[Buff] " + LocalizationManager.Stuff.GetText(buffRow.Name) + " Execute Effect ~~~~~~~~~~~~~~~~~~~~~~~");
        }

#region Buff Time
        private BuffLifeTimeComponent buffLifeTime;
        private BuffLifeTimeComponent BuffLifeTime
        {
            get
            {
                if (buffLifeTime == null)
                {
                    buffLifeTime = GetComponent<BuffLifeTimeComponent>();
                }
                return buffLifeTime;
            }
        }

        private LogicEntity _logicEntity;

        private LogicIntervalTriggerComponent _logicIntervalTrigger;
        private LogicIntervalTriggerComponent LogicIntervalTrigger
        {
            get
            {
                if (_logicEntity != null
                    && _logicIntervalTrigger == null)
                {
                    _logicIntervalTrigger = _logicEntity.GetComponent<LogicIntervalTriggerComponent>();
                }
                return _logicIntervalTrigger;
            }
        }

        public void SetBuffTime(float time)
        {
            if (BuffLifeTime != null)
            {
                BuffLifeTime.LifeTimer.MaxTime = time;
                BuffLifeTime.LifeTimer.Reset();
            }
        }

        public void SetBuffIntervalTime(float step)
        {
            if (LogicIntervalTrigger != null
                && step > 0)
            {
                LogicIntervalTrigger.ResetTime(step);
            }
        }

        public float GetBuffCurrentTime()
        {
            if (BuffLifeTime != null)
            {
                return BuffLifeTime.LifeTimer.GetTime();
            }
            return 0.0f;
        }

        public float GetBuffMaxTime()
        {
            if (BuffLifeTime != null)
            {
                return BuffLifeTime.LifeTimer.MaxTime;
            }
            return 0.0f;
        }
#endregion

#region 抵御DEBUFF
        public bool IsCanForbidDeBuff()
        {
            for (int i = 0; i < BuffEffectLogicLst.Count; i++)
            {
                if (BuffEffectLogicLst[i].GetBuffEffectType() == BUFF_EFFECT_TYPE.FORBID_DEBUFF)
                {
                    return true;
                }
            }
            return false;
        }

        public int GetForbidDeBuffNum()
        {
            int forbidDeBuffNum = 0;
            for (int i = 0; i < BuffEffectLogicLst.Count; i++)
            {
                if (BuffEffectLogicLst[i].GetBuffEffectType() == BUFF_EFFECT_TYPE.FORBID_DEBUFF)
                {
                    var buffEffectLogic = BuffEffectLogicLst[i] as BuffForbidDebuffLogic;
                    if (buffEffectLogic != null)
                    {
                        forbidDeBuffNum += buffEffectLogic.ForbidDeBuffNum;
                    }
                }
            }
            return forbidDeBuffNum;
        }

        /// <summary>
        /// 抵御DEBUFF次数
        /// </summary>
        /// <param name="subNum">debuff数量</param>
        /// <returns>剩余数量</returns>
        public void SubForbidDeBuffNum(int _subNum)
        {
            int subNum = _subNum;
            for (int i = 0; i < BuffEffectLogicLst.Count; i++)
            {
                if (BuffEffectLogicLst[i].GetBuffEffectType() == BUFF_EFFECT_TYPE.FORBID_DEBUFF)
                {
                    var buffEffectLogic = BuffEffectLogicLst[i] as BuffForbidDebuffLogic;
                    if (buffEffectLogic == null)
                    {
                        BuffEffectLogicLst[i].EndLogic();
                        BuffEffectLogicLst.RemoveAt(i);
                        i--;
                        continue;
                    }
                    if (buffEffectLogic.ForbidDeBuffNum >= subNum)
                    {
                        buffEffectLogic.ForbidDeBuffNum -= subNum;
                        return;
                    }
                    else
                    {
                        subNum -= buffEffectLogic.ForbidDeBuffNum;
                        buffEffectLogic.ForbidDeBuffNum = 0;
                    }
                }
            }
        }
#endregion

#region 重生
        public BuffEffectLogic IsCanReborn()
        {
            for (int i = 0; i < BuffEffectLogicLst.Count; i++)
            {
                if (BuffEffectLogicLst[i].GetBuffEffectType() == BUFF_EFFECT_TYPE.BE_REBORN)
                {
                    return BuffEffectLogicLst[i];
                }
            }
            return null;
        }
#endregion
    }
}