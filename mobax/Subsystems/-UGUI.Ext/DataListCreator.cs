using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataListCreator : MonoBehaviour
{
    public GameObject prototype;
    public RectTransform content;
    public List<RecycledGameObject> viewList = new List<RecycledGameObject>();

    public Action<object, Transform> ViewSetter;
    public Action<Transform> RecycleView;


    void Awake()
    {
        if (null != prototype)
        {
            // 如果非预制体，就要先隐藏
            var scene = prototype.scene;
            if (!string.IsNullOrEmpty(scene.name))
            {
                prototype.SetActive(false);
            }
        }
    }

    protected IList _dataList;
    public virtual IList DataList
    {
        set
        {
            // 回收所有数据视图
            foreach (var view in viewList)
            {
                view.Recycle();
                UnbindView(view);
            }
            viewList.Clear();
            viewToDataDic.Clear();
            dataToViewDic.Clear();
            dataToIndexDic.Clear();
            this._dataList = value;
            if (value != null)
            {
                for (int i = 0; i < value.Count; i++)
                {
                    var data = value[i];
                    var bucket = BucketManager.Stuff.GetBucket(UIEngine.LatestNavigatePageName);
                    var view = bucket.Pool.Reuse<RecycledGameObject>(prototype);
                    view.transform.SetParent(content, false);
                    view.transform.localScale = this.prototype.transform.localScale;
                    view.name = i.ToString();
                    BindView(view, data, i);
                    viewList.Add(view);
                }
            }
        }
        get { return this._dataList; }
    }

    private Dictionary<RecycledGameObject, object> viewToDataDic = new Dictionary<RecycledGameObject, object>();
    private Dictionary<object, RecycledGameObject> dataToViewDic = new Dictionary<object, RecycledGameObject>();
    private Dictionary<object, int> dataToIndexDic = new Dictionary<object, int>();

    void BindView(RecycledGameObject view, object data, int index)
    {
        dataToIndexDic[data] = index;
        viewToDataDic[view] = data;
        dataToViewDic[data] = view;
        ViewSetter?.Invoke(data, view.transform);
    }

    void UnbindView(RecycledGameObject view)
    {
        RecycleView?.Invoke(view.transform);
    }

    public object ViewToData(RecycledGameObject go)
    {
        var view = go.GetComponent<RecycledGameObject>();
        if (viewToDataDic.ContainsKey(view))
        {
            return viewToDataDic[view];
        }
        return null;
    }

    public RecycledGameObject DataToView(object data)
    {
        if (dataToViewDic.ContainsKey(data))
        {
            return dataToViewDic[data];
        }
        return null;
    }

    public int DataToIndex(object data)
    {
        if(this.dataToIndexDic.ContainsKey(data))
        {
            return dataToIndexDic[data];
        }
        return -1;
    }

    public void CleanContent()
    {
        TransformUtil.RemoveAllChildren(this.content);
    }
}