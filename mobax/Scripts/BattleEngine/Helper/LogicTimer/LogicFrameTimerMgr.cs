/* Created:Loki Date:2023-01-16*/

using UnityEngine;

namespace BattleEngine.Logic
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 底层战斗逻辑帧等待,需要使用到BattleLogicManager的currentFrame
    /// </summary>
    public class LogicFrameTimerMgr : Singleton<LogicFrameTimerMgr>
    {
        private List<LogicFrameTimer> _scheduledLogicFrameTimerList = new List<LogicFrameTimer>();
        private Queue<LogicFrameTimer> _logicFrameTimerPool = new Queue<LogicFrameTimer>();

        /// <summary>
        /// 逻辑层计时器,向下取整,有些情况逻辑层会比表现层快一点
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="callback"></param>
        /// <param name="loop"></param>
        /// <param name="name"></param>
        public void ScheduleTimer(float delay, Action callback, bool loop = false, string name = "")
        {
            int delayFrame = Mathf.FloorToInt(delay / BattleLogicDefine.LogicSecTime);
            ScheduleTimer(delayFrame, callback, loop, name);
        }

        /// <summary>
        /// 设置一个计时器，当 loop 为 true 时，回调会额外立刻触发一次
        /// </summary>
        /// <param name="delayFrame">帧</param>
        /// <param name="callback"></param>
        /// <param name="loop"></param>
        /// <param name="name"></param>
        public void ScheduleTimer(int delayFrame, Action callback, bool loop = false, string name = "")
        {
            if (delayFrame < 0)
            {
                throw new Exception("delay must be not negative");
            }
            if (delayFrame == 0 && loop)
            {
                throw new Exception("can not be loop while delay is 0");
            }
            if (callback == null
                || delayFrame == 0)
            {
                callback?.Invoke();
                return;
            }
            LogicFrameTimer timer = TakeOneTimer();
            timer.name = name;
            timer.triggerHandler = callback;
            timer.triggerFrame = BattleLogicManager.Instance.CurrentFrame + delayFrame;
            timer.delayFrame = delayFrame;
            timer.loop = loop;
            timer.isRemoved = false;
            timer.isPuased = false;
            if (loop)
            {
                callback.Invoke();
            }
            _scheduledLogicFrameTimerList.Add(timer);
        }

        private LogicFrameTimer TakeOneTimer()
        {
            if (_logicFrameTimerPool.Count > 0)
            {
                return _logicFrameTimerPool.Dequeue();
            }
            else
            {
                return new LogicFrameTimer();
            }
        }

        public void RemoveAll()
        {
            for (int i = _scheduledLogicFrameTimerList.Count - 1; i >= 0; i--)
            {
                _logicFrameTimerPool.Enqueue(_scheduledLogicFrameTimerList[i]);
                _scheduledLogicFrameTimerList.Remove(_scheduledLogicFrameTimerList[i]);
            }
        }

        public void PauseAll()
        {
            for (int i = _scheduledLogicFrameTimerList.Count - 1; i >= 0; i--)
            {
                _scheduledLogicFrameTimerList[i].isPuased = true;
            }
        }

        public void ResumeAll()
        {
            for (int i = _scheduledLogicFrameTimerList.Count - 1; i >= 0; i--)
            {
                _scheduledLogicFrameTimerList[i].isPuased = false;
            }
        }

        public void OnLogicUpdate()
        {
            if (_scheduledLogicFrameTimerList.Count == 0)
            {
                return;
            }
            var time = BattleLogicManager.Instance.CurrentFrame;
            List<LogicFrameTimer> removeList = new List<LogicFrameTimer>();
            for (int i = 0; i < _scheduledLogicFrameTimerList.Count; i++)
            {
                var timer = _scheduledLogicFrameTimerList[i];
                if (timer.isRemoved)
                {
                    _logicFrameTimerPool.Enqueue(timer);
                    removeList.Add(timer);
                }
                else if (timer.isPuased)
                {
                    timer.triggerFrame += 1;
                }
                else if (time >= timer.triggerFrame)
                {
                    timer.triggerHandler?.Invoke();
                    if (timer.loop)
                    {
                        timer.triggerFrame += timer.delayFrame;
                    }
                    else
                    {
                        _logicFrameTimerPool.Enqueue(timer);
                        removeList.Add(timer);
                    }
                }
            }
            for (int i = 0; i < removeList.Count; i++)
            {
                _scheduledLogicFrameTimerList.Remove(removeList[i]);
            }
        }
    }
}