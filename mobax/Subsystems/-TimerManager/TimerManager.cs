using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class TimerMgr : Single<TimerMgr>, IUpdatable
{
    private List<Timer> scheduledTimerList = new List<Timer>();
    private HashSet<string> pauseHash = new HashSet<string>();
    private Queue<Timer> timerPool = new Queue<Timer>();

    /// <summary>
    /// 设置一个计时器，当 loop 为 true 时，回调会额外立刻触发一次
    /// </summary>
    /// <param name="delay">秒</param>
    /// <param name="callback"></param>
    /// <param name="loop"></param>
    /// <param name="name"></param>
    private Timer CreateScheduleTimer(float delay, Action callback, bool loop = false, string name = "")
    {
        if (delay < 0)
        {
            throw new Exception("delay must be not negative");
        }
        if (delay == 0 && loop)
        {
            throw new Exception("can not be loop while delay is 0");
        }
        if (callback == null)
        {
            return null;
        }
        if (delay == 0)
        {
            callback.Invoke();
            return null;
        }
        Timer timer = TakeOneTimer();
        timer.name = name;
        timer.triggerHandler = callback;
        timer.triggerTime = Time.time + delay;
        timer.delay = delay;
        timer.loop = loop;
        timer.isRemoved = false;
        if (!string.IsNullOrEmpty(name))
        {
            timer.isPuased = this.pauseHash.Contains(name);
        }
        else
        {
            timer.isPuased = false;
        }
        if (scheduledTimerList.Count == 0)
        {
            UpdateManager.Stuff.Add(Instance);
        }
        scheduledTimerList.Add(timer);
        return timer;
    }

    /// <summary>
    /// 设置一个计时器，当 loop 为 true 时，回调会额外立刻触发一次
    /// </summary>
    /// <param name="delay">秒</param>
    /// <param name="callback"></param>
    /// <param name="loop"></param>
    /// <param name="name"></param>
    public Timer ScheduleTimer(float delay, Action callback, bool loop = false, string name = "")
    {
        Timer timer = CreateScheduleTimer(delay, callback, loop, name);
        if (timer == null)
        {
            return null;
        }
        if (loop)
        {
            callback?.Invoke();
        }
        return timer;
    }

    public Timer BattleSchedulerTimer(float delay, Action callback, bool loop = false, string name = "")
    {
        Timer timer = CreateScheduleTimer(delay, callback, loop, name);
        if (timer == null)
        {
            return null;
        }
        if (loop)
        {
            callback?.Invoke();
        }
        timer.type = TimerType.Battle;
        return timer;
    }

    /// <summary>
    /// 设置一个计时器，当 loop 为 true 时，不会立刻触发一次
    /// </summary>
    /// <param name="delay">秒</param>
    /// <param name="callback"></param>
    /// <param name="loop"></param>
    /// <param name="name"></param>
    public Timer ScheduleTimerDelay(float delay, Action callback, bool loop = false, string name = "")
    {
        Timer timer = CreateScheduleTimer(delay, callback, loop, name);
        return timer;
    }

    public Timer BattleSchedulerTimerDelay(float delay, Action callback, bool loop = false, string name = "")
    {
        Timer timer = CreateScheduleTimer(delay, callback, loop, name);
        if (timer != null)
        {
            timer.type = TimerType.Battle;
        }
        return timer;
    }

    public Task<bool> DelaySecondAsync(float delay, string name = "")
    {
        var tcs = new TaskCompletionSource<bool>();
        this.ScheduleTimer(delay, () => { tcs.SetResult(true); }, false, name);
        return tcs.Task;
    }

    private Timer TakeOneTimer()
    {
        if (timerPool.Count > 0)
        {
            return timerPool.Dequeue();
        }
        else
        {
            return new Timer();
        }
    }

    public void ExcuteCallBack(string name)
    {
        if (!HasSchedule(name))
        {
            return;
        }
        Timer excuteTimer = null;
        for (int i = scheduledTimerList.Count - 1; i >= 0; i--)
        {
            if (scheduledTimerList[i].name == name)
            {
                excuteTimer = scheduledTimerList[i];
            }
        }
        excuteTimer?.triggerHandler?.Invoke();
        Remove(name);
    }

    private bool HasSchedule(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }
        for (int i = scheduledTimerList.Count - 1; i >= 0; i--)
        {
            if (scheduledTimerList[i].name == name)
            {
                return true;
            }
        }
        return false;
    }

