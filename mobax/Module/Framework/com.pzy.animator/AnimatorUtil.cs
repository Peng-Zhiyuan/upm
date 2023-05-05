using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

public static class AnimatorUtil
{
    public static async Task WaitUtilNotInStateAsync(Animator animator, string name, int layerIndex = 0)
    {
        while(true)
        {
            var currentInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
            var nextInfo = animator.GetNextAnimatorStateInfo(layerIndex);
            var currentIsTarget = currentInfo.IsName(name);
            var nextIfTarget = nextInfo.IsName(name);
            if(!currentIsTarget && !nextIfTarget)
            {
                return;
            }
            if (currentIsTarget)
            {
                if (currentInfo.normalizedTime >= 1f)
                {
                    return;
                }
            }
            await Task.Delay(10);
            if (animator == null)
            {
                return;
            }
        }
    }

    public static bool HasState(GameObject obj, EventType associatedEvent)
    {
        var eventName = GetEventName(associatedEvent);
        var stateName = GetStateName(eventName);
        return HasState(obj, stateName);
    }

    public static bool HasState(GameObject obj, string stateName)
    {
        var animator = obj.GetComponent<Animator>();
        if(animator == null)
        {
            return false;
        }
        var stateId = Animator.StringToHash(stateName);
        var b = animator.HasState(0, stateId);
        return b;
    }

    public enum EventType
    {
        Apear,
        Disapear,
        Enter,
        Exit,
    }

    static Dictionary<EventType, string> eventTypeToNameDic = new Dictionary<EventType, string>();
    static string GetEventName(EventType eventType)
    {
        var b = eventTypeToNameDic.TryGetValue(eventType, out var ret);
        if (b)
        {
            return ret;
        }
        var name = EventTypeToNameStirng(eventType);
        eventTypeToNameDic[eventType] = name;
        return name;
    }

    static string EventTypeToNameStirng(EventType eventType)
    {
        var str = eventType.ToString();
        var firstCh = str[0];
        var firstChLower = char.ToLower(firstCh);
        var ret = $"{firstChLower}{str.Substring(1)}";
        return ret;
    }


    static Dictionary<string, string> eventNameToStateDic = new Dictionary<string, string>();
    static string GetStateName(string eventName)
    {
        var b = eventNameToStateDic.TryGetValue(eventName, out var ret);
        if(b)
        {
            return ret;
        }
        var stateName = EventNameToWaitStateName(eventName);
        eventNameToStateDic[eventName] = stateName;
        return stateName;
    }

    static string EventNameToWaitStateName(string eventName)
    {
        var firstCh = eventName[0];
        var firstChUpper = char.ToUpper(firstCh);
        var ret = $"{firstChUpper}{eventName.Substring(1)}ing";
        return ret;
    }


    public static async void SendEvent(GameObject gameObject, EventType eventType)
    {
        await SendEventThenWaitState(gameObject, eventType);
    }

    public static async Task SendEventThenWaitState(GameObject gameObject, EventType eventType, bool waitState = true)
    {
        var eventName = GetEventName(eventType);
        await SendEventThenWaitState(gameObject, eventName, waitState);
    }

    public static async Task SendEventThenWaitState(GameObject gameObject, string eventName, bool waitState = true)
    {
        if(gameObject == null)
        {
            return;
        }
        var animator = gameObject.GetComponent<Animator>();
        if(animator == null)
        {
            return;
        }
        animator.SetTrigger(eventName);
        animator.Update(0f);
        if(waitState)
        {
            var stateName = GetStateName(eventName);
            await AnimatorUtil.WaitUtilNotInStateAsync(animator, stateName);
        }
    }

    public static void ResetToDefaultState(GameObject go)
    {
        var animator = go.GetComponent<Animator>();
        if(animator != null)
        {
            animator.CrossFade("Default", 0f);
            animator.Update(0f);
            animator.Update(0f);
        }
    }
}
