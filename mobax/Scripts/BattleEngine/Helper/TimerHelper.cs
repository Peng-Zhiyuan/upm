namespace BattleEngine.View
{
    using System;
    using UnityEngine;
    using Neatly.Timer;
    using Neatly;

    public static class TimerHelper
    {
        public static GameObject AddClock(string name, Action<float> action, float intervalClock = 1, bool once = false)
        {
            Transform Root = new GameObject().transform;
            Transform trans = Root.Find(name);
            NeatlyBehaviour clock;
            if (trans == null)
            {
                clock = new GameObject(name).AddComponent<NeatlyBehaviour>();
                clock.transform.SetParent(Root);
            }
            else
            {
                clock = trans.GetComponent<NeatlyBehaviour>();
                if (clock == null)
                {
                    clock = trans.gameObject.AddComponent<NeatlyBehaviour>();
                }
            }
            NeatlyTimer.AddClock(clock, action, intervalClock);
            return clock.gameObject;
        }

        public static NeatlyBehaviour AddFrame(string name, Action<float> action, float intervalFrame = 1, bool once = false)
        {
            GameObject RootGo = GameObject.Find("TimerHelper");
            if (RootGo == null)
            {
                RootGo = new GameObject("TimerHelper");
            }
            Transform Root = RootGo.transform;
            Transform trans = Root.Find(name);
            NeatlyBehaviour frame;
            if (trans == null)
            {
                frame = new GameObject(name).AddComponent<NeatlyBehaviour>();
                frame.transform.SetParent(Root);
            }
            else
            {
                frame = trans.GetComponent<NeatlyBehaviour>();
                if (frame == null)
                {
                    frame = trans.gameObject.AddComponent<NeatlyBehaviour>();
                }
            }
            NeatlyTimer.AddFrame(frame, action, intervalFrame);
            return frame;
        }

        public static void Remove(object timeObj)
        {
            NeatlyTimer.Remove(timeObj as NeatlyBehaviour);
        }

        private static readonly long epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

        /// <summary>
        /// 客户端时间
        /// </summary>
        /// <returns></returns>
        public static long ClientNow()
        {
            return (DateTime.UtcNow.Ticks - epoch) / 10000;
        }

        public static long ClientNowSeconds()
        {
            return (DateTime.UtcNow.Ticks - epoch) / 10000000;
        }

        public static long Now()
        {
            return ClientNow();
        }
    }
}