#region Remove
    public void Remove(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }
        for (int i = scheduledTimerList.Count - 1; i >= 0; i--)
        {
            if (scheduledTimerList[i].name == name)
            {
                scheduledTimerList[i].isRemoved = true;
            }
        }
        this.pauseHash.Remove(name);
    }

    public void RemoveType(TimerType type)
    {
        for (int i = scheduledTimerList.Count - 1; i >= 0; i--)
        {
            if (scheduledTimerList[i].type == type)
            {
                scheduledTimerList[i].isRemoved = true;
                if (!string.IsNullOrEmpty(scheduledTimerList[i].name)
                    && pauseHash.Contains(scheduledTimerList[i].name))
                {
                    pauseHash.Remove(scheduledTimerList[i].name);
                }
            }
        }
    }

    public void RemoveAll()
    {
        for (int i = scheduledTimerList.Count - 1; i >= 0; i--)
        {
            timerPool.Enqueue(scheduledTimerList[i]);
            scheduledTimerList.Remove(scheduledTimerList[i]);
        }
    }
#endregion

#region Pause
    public void Pause(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }
        if (!pauseHash.Contains(name))
            pauseHash.Add(name);
        for (int i = scheduledTimerList.Count - 1; i >= 0; i--)
        {
            if (scheduledTimerList[i].name == name)
            {
                scheduledTimerList[i].isPuased = true;
            }
        }
    }

    public void PauseType(TimerType type)
    {
        for (int i = scheduledTimerList.Count - 1; i >= 0; i--)
        {
            if (scheduledTimerList[i].type == type)
            {
                scheduledTimerList[i].isPuased = true;
                if (!string.IsNullOrEmpty(scheduledTimerList[i].name)
                    && !pauseHash.Contains(scheduledTimerList[i].name))
                {
                    pauseHash.Add(scheduledTimerList[i].name);
                }
                break;
            }
        }
    }

    public void PauseAll()
    {
        for (int i = scheduledTimerList.Count - 1; i >= 0; i--)
        {
            if (!string.IsNullOrEmpty(scheduledTimerList[i].name)
                && !pauseHash.Contains(scheduledTimerList[i].name))
                pauseHash.Add(scheduledTimerList[i].name);
            scheduledTimerList[i].isPuased = true;
        }
    }
#endregion

#region Resume
    public void Resume(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }
        if (pauseHash.Contains(name))
        {
            pauseHash.Remove(name);
        }
        for (int i = scheduledTimerList.Count - 1; i >= 0; i--)
        {
            if (scheduledTimerList[i].name == name)
            {
                scheduledTimerList[i].isPuased = false;
            }
        }
    }

    public void ResumeType(TimerType type)
    {
        for (int i = scheduledTimerList.Count - 1; i >= 0; i--)
        {
            if (scheduledTimerList[i].type == type)
            {
                scheduledTimerList[i].isPuased = false;
                if (!string.IsNullOrEmpty(scheduledTimerList[i].name)
                    && pauseHash.Contains(scheduledTimerList[i].name))
                {
                    pauseHash.Remove(scheduledTimerList[i].name);
                }
            }
        }
    }

    public void ResumeAll()
    {
        for (int i = scheduledTimerList.Count - 1; i >= 0; i--)
        {
            scheduledTimerList[i].isPuased = false;
        }
        pauseHash.Clear();
    }
#endregion

    public void OnUpdate()
    {
        if (Time.timeScale == 0)
        {
            return;
        }
        if (scheduledTimerList.Count == 0)
        {
            return;
        }
        var time = Time.time;
        List<Timer> removeList = new List<Timer>();
        for (int i = 0; i < scheduledTimerList.Count; i++)
        {
            var timer = scheduledTimerList[i];
            if (timer.isRemoved)
            {
                timerPool.Enqueue(timer);
                removeList.Add(timer);
            }
            else if (timer.isPuased)
            {
                timer.triggerTime += Time.deltaTime / Time.timeScale;
            }
            else if (time >= timer.triggerTime)
            {
                timer.triggerHandler?.Invoke();
                if (timer.loop)
                {
                    timer.triggerTime += timer.delay;
                }
                else
                {
                    timerPool.Enqueue(timer);
                    removeList.Add(timer);
                }
            }
        }
        for (int i = 0; i < removeList.Count; i++)
        {
            scheduledTimerList.Remove(removeList[i]);
            if (!string.IsNullOrEmpty(removeList[i].name)
                && pauseHash.Contains(removeList[i].name))
            {
                pauseHash.Remove(removeList[i].name);
            }
        }
        if (scheduledTimerList.Count == 0)
        {
            UpdateManager.Stuff.Remove(this);
        }
    }
}