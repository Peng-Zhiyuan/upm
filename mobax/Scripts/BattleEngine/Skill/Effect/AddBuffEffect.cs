namespace BattleEngine.Logic
{
    public class AddBuffEffect : Effect
    {
        public BuffTrigger buffTrigger;
        public BUFF_EFFECT_TARGET buffEffectTarget;
        public BuffRow buffRow;

        public uint Duration;

        public AddBuffEffect(BuffRow buffRow, BUFF_EFFECT_TARGET target = BUFF_EFFECT_TARGET.CURRENT_TARGET, BuffTrigger buffTrigger = null)
        {
            this.buffEffectTarget = target;
            this.buffRow = buffRow;
            this.buffTrigger = buffTrigger;
            this.Duration = (uint)buffRow.Time;
        }
    }
}