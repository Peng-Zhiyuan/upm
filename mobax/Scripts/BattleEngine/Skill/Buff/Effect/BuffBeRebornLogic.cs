/* Created:Loki Date:2022-08-31*/

namespace BattleEngine.Logic
{
    public class BuffBeRebornLogic : BuffEffectLogic
    {
        private bool isBeReborn = false;
        public bool IsHaveBeReborn
        {
            get { return isBeReborn; }
        }

        public override void InitData(BuffAbility _buffAbility, BuffEffect _buffEffect)
        {
            base.InitData(_buffAbility, _buffEffect);
            isBeReborn = false;
        }

        public override void ExecuteLogic()
        {
            isBeReborn = true;
        }

        public override void EndLogic()
        {
            isBeReborn = false;
        }
    }
}