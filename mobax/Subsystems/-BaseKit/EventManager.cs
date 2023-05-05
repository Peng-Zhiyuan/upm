using System;
using System.Collections.Generic;

public class TriggerEventInfo
{
    public string EventType;
    public object Data;
}

public delegate bool EVENT_DELEGATE<T>(T arg);

public delegate bool EVENT_DELEGATE();

public class EventManager : Single<EventManager>
{
    private Dictionary<string, Delegate> mDelegates = new Dictionary<string, Delegate>();

    public void SendEvent(string eventType)
    {
        Delegate d = null;
        if (mDelegates.TryGetValue(eventType, out d))
        {
            Action callback = d as Action;
            if (callback != null)
            {
                callback();
            }
        }
    }

    public void SendEvent<T>(string eventType, T arg)
    {
        if (arg == null)
        {
            throw new ArgumentNullException("arg");
        }
        Delegate d = null;
        if (mDelegates.TryGetValue(eventType, out d))
        {
            Action<T> callback = d as Action<T>;
            if (callback != null)
            {
                callback(arg);
            }
        }
    }

    public List<TriggerEventInfo> mTriggerEvents = new List<TriggerEventInfo>();

    public void TriggerEvent(string eventType, string arg)
    {
        if (arg == null)
        {
            throw new ArgumentNullException("arg");
        }
        mTriggerEvents.Add(new TriggerEventInfo { EventType = eventType, Data = arg });
    }

    public void TriggerEvent(string eventType)
    {
        mTriggerEvents.Add(new TriggerEventInfo { EventType = eventType, Data = "" });
    }

    private void Update()
    {
        if (mTriggerEvents.Count == 0)
            return;
        for (int i = 0; i < mTriggerEvents.Count; i++)
        {
            var eventInfo = mTriggerEvents[i];
            Delegate d = null;
            if (mDelegates.TryGetValue(eventInfo.EventType, out d))
            {
                Action<string> callback = d as Action<string>;
                if (callback != null)
                {
                    callback(eventInfo.Data as string);
                }
                else
                {
                    (d as Action)?.Invoke();
                }
            }
        }
        mTriggerEvents.Clear();
    }

    public void AddListener(string eventType, Action listener)
    {
        Delegate d = null;
        if (mDelegates.TryGetValue(eventType, out d))
        {
            mDelegates[eventType] = Delegate.Combine(d, listener);
        }
        else
        {
            mDelegates[eventType] = listener;
        }
    }

    public void AddListener<T>(string eventType, Action<T> listener)
    {
        Delegate d = null;
        if (mDelegates.TryGetValue(eventType, out d))
        {
            mDelegates[eventType] = Delegate.Combine(d, listener);
        }
        else
        {
            mDelegates[eventType] = listener;
        }
    }

    public void RemoveListener(string eventType, Action listener)
    {
        Delegate d = null;
        if (mDelegates.TryGetValue(eventType, out d))
        {
            Delegate currentDel = Delegate.Remove(d, listener);
            if (currentDel == null)
            {
                mDelegates.Remove(eventType);
            }
            else
            {
                mDelegates[eventType] = currentDel;
            }
        }
    }

    public void RemoveListener<T>(string eventType, Action<T> listener)
    {
        Delegate d = null;
        if (mDelegates.TryGetValue(eventType, out d))
        {
            Delegate currentDel = Delegate.Remove(d, listener);
            if (currentDel == null)
            {
                mDelegates.Remove(eventType);
            }
            else
            {
                mDelegates[eventType] = currentDel;
            }
        }
    }

    public bool SendCallbackEvent(string eventType)
    {
        Delegate d = null;
        if (mDelegates.TryGetValue(eventType, out d))
        {
            EVENT_DELEGATE callback = d as EVENT_DELEGATE;
            if (callback != null)
            {
                return callback();
            }
        }
        return false;
    }

    public bool SendCallbackEvent<T>(string eventType, T arg)
    {
        if (arg == null)
        {
            throw new ArgumentNullException("arg");
        }
        Delegate d = null;
        if (mDelegates.TryGetValue(eventType, out d))
        {
            EVENT_DELEGATE<T> callback = d as EVENT_DELEGATE<T>;
            if (callback != null)
            {
                return callback(arg);
            }
        }
        return false;
    }

    public void AddCallbackListener<T>(string eventType, EVENT_DELEGATE<T> listener)
    {
        Delegate d = null;
        if (mDelegates.TryGetValue(eventType, out d))
        {
            mDelegates[eventType] = Delegate.Combine(d, listener);
        }
        else
        {
            mDelegates[eventType] = listener;
        }
    }

    public void AddCallbackListener(string eventType, EVENT_DELEGATE listener)
    {
        Delegate d = null;
        if (mDelegates.TryGetValue(eventType, out d))
        {
            mDelegates[eventType] = Delegate.Combine(d, listener);
        }
        else
        {
            mDelegates[eventType] = listener;
        }
    }

    public void RemoveCallbackListener<T>(string eventType, EVENT_DELEGATE<T> listener)
    {
        Delegate d = null;
        if (mDelegates.TryGetValue(eventType, out d))
        {
            Delegate currentDel = Delegate.Remove(d, listener);
            if (currentDel == null)
            {
                mDelegates.Remove(eventType);
            }
            else
            {
                mDelegates[eventType] = currentDel;
            }
        }
    }

    public void RemoveCallbackListener(string eventType, EVENT_DELEGATE listener)
    {
        Delegate d = null;
        if (mDelegates.TryGetValue(eventType, out d))
        {
            Delegate currentDel = Delegate.Remove(d, listener);
            if (currentDel == null)
            {
                mDelegates.Remove(eventType);
            }
            else
            {
                mDelegates[eventType] = currentDel;
            }
        }
    }

    public void ClearAllListener()
    {
        mDelegates.Clear();
    }
}