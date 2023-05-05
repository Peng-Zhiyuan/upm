namespace BattleEngine.Logic
{
    using System;
    using System.Linq;

    public static class IdFactory
    {
        public static long BaseRevertTicks { get; set; }

        public static long NewInstanceId()
        {
            if (BaseRevertTicks == 0)
            {
                var now = DateTime.UtcNow.Ticks;
                var str = now.ToString().Reverse();
                BaseRevertTicks = long.Parse(string.Concat(str));
            }
            BaseRevertTicks++;
            return BaseRevertTicks;
        }
    }
}