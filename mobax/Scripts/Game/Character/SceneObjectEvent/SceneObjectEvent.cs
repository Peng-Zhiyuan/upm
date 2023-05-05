using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class SceneObjectEvent
{
    private Dictionary<int, Delegate> m_EventTable = new Dictionary<int, Delegate>();
    private Dictionary<string, Delegate> m_DelegateTable = new Dictionary<string, Delegate>();
    private Dictionary<string, EventDelegate> m_EventDelegateTable = new Dictionary<string, EventDelegate>();

    private void OnListenerAdding(int eventType, string key, Delegate action)
    {
        if (!m_EventTable.ContainsKey(eventType))
        {
            m_EventTable.Add(eventType, null);
        }
        if (!m_DelegateTable.ContainsKey(key))
        {
            m_DelegateTable.Add(key, null);
        }
        m_DelegateTable[key] = action;
        m_EventTable[eventType] = Delegate.Combine(m_EventTable[eventType], action);
    }

    private void OnListenerRemoving(int eventType, string key)
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
                    m_EventDelegateTable[key].Clear();
                    m_EventDelegateTable.Remove(key);
                }
            }
            else
            {
                //LogMgr.LogError(string.Format("未找到委托类型{0}", key), LogAuthor.Ck);
            }
        }
        else
        {
            //LogMgr.LogError(string.Format("未找到事件类型{0}", eventType), LogAuthor.Ck);
        }
    }

    private void OnBroadcast<T>(int eventType, T args)
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

    private void ConvertDelegate(int eventType, string key, Action<object[]> action)
    {
        OnListenerAdding(eventType, key, action);
    }

    //注册事件
    public void AddListener<T>(int eventType, object obj, Action action)
    {
        string key = eventType.ToString() + obj.ToString();
        EventDelegate d = new EventDelegate();
        d.m_Action = action;
        m_EventDelegateTable.Add(key, d);
        ConvertDelegate(eventType, key, d.CallbackEvent);
    }

    public void AddListener(int eventType, object obj, Action<object[]> action)
    {
        string key = eventType.ToString() + obj.ToString();
        EventDelegate d = new EventDelegate();
        d.m_ActionParams = action;
        m_EventDelegateTable.Add(key, d);
        ConvertDelegate(eventType, key, d.CallbackEventParams);
    }

    //移除事件
    public void RemoveListener<T>(int eventType, object obj, Action action)
    {
        string key = eventType.ToString() + obj.ToString();
        OnListenerRemoving(eventType, key);
    }

    public void RemoveListener(int eventType, object obj, Action<object[]> action)
    {
        string key = eventType.ToString() + obj.ToString();
        OnListenerRemoving(eventType, key);
    }

    //广播事件
    public void Broadcast(int eventType)
    {
        Broadcast(eventType, null);
    }

    public void Broadcast(int eventType, params object[] args)
    {
        OnBroadcast(eventType, args);
    }

    //清除多余的事件表
    public void Cleanup()
    {
        List<int> eventToRemove = new List<int>();
        foreach (KeyValuePair<int, Delegate> pair in m_EventTable)
        {
            Delegate[] list = pair.Value.GetInvocationList();
            if (list.Length == 0)
            {
                eventToRemove.Add(pair.Key);
                if (m_DelegateTable.ContainsValue(pair.Value))
                {
                    foreach (KeyValuePair<string, Delegate> temp in m_DelegateTable)
                    {
                        if (temp.Value == pair.Value)
                        {
                            m_DelegateTable.Remove(temp.Key);
                            m_EventDelegateTable[temp.Key].Clear();
                            m_EventDelegateTable.Remove(temp.Key);
                            break;
                        }
                    }
                }
            }
        }
        for (int i = 0; i < eventToRemove.Count; i++)
        {
            m_EventTable.Remove(eventToRemove[i]);
        }
    }
}