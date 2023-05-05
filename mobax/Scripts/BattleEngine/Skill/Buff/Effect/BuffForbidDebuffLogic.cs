/* Created:Loki Date:2022-08-31*/

namespace BattleEngine.Logic
{
    /// <summary>
    /// 抵抗减益
    /// </summary>
    public class BuffForbidDebuffLogic : BuffEffectLogic
    {
        private int forbidDeBuffNum = 0;
        public int ForbidDeBuffNum
        {
            set { forbidDeBuffNum = value; }
            get { return forbidDeBuffNum; }
        }

        public override void InitData(BuffAbility _buffAbility, BuffEffect _buffEffect)
        {
            base.InitData(_buffAbility, _buffEffect);
            forbidDeBuffNum = 0;
        }

        public override void ExecuteLogic()
        {
            forbidDeBuffNum = buffEffect.Param1;
        }

        public override void EndLogic()
        {
            forbidDeBuffNum = 0;
        }
    }
}