using System;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEngine : InstanceStuff<TriggerEngine>
{
    public List<Trigger> eventList;

    bool _isOn;

    public bool IsOn
    {
        get { return this._isOn; }
        set
        {
            if (this._isOn == value)
            {
                return;
            }

            this._isOn = value;
            if (value)
            {
                this.OnOn();
            }
            else
            {
                this.OnOff();
            }
        }
    }

    void OnOn()
    {
        this.TryNotifyAll("enabled");
    }

    public Action<bool, Trigger> OnExecuteCompelte;

    public int executingCount;
    public List<string> executingIdList = new List<string>();

    public async void Execute(Trigger e)
    {
        executingCount++;
        e.isProcessed = true;
        executingIdList.Add(e.id);
        var success = await ScriptEventExecutingUtil.ExecuteAsync(e, this);
        executingIdList.Remove(e.id);
        executingCount--;
        OnExecuteCompelte?.Invoke(success, e);
    }

    void OnOff()
    {
    }

    public void TrySetTriggerEnable(string id, bool b)
    {
        var e = this.GetEventById(id);
        if (e == null)
        {
            return;
        }

        e.enable = b;
        if (b)
        {
            this.TryNotify(e, "enabled");
        }
    }

    public void Load(string script, Action<List<Trigger>> onTriggerCreatedHandler = null)
    {
        var reader = new ScriptEventReader(script);
        var infoList = reader.ReadScriptEventInfoList();
        if (this.eventList == null)
        {
            this.eventList = new List<Trigger>();
        }

        this.eventList.AddRange(infoList);
        onTriggerCreatedHandler?.Invoke(infoList);
        foreach (var info in infoList)
        {
            TryNotify(info, "enabled");
        }
    }

    public void DisableAllEvent()
    {
        foreach (var info in this.eventList)
        {
            info.enable = false;
        }
    }

    public Trigger GetEventById(string id)
    {
        foreach (var one in this.eventList)
        {
            if (one.id == id)
            {
                return one;
            }
        }

        return null;
    }

    public void ForceExecute(string id, bool allowNotFound = false)
    {
        var trigger = GetEventById(id);
        if (trigger != null)
        {
            this.Execute(trigger);
        }
        else
        {
            if (!allowNotFound)
            {
                throw new Exception($"[TriggerEngine] trigger '{id}' not found");
            }
        }
    }

    void TryNotify(Trigger info, string msg)
    {
        if (!this.IsOn)
        {
            return;
        }

        if (info.isProcessed)
        {
            return;
        }

        if (!info.enable)
        {
            return;
        }

        if (info.when == msg)
        {
            var match = false;
            var expression = info.whenExpresion;
            if (expression != null && expression.Count > 0)
            {
                var result = Expression.Resolve(expression);
                if (result.type == ResultValueType.Bool)
                {
                    if (result.boolValue == true)
                    {
                        match = true;
                    }
                }
            }
            else
            {
                match = true;
            }

            if (match)
            {
                this.Execute(info);
            }
        }
    }

    void TryNotifyAll(string msg)
    {
        if (this.eventList == null)
        {
            return;
        }

        foreach (var info in this.eventList)
        {
            TryNotify(info, msg);
        }
    }

    public void Notify(string msg)
    {
        Debug.Log("[Script] receive notify: " + msg);
        this.InvokeOnetimeListner(msg);
        if (this.eventList == null)
        {
            return;
        }

        this.TryNotifyAll(msg);
    }


    // pzy:
    // 支持一次性监听

    Dictionary<string, List<Action>> eventIdToOneTimeListnerDic = new Dictionary<string, List<Action>>();

    void InvokeOnetimeListner(string eventName)
    {
        var b = eventIdToOneTimeListnerDic.TryGetValue(eventName, out var list);
        if (!b)
        {
            return;
        }

        var copyList = new List<Action>(list);
        list.Clear();
        foreach (var l in copyList)
        {
            l.Invoke();
        }
    }

    public void RegisterOnetimeListner(string eventName, Action action)
    {
        var list = DictionaryUtil.GetOrCreateList(eventIdToOneTimeListnerDic, eventName);
        list.Add(action);
    }
}