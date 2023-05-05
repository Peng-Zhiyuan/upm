namespace BattleEngine.Logic
{
    public partial class LogicEntity : Entity
    {
        public Effect Effect { get; set; }
        public BuffRow buffEffect { get; set; }

        public override void Awake(object initData)
        {
            Effect = null;
            buffEffect = null;
            if (initData == null)
            {
                return;
            }
            if (initData is Effect)
            {
                Effect = initData as Effect;
            }
            else if (initData is BuffRow)
            {
                buffEffect = initData as BuffRow;
            }
        }

        public void ApplyEffect()
        {
            if (Effect != null)
            {
                if (Effect is DamageEffect damageEffect)
                {
                    var damageAction = GetParent<BuffAbility>().Caster.CreateCombatAction<DamageAction>();
                    damageAction.DamageEffect = damageEffect;
                    damageAction.Target = GetParent<BuffAbility>().SelfActorEntity;
                    damageAction.DamageSource = DamageSource.Buff;
                    damageAction.HurtRatio = 1;
                    damageAction.ApplyDamage();
                }
                else if (Effect is CureEffect cureEffect)
                {
                    if (GetParent<BuffAbility>() == null)
                    {
                        return;
                    }
                    if (GetParent<BuffAbility>().SelfActorEntity == null)
                    {
                        return;
                    }
                    var action = GetParent<BuffAbility>().SelfActorEntity.CreateCombatAction<CureAction>();
                    action.Target = GetParent<BuffAbility>().SelfActorEntity;
                    if (action.Target.CurrentHealth.Value > 0)
                    {
                        action.CureEffect = cureEffect;
                        action.ApplyCure();
                    }
                }
            }
            if (buffEffect != null)
            {
                BuffAbility buffAbility = GetParent<BuffAbility>();
                if (buffAbility == null
                    || buffAbility.OwnerEntity == null)
                {
                    Destroy(this);
                    return;
                }
                buffAbility.ApplyAbilityEffectsTo(buffAbility.OwnerEntity);
            }
        }
    }
}