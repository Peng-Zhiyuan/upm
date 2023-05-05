/* Created:Loki Date:2022-08-31*/

namespace BattleEngine.Logic
{
    public abstract class BuffEffectLogic
    {
        protected BuffAbility buffAbility;
        protected BuffEffect buffEffect;
        public BuffEffect GetBuffEffect
        {
            get { return buffEffect; }
        }

        public virtual void InitData(BuffAbility _buffAbility, BuffEffect _buffEffect)
        {
            this.buffAbility = _buffAbility;
            this.buffEffect = _buffEffect;
        }

        public abstract void ExecuteLogic();
        public abstract void EndLogic();

        /// <summary>
        /// 获取效果类型
        /// </summary>
        public BUFF_EFFECT_TYPE GetBuffEffectType()
        {
            if (buffEffect == null)
            {
                return BUFF_EFFECT_TYPE.None;
            }
            return (BUFF_EFFECT_TYPE)buffEffect.Type;
        }
    }
}