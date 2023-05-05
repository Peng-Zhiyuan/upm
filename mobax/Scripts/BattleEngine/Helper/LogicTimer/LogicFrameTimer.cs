/* Created:Loki Date:2023-01-16*/

namespace BattleEngine.Logic
{
    using System;

    public class LogicFrameTimer
    {
        public string name;
        public Action triggerHandler;
        public int triggerFrame;
        public int delayFrame;
        public bool loop;
        public bool isPuased;
        public bool isRemoved;
    }
}