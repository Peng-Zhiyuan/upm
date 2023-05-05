namespace BattleEngine.Logic
{
    public sealed class BuffKillOwnerLogic : BuffEffectLogic
    {
        public override void ExecuteLogic()
        {
            int killHP = buffAbility.SelfActorEntity.CurrentHealth.MaxValue;
            DamageAction killOwnAction = buffAbility.SelfActorEntity.CreateCombatAction<DamageAction>();
            killOwnAction.DamageValue = killHP;
            killOwnAction.Creator = buffAbility.SelfActorEntity;
            killOwnAction.Target = buffAbility.SelfActorEntity;
            killOwnAction.behitData = new BehitData();
            killOwnAction.behitData.damage = 0;
            buffAbility.SelfActorEntity.ReceiveDamage(killOwnAction, true);
            buffAbility.SelfActorEntity.OnBuffTrigger(buffAbility);
            BattleLog.LogWarning("[BUFF] 自杀" + killOwnAction.DamageValue);
        }

        public override void EndLogic() { }
    }
}