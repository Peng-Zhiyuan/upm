using System;
using System.Collections.Generic;
using UnityEngine;

namespace Neatly.Timer {
    
    public class NeatlyTimer : MonoBehaviour {
        public static NeatlyTimer instance;

        public static NeatlyTimer Init() {
            if (instance == null) {
                GameObject go = new GameObject("NeatlyTimer");
                instance = go.AddComponent<NeatlyTimer>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
        public bool timeStop = false;

        private NeatlyTimerPool timerPool = new NeatlyTimerPool();
        private List<NeatlyTimerImplement> timers = new List<NeatlyTimerImplement>();

        public static void AddFrame(NeatlyBehaviour behaviour, Action<float> action, float intervalFrame = 1, bool once = false) {
            var timerImp = instance.timerPool.Create();
            instance.timers.Add(timerImp.InitFrame(behaviour, action, intervalFrame, once));
        }

        public static void AddClock(NeatlyBehaviour behaviour, Action<float> action, float intervalClock = 1, bool once = false) {
            var timerImp = instance.timerPool.Create();
            instance.timers.Add(timerImp.InitClock(behaviour, action, intervalClock, once));
        }
        public static void AddFrame(GameObject go, Action<float> action, float intervalFrame = 1, bool once = false) {
            var timerImp = instance.timerPool.Create();
            instance.timers.Add(timerImp.InitFrame(go, action, intervalFrame, once));
        }

        public static void AddClock(GameObject go, Action<float> action, float intervalClock = 1, bool once = false) {
            var timerImp = instance.timerPool.Create();
            instance.timers.Add(timerImp.InitClock(go, action, intervalClock, once));
        }

        public static NeatlyTimerImplement CheckReapeat(NeatlyBehaviour behaviour, Action<float> action) {
            for (int i = 0; i < instance.timers.Count; i++) {
                if (instance.timers[i].behaviour == behaviour && instance.timers[i].function == action) {
                    return instance.timers[i];
                }
            }
            return null;
        }

        public static void Remove(NeatlyBehaviour behaviour) {
            var list = instance.timers;
            for (int i = 0; i < list.Count; i++) {
                var t = list[i];
                if (t.behaviour == behaviour) {
                    t.SetDestroy();
                }
            }
        }

        public static void Remove(GameObject gameObject) {
            var list = instance.timers;
            for (int i = 0; i < list.Count; i++) {
                var t = list[i];
                if (t.gameObject == gameObject) {
                    t.SetDestroy();
                }
            }
        }

        public static void Remove(NeatlyBehaviour behaviour, Action<float> action) {
            var list = instance.timers;
            for (int i = 0; i < list.Count; i++) {
                var t = list[i];
                if (t.behaviour == behaviour && t.function == action) {
                    t.SetDestroy();
                }
            }
        }

        public static void Remove(GameObject gameObject, Action<float> action) {
            var list = instance.timers;
            for (int i = 0; i < list.Count; i++) {
                var t = list[i];
                if (t.gameObject == gameObject && t.function == action) {
                    t.SetDestroy();
                }
            }
        }

        void Update() {
            if (timeStop) {
                return;
            }
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < timers.Count; i++) {
                if (timers[i].IsDestroy) {
                    timerPool.Recycle(timers[i]);
                    timers.RemoveAt(i);
                    i--;
                    continue;
                }
                timers[i].Excute(deltaTime);
            }
        }
    }

    internal sealed class NeatlyTimerPool {
        List<NeatlyTimerImplement> pool = new List<NeatlyTimerImplement>();

        public NeatlyTimerImplement Create() {
            NeatlyTimerImplement imp;
            if (pool.Count <= 0) {
                imp = new NeatlyTimerImplement();
                return imp;
            }
            imp = pool[pool.Count - 1];
            pool.RemoveAt(pool.Count - 1);
            return imp;
        }

        public void Recycle(NeatlyTimerImplement imp) {
            imp.Dispose();
            pool.Add(imp);
        }
    }

    public sealed class NeatlyTimerImplement {
        public enum SaveMode {
            Behaviour,
            GameObject,
        }
        public enum TimerMode {
            Frame,
            Clock,
        }

