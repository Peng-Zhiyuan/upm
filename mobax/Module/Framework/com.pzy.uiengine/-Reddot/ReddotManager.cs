using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ReddotManager 
{
    Dictionary<GameObject, Func<bool>> reddotToProviderDic = new Dictionary<GameObject, Func<bool>>();

    public void Register(GameObject reddot, Func<bool> provider)
    {
        reddotToProviderDic[reddot] = provider;
    }

    public Func<bool> this[GameObject reddot]
    {
        set
        {
            this.Register(reddot, value);
        }
    }

    public Func<bool> this[Component reddot]
    {
        set
        {
            this.Register(reddot.gameObject, value);
        }
    }

    public void Refresh()
    {
        foreach (var kv in this.reddotToProviderDic)
        {
            var reddot = kv.Key;
            var provider = kv.Value;
            var visible = false;
            if (provider != null)
            {
                visible = provider.Invoke();
            }
            reddot.SetActive(visible);
        }
    }
}
