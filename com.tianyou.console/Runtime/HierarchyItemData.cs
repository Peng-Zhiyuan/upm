using System;
using UnityEngine;
using System.Collections.Generic;

public class HierachyItemData
{
    public Transform transform;

    public HierachyItemData parent;

    public List<HierachyItemData> childList = new List<HierachyItemData>();

    public void SetParent(HierachyItemData parent)
    {
        if(this.parent != null)
        {
            this.parent.childList.Remove(this);
        }
        this.parent = parent;
        if(this.parent != null)
        {
            this.parent.childList.Add(this);
        }
    }

    public int Generation
    {
        get
        {
            var ret = 0;
            var pointer = this;
            while(pointer.parent != null)
            {
                ret++;
                pointer = pointer.parent;
            }
            return ret;
        }
    }

    public string GameObjectName
    {
        get
        {
            return this.transform.name;
        }
    }

    public bool isFoldout;

    public bool HasChild
    {
        get
        {
            var childCount = this.childList.Count;
            return childCount > 0;
        }
    }

    public bool IsActiveSelf
    {
        get
        {
            var b = this.transform.gameObject.activeSelf;
            return b;
        }
        set
        {
            this.transform.gameObject.SetActive(value);
        }
    }

    public bool IsActiveInHierarchy
    {
        get
        {
            var b = this.transform.gameObject.activeInHierarchy;
            return b;
        }
    }


}
