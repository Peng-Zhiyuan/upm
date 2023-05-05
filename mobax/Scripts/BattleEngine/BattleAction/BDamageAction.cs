namespace BattleEngine.Logic
{
    public class BDamageAction : IBattleAction
    {
        public string targetID; //目标
        public string casterID; //施法者
        public string targetPartNodeKey;
        public int damage;
        public int currentValue;
        public bool atk;
        public string imageAddress;
        public bool baoji;
        public bool attackDamage;
        public int block;
        public DamageSource DamageSource { get; set; }

        public string GetActionType()
        {
            return "BDamageAction";
        }

        public HitType hitType;

        public bool HasState(HitType state)
        {
            return FlagsHelper.IsSet(hitType, state);
        }

        public void SetState(HitType state)
        {
            FlagsHelper.Set(ref hitType, state);
        }

        public void RemoveState(HitType state)
        {
            FlagsHelper.Unset(ref hitType, state);
        }
    }
}