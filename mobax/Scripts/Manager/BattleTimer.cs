using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
//using LuaInterface;

public class TimerEvent
{
    public float currentTick;
    public float delayTick;
    public Action<object[]> action;
    public object[] param;
    public bool isLua;
    public bool one_time;
    public float betweenTick = -1f;
}

public class BattleTimer : BattleComponent<BattleTimer>
{
    private List<TimerEvent> _list_action = new List<TimerEvent>();

    private List<TimerEvent> _list_readyto_remove = new List<TimerEvent>();

    public TimerEvent AddFrameEvent(float delayTick, Action<object[]> action, params object[] param)
    {
        TimerEvent timer = new TimerEvent();
        timer.currentTick = 0f;
        timer.delayTick = delayTick;
        timer.action = action;
        timer.param = param;
        timer.one_time = false;

        _list_action.Add(timer);
        return timer;
    }

    public TimerEvent AddFrameEvent(float delayTick, float betweenTick, Action<object[]> action, params object[] param)
    {
        TimerEvent timer = new TimerEvent();
        timer.currentTick = 0f;
        timer.delayTick = delayTick;
        timer.action = action;
        timer.param = param;
        timer.one_time = false;
        timer.betweenTick = betweenTick;

        _list_action.Add(timer);
        return timer;
    }

    public TimerEvent AddFrameEvent(float delayTick, Action<object[]> action)
    {
        TimerEvent timer = new TimerEvent();
        timer.currentTick = 0f;
        timer.delayTick = delayTick;
        timer.action = action;
        timer.one_time = true;
        _list_action.Add(timer);
        return timer;
    }

    public TimerEvent AddFrameEvent(float delayTick, float betweenTick, bool isOnce, Action<object[]> action)
    {
        TimerEvent timer = new TimerEvent();
        timer.currentTick = 0f;
        timer.delayTick = delayTick;
        timer.action = action;
        timer.betweenTick = betweenTick;
        timer.one_time = isOnce;
        _list_action.Add(timer);
        return timer;
    }

    public void RemoveFrameEvent(Action<object[]> action)
    {
        foreach (TimerEvent timer in _list_action)
        {
            if (timer.action == action)
            {
                _list_readyto_remove.Add(timer);
                return;
            }
        }
    }

    public void RemoveFrameEvent(TimerEvent timer)
    {
        if (_list_action.Contains(timer))
        {
            _list_action.Remove(timer);
        }
    }

    public void DelayCall(float delayTick, Action<object[]> action, params object[] param)
    {
        TimerEvent timer = new TimerEvent();
        timer.currentTick = 0f;
        timer.delayTick = delayTick;
        timer.action = action;
        timer.param = param;
        timer.one_time = true;

        _list_action.Add(timer);
    }

    public override void OnUpdate()
    {
        for (int i = 0; i < _list_action.Count; i++)
        {
            var timer = _list_action[i];
            if (timer.currentTick >= timer.delayTick)
            {
                timer.currentTick -= timer.delayTick;
                timer.action(timer.param);
                //如果只执行一次
                if (timer.one_time)
                {
                    _list_readyto_remove.Add(timer);
                }
                //如果执行多次
                else
                {
                    if (timer.betweenTick > 0)
                    {
                        timer.delayTick = timer.betweenTick;
                    }
                }
            }
            else
            {
                timer.currentTick += Time.deltaTime;
            }
        }

        foreach (TimerEvent timer in _list_readyto_remove)
        {
            _list_action.Remove(timer);
        }
        _list_readyto_remove.Clear();
    }

    // public static void SetTimeScale(float fScale)
    // {
    //     Time.timeScale = fScale;
    // }
    //
    // public static void ResetTimeScale()
    // {
    //     Time.timeScale = 1;
    // }
}