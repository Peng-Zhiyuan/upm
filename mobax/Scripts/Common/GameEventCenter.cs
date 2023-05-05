using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class EventDelegate {
    public Action m_Action;
    public Action<object[]> m_ActionParams;

    public void CallbackEvent(object[] args) {
        m_Action();
    }

    public void CallbackEventParams(object[] args)
    {
        m_ActionParams(args);
    }
    public void Clear() {
        m_Action = null;
        m_ActionParams = null;
    }
}

public static class GameEventCenter
{
    private static Dictionary<int, Delegate> m_EventTable = new Dictionary<int, Delegate>();
    private static Dictionary<int, int> m_DelegateCount = new Dictionary<int, int>();
    private static Dictionary<string, Delegate> m_DelegateTable = new Dictionary<string, Delegate>();
    //private static Dictionary<string, EventDelegate> m_EventDelegateTable = new Dictionary<string, EventDelegate>();

    private static void OnListenerAdding(int eventType,string key ,Delegate action)
    {
        if (!m_EventTable.ContainsKey(eventType))
        {
            m_EventTable.Add(eventType, null);
        }

        if (!m_DelegateCount.ContainsKey(eventType))
        {
            m_DelegateCount.Add(eventType, 0);
        }

        if (!m_DelegateTable.ContainsKey(key))
        {
            m_DelegateTable.Add(key, null);
        }

        m_DelegateTable[key] = action;
        m_EventTable[eventType] = Delegate.Combine(m_EventTable[eventType], action);
        m_DelegateCount[eventType]++;
    }

    private static void OnListenerRemoving(int eventType, string key)
    {
        if (m_EventTable.ContainsKey(eventType))
        {
            if (m_DelegateTable.ContainsKey(key))
            {
                Delegate d;
                if (m_DelegateTable.TryGetValue(key, out d))
                {
                    m_EventTable[eventType] = Delegate.Remove(m_EventTable[eventType], d);
                    m_DelegateTable.Remove(key);
                    m_DelegateCount[eventType]--;
                    //m_EventDelegateTable[key].Clear();
                    //m_EventDelegateTable.Remove(key);
                }
            }
            else {
                //LogMgr.LogWarning(string.Format("未找到委托类型{0}", key), LogAuthor.All);
            }
        }
        else
        {
	        //LogMgr.LogWarning(string.Format("未找到事件类型{0}", eventType), LogAuthor.All);
        }
    }
    private static void OnBroadcast<T>(int eventType, T args)
    {
        Delegate d;
        if (m_EventTable.TryGetValue(eventType, out d))
        {
            Action<T> callback = d as Action<T>;
            if (callback != null)
            {
                callback(args);
            }
        }
    }

    private static void ConvertDelegate(int eventType, string key, Action<object[]> action) {
        OnListenerAdding(eventType, key, action);
    }

    //注册事件
    public static void AddListener<T>(int eventType, object obj, Action action)
    {
        string key = eventType.ToString() + obj.ToString();
        EventDelegate d = new EventDelegate();
        d.m_Action = action;
        //m_EventDelegateTable.Add(key, d);
        ConvertDelegate(eventType, key, d.CallbackEvent);
    }
    public static void AddListener(int eventType, object obj, Action<object[]> action)
    {
        string key = eventType.ToString() + obj.ToString();
        EventDelegate d = new EventDelegate();
        d.m_ActionParams = action;
        //m_EventDelegateTable.Add(key, d);
        ConvertDelegate(eventType, key, d.CallbackEventParams);
    }
    //移除事件
    public static void RemoveListener<T>(int eventType, object obj)
    {
        string key = eventType.ToString() + obj.ToString();
        OnListenerRemoving(eventType, key);
    }
    public static void RemoveListener(int eventType, object obj)
    {
        string key = eventType.ToString() + obj.ToString();
        OnListenerRemoving(eventType, key);
    }
    //广播事件
    public static void Broadcast(int eventType)
    {
        Broadcast(eventType,null);
    }
    public static void Broadcast(int eventType, params object[] args)
    {
        OnBroadcast(eventType, args);
    }

    public static void CleanAll()
    {
        m_DelegateCount.Clear();
        m_EventTable.Clear();
        m_DelegateTable.Clear();
    }
    //清除多余的事件表
    public static void Cleanup()
    {
        List<int> eventToRemove = new List<int>();
        foreach (KeyValuePair<int, Delegate> pair in m_EventTable)
        {
            Delegate[] list = pair.Value.GetInvocationList();
            if (list.Length == 0)
            {
                eventToRemove.Add(pair.Key);

                if (m_DelegateTable.ContainsValue(pair.Value)) {
                    foreach (KeyValuePair<string, Delegate> temp in m_DelegateTable)
                    {
                        if (temp.Value == pair.Value) {
                            m_DelegateTable.Remove(temp.Key);
                            //m_EventDelegateTable[temp.Key].Clear();
                            //m_EventDelegateTable.Remove(temp.Key);
                            break;
                        }
                    }
                }
            }
        }

        for (int i = 0; i < eventToRemove.Count; i++)
        {
            m_EventTable.Remove(eventToRemove[i]);
            m_DelegateCount.Remove(eventToRemove[i]);
        }
    }

    public static int GetDelegateCount(int eventType)
    {
        int count = 0;
        m_DelegateCount.TryGetValue(eventType,out count);
        return count;
    }
}

public static class GameEventManager
{
    public static void AddListener(int eventType, object obj, Action<object[]> action)
    {
        GameEventCenter.AddListener(eventType, obj, action);
    }
    public static void RemoveListener(int eventType, object obj)
    {
        GameEventCenter.RemoveListener(eventType,obj);
    }
    public static void Broadcast(int eventType)
    {
        GameEventCenter.Broadcast(eventType,null);
    }
    public static void Broadcast(int eventType, params object[] args)
    {
        GameEventCenter.Broadcast(eventType,args);
    }
    public static void Cleanup()
    {
        GameEventCenter.Cleanup();
    }
    public static int GetDelegateCount(int eventType)
    {
        return GameEventCenter.GetDelegateCount(eventType);
    }
}