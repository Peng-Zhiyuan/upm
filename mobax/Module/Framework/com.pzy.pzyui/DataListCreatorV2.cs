using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public partial class DataListCreatorV2 : MonoBehaviour
{
    public RectTransform viewPrototype;
    public RectTransform wrapperPrototype;
    public RectTransform content;
    public RectTransform poolRoot;
    public event Action<RectTransform, object> ViewCreated;

    [ShowInInspector, ReadOnly]
    List<DataEntry> entryList = new List<DataEntry>();

    [ShowInInspector, ReadOnly]
    GameObjectPoolV2 pool = new GameObjectPoolV2();

    public class DataEntry
    {
        public object data;
        public RectTransform viewWrapper;
        public RectTransform view;
    }


    void Awake()
    {
        if (viewPrototype != null)
        {
            // 如果非预制体，就要先隐藏
            var scene = viewPrototype.gameObject.scene;
            if (!string.IsNullOrEmpty(scene.name))
            {
                viewPrototype.SetActive(false);
            }
        }

        this.pool.SetStorageRoot(this.poolRoot.gameObject);
    }

    public void ForeachViewWrapper(Action<RectTransform> handler)
    {
        foreach(var entry in this.entryList)
        {
            handler.Invoke(entry.viewWrapper);
        }
    }

    public void ForeachData(Action<object> handler)
    {
        foreach (var entry in this.entryList)
        {
            handler.Invoke(entry.data);
        }
    }

    public RectTransform GetView(int index) => this.entryList[index].view;

    public void Insert(int index, object data)
    {
        var entry = new DataEntry();
        entry.data = data;
        var wrapper = this.pool.Reuse(this.wrapperPrototype, this.content.transform, true);
        wrapper.transform.SetSiblingIndex(index);
        var view = this.pool.Reuse(this.viewPrototype, wrapper, true);
        wrapper.sizeDelta = this.viewPrototype.sizeDelta;
        ViewCreated?.Invoke(view.rectTransform(), data);
        entry.viewWrapper = wrapper;
        entry.view = view;
        this.entryList.Insert(index, entry);
    }

    public void Reset(IList dataList)
    {
        this.CleanEntry();
        if(dataList != null)
        {
            foreach (var data in dataList)
            {
                this.Add(data);
            }
        }
    }


    void Add(object data)
    {
        var entry = new DataEntry();
        entry.data = data;
        //var wrapper = this.CreateWrapper(this.content.transform);
        var wrapper = this.pool.Reuse(this.wrapperPrototype, this.content.transform, true);
        var view = this.pool.Reuse(this.viewPrototype, wrapper, true);
        wrapper.sizeDelta = this.viewPrototype.sizeDelta;
        ViewCreated?.Invoke(view.rectTransform(), data);
        entry.viewWrapper = wrapper;
        entry.view = view;
        this.entryList.Add(entry);
    }

    public void RemoveAt(int index)
    {
        var entry = this.entryList[index];
        var view = entry.view;
        var viewWrapper = entry.viewWrapper;
        if(view != null)
        {
            this.pool.Recycle(entry.view.gameObject);
        }
        if(viewWrapper != null)
        {
            this.pool.Recycle(entry.viewWrapper.gameObject);
        }
        this.entryList.RemoveAt(index);
    }

    public int Count => this.entryList.Count;

    public void CleanEntry()
    {
        foreach(var entry in this.entryList)
        {
            var view = entry.view;
            var viewWrapper = entry.viewWrapper;
            if (view != null)
            {
                this.pool.Recycle(entry.view.gameObject);
            }
            if (viewWrapper != null)
            {
                this.pool.Recycle(entry.viewWrapper.gameObject);
            }
        }
        this.entryList.Clear();
    }

    public List<RectTransform> ViewList
    {
        get
        {
            var ret = new List<RectTransform>();
            foreach(var entry in this.entryList)
            {
                var view = entry.view;
                if(view != null)
                {
                    ret.Add(view);
                }
            }
            return ret;
        }
    }

    public int GetIndexByView(RectTransform view)
    {
        for (int i = 0; i < this.entryList.Count; i++)
        {
            var entry = this.entryList[i];
            if(entry.view == view)
            {
                return i;
            }
        }
        return -1;
    }

    public object GetData(int index) => this.entryList[index].data;
}