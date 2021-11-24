using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HierarchyFloating : Floating
{

    public Transform itemRoot;
    public HierarchyItemView itemSample;

    public void Awake()
    {
        this.itemSample.gameObject.SetActive(false);
    }

    public void OnButton(string msg)
    {
        if(msg == "regenerate")
        {
            this.RerenerateData();
            this.RefreshView();
        }
        else if(msg == "ok")
        {
            UIEngine.Stuff.RemoveFloating<HierarchyFloating>();
        }
    }

    private HierachyItemData rootData;
    private void RerenerateData()
    {
        //TransformUtil.RemoveAllChildren(this.itemRoot);
        var hot = GameObject.Find("PageLayer").transform;
        this.rootData = this.GenerateData(null, hot);
    }

    private HierachyItemData GenerateData(HierachyItemData parentData, Transform node)
    {
        var data = new HierachyItemData();
        data.transform = node;
        data.SetParent(parentData);

        //var view = this.CreateItemView();
        //view.Data = data;

        // child
        var childCount = node.childCount;
        for(var i = 0; i < childCount; i++)
        {
            var child = node.GetChild(i);
            GenerateData(data, child);
        }
        return data;
    }

    private void GenerateView(HierachyItemData data)
    {
        var view = this.CreateItemView();
        view.Data = data;

        var isFoldout = data.isFoldout;

        if(isFoldout)
        {
            foreach (var child in data.childList)
            {
                this.GenerateView(child);
            }
        }
    }

    private void RefreshView()
    {
        RemoveAllChildren(this.itemRoot);
        if(this.rootData != null)
        {
            this.GenerateView(this.rootData);
        }
    }

    private HierarchyItemView CreateItemView()
    {
        var go = GameObject.Instantiate(this.itemSample.gameObject);
        var item = go.GetComponent<HierarchyItemView>();
        go.transform.SetParent(this.itemRoot, false);
        item.Clicked = this.OnItemViewClicked;
        item.SelfActiveChanged = this.OnSelfActiveChnaged;
        go.SetActive(true);
        return item;
    }

    private void OnItemViewClicked(HierarchyItemView view)
    {
        view.Data.isFoldout = !view.Data.isFoldout;
        this.RefreshView();
    }

    private void OnSelfActiveChnaged(HierarchyItemView view)
    {
        this.RefreshView();
    }

    public static void RemoveAllChildren(Transform a)
    {
        if (a == null)
        {
            return;
        }
        for (int i = 0; i < a.childCount; i++)
        {
            GameObject.Destroy(a.GetChild(i).gameObject);
        }
        a.DetachChildren();
    }
}
