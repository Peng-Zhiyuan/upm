using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class View : RecycledGameObject
{

    [ShowInInspector, ReadOnly]
    int refreshCount;
    bool dirty;

    [ShowInInspector]
    public void SetDirty()
    {
        dirty = true;
    }

    public void CancelDirty()
    {
        this.dirty = false;
    }
    public void RefreshIfNeed()
    {
        if (dirty)
        {
            this.dirty = false;
            refreshCount++;
            this.Refresh();
        }
    }

    public void ForceRefresh()
    {
        this.dirty = true;
        this.RefreshIfNeed();
    }

    protected virtual void Refresh()
    {

    }

    void Update()
    {
        this.RefreshIfNeed();
    }
}