        public NeatlyBehaviour m_NeatlyBehaviour;
        public GameObject m_GameObject;
        private SaveMode m_SaveMode;
        public Action<float> m_Function;
        public bool IsDestroy { get; private set; }
        public bool IsEnabled { get; private set; }

        private TimerMode m_TimerMode;
        private bool m_Once;
        private float m_IntervalFrame;
        private float m_DeltaFrame;
        private float m_IntervalClock;
        private float m_DeltaTime;

        public NeatlyBehaviour behaviour { get { return m_NeatlyBehaviour; } }
        public GameObject gameObject { get { return m_GameObject; } }
        public Action<float> function { get { return m_Function; } }

        public void Clear() {
            m_NeatlyBehaviour = null;
            m_GameObject = null;
            m_Function = null;
            IsDestroy = false;
            m_IntervalFrame = 0;
            m_DeltaFrame = 0;
            m_IntervalClock = 0;
            m_DeltaTime = 0;
        }

        public void Init(NeatlyBehaviour bh, Action<float> action) {
            m_NeatlyBehaviour = bh;
            if (m_NeatlyBehaviour == null) {
                Debug.LogError("严重错误初始化空...");
            }
            m_Function = action;
            m_SaveMode = SaveMode.Behaviour;
        }

        public void Init(GameObject go, Action<float> action) {
            m_GameObject = go;
            m_Function = action;
            m_SaveMode = SaveMode.GameObject;
        }

        public NeatlyTimerImplement InitFrame(NeatlyBehaviour bh, Action<float> action, float intervalFrame, bool once) {
            Init(bh, action);
            m_IntervalFrame = intervalFrame;
            m_TimerMode = TimerMode.Frame;
            m_Once = once;
            return this;
        }

        public NeatlyTimerImplement InitClock(NeatlyBehaviour bh, Action<float> action, float intervalClock, bool once) {
            Init(bh, action);
            m_IntervalClock = intervalClock;
            m_TimerMode = TimerMode.Clock;
            m_Once = once;
            return this;
        }
        public NeatlyTimerImplement InitFrame(GameObject go, Action<float> action, float intervalFrame, bool once) {
            Init(go, action);
            m_IntervalFrame = intervalFrame;
            m_TimerMode = TimerMode.Frame;
            m_Once = once;
            return this;
        }

        public NeatlyTimerImplement InitClock(GameObject go, Action<float> action, float intervalClock, bool once) {
            Init(go, action);
            m_IntervalClock = intervalClock;
            m_TimerMode = TimerMode.Clock;
            m_Once = once;
            return this;
        }

        public void Excute(float dt) {
            CheckState();
            if (IsDestroy || !IsEnabled) {
                return;
            }
            m_DeltaTime += dt;
            switch (m_TimerMode) {
                case TimerMode.Frame:
                    m_DeltaFrame++;
                    if (m_DeltaFrame >= m_IntervalFrame) {
                        m_Function(m_DeltaTime);
                        m_DeltaFrame = 0;
                        m_DeltaTime = 0;
                        if (m_Once) {
                            SetDestroy();
                        }
                    }
                    break;
                case TimerMode.Clock:
                    if (m_DeltaTime >= m_IntervalClock) {
                        m_Function(m_DeltaTime);
                        m_DeltaTime -= m_IntervalClock;
                        if (m_Once) {
                            SetDestroy();
                        }
                    }
                    break;
            }
        }

        public void CheckState() {
            switch (m_SaveMode) {
                case SaveMode.Behaviour:
                    if (m_NeatlyBehaviour.IsDestroy) {
                        SetDestroy();
                        return;
                    }
                    IsEnabled = m_Once || m_NeatlyBehaviour.IsEnable;
                    break;
                case SaveMode.GameObject:
                    if (m_GameObject == null) {
                        SetDestroy();
                        return;
                    }
                    IsEnabled = m_Once || gameObject.activeInHierarchy;
                    break;
            }
        }

        public void Dispose() {
            Clear();
        }

        public void SetDestroy() {
            IsDestroy = true;
        }
    }
}
