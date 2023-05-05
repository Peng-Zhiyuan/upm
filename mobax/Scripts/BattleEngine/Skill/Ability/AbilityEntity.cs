namespace BattleEngine.Logic
{
    public abstract class AbilityEntity<T> : AbilityEntity where T : AbilityExecution
    {
        public virtual new T CreateExecution()
        {
            return base.CreateExecution() as T;
        }
    }

    /// <summary>
    /// 能力实体，存储着某个英雄某个能力的数据和状态
    /// </summary>
    public abstract class AbilityEntity : Entity
    {
        private CombatActorEntity _ownerEntity;
        public CombatActorEntity OwnerEntity
        {
            get
            {
                if (_ownerEntity == null)
                {
                    _ownerEntity = GetParent<CombatActorEntity>();
                }
                return _ownerEntity;
            }
        }
        public bool Enable { get; set; } = true;
        public object ConfigObject { get; set; }
        public int Level { get; set; } = 1;

        public override void Awake(object initData)
        {
            ConfigObject = initData;
        }

        //尝试激活能力
        public virtual void TryActivateAbility()
        {
            ActivateAbility();
        }

        //激活能力
        public virtual void ActivateAbility() { }

        //结束能力
        public virtual void EndAbility()
        {
            Destroy(this);
        }

        //创建能力执行体
        public virtual AbilityExecution CreateExecution()
        {
            return null;
        }

        public virtual void ApplyEffectTo(CombatActorEntity targetEntity, Effect effectItem, DamageSource damageSource = DamageSource.Attack, CombatUnitEntity partEntity = null, float hurtRatio = 1, bool isOneHitKill = false, SkillRow skillRow = null)
        {
            if (effectItem is DamageEffect damageEffect)
            {
                var action = this.OwnerEntity.CreateCombatAction<DamageAction>();
                action.Target = targetEntity;
                action.partEntity = partEntity;
                action.IsOneHitKill = isOneHitKill;
                action.HurtRatio = hurtRatio;
                action.DamageSource = damageSource;
                action.DamageEffect = damageEffect;
                action.DamageSkillRow = skillRow;
                action.abilityEntity = this;
                action.attackDamage = true;
                action.ApplyDamage();
            }
            else if (effectItem is CureEffect cureEffect)
            {
                var action = this.OwnerEntity.CreateCombatAction<CureAction>();
                action.Target = targetEntity;
                action.CureEffect = cureEffect;
                action.CureRatio = hurtRatio;
                action.abilityEntity = this;
                action.cureSkillRow = skillRow;
                action.ApplyCure();
            }
            else if (effectItem is TriggerSkillEffect triggerSkillEffect)
            {
                var action = this.OwnerEntity.CreateCombatAction<TriggerSkillAction>();
                action.Target = targetEntity;
                action.TriggerSkillEffect = triggerSkillEffect;
                action.ApplyTrigger();
            }
            else
            {
                if (OwnerEntity.AssignEffectActionAbility.TryCreateAction(out var action))
                {
                    action.Target = targetEntity;
                    action.SourceAbility = this;
                    action.Effect = effectItem;
                    action.ApplyAssignEffect();
                }
            }
        }

        //应用能力效果
        public virtual void ApplyAbilityEffectsTo(CombatActorEntity targetEntity, CombatUnitEntity partEntity = null, float hurtRatio = 1, bool isOneHitKill = false) { }
    }
}