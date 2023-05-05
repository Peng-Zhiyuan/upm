using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public partial class ListView : MonoBehaviour
{
    [ShowInInspector]
    public RectTransform Prototype
    {
        get
        {
            return this.DataListRoot.viewPrototype;
        }
        set
        {
            this.DataListRoot.viewPrototype = value;
        }
    }


    [ShowInInspector]
    public LayoutGroup LayoutGroup
    {
        get
        {
            return this.DataListRoot?.content?.GetComponent<LayoutGroup>();
        }
    }

    [ShowInInspector]
    public bool FirstViewHasApearAnimation
    {
        get
        {
            if (this.DataListRoot.Count == 0)
            {
                return false;
            }
            var view = this.DataListRoot.GetView(0);
            var b = AnimatorUtil.HasState(view.gameObject, AnimatorUtil.EventType.Apear);
            return b;
        }
    }

    [ShowInInspector]
    public bool FirstViewHasDispearAnimation
    {
        get
        {
            if (this.DataListRoot.Count == 0)
            {
                return false;
            }
            var view = this.DataListRoot.GetView(0);
            var b = AnimatorUtil.HasState(view.gameObject, AnimatorUtil.EventType.Disapear);
            return b;
        }
    }

    [ShowInInspector]
    public async void TestDisapearAnimation(AnimatorUtil.EventType eventType)
    {
        if (this.DataListRoot.Count == 0)
        {
            return;
        }
        var firstView = this.ViewList[0];
        Debug.Log($"{eventType} -> {firstView.gameObject}");
        await AnimatorUtil.SendEventThenWaitState(firstView.gameObject, eventType);
        await Task.Delay(500);
        var animator = firstView.GetComponent<Animator>();
        if(animator != null)
        {
            animator.Rebind();
        }
        
    }

    private void Start()
    {
        this.AutoMargin();
    }

    [ShowInInspector]
    public bool IsAutoHorizenPadding
    {
        get
        {
            if (this.LayoutGroup is GridLayoutGroup grid)
            {
                if (grid.startCorner == GridLayoutGroup.Corner.UpperLeft)
                {
                    return true;
                }
            }
            return false;
        }
    }

    [ShowInInspector]
    public int AutoHorizenPadding
    {
        get
        {
            if(!this.IsAutoHorizenPadding)
            {
                return -1;
            }
            var grid = this.LayoutGroup as GridLayoutGroup;
            var rt = this.GetComponent<RectTransform>();
            var contentWidth = rt.rect.width;
            var cellWidth = grid.cellSize.x;
            var spaceWidth = grid.spacing.x;
            var rest1 = contentWidth % (cellWidth + spaceWidth);
            var rest2 = 0f;
            if (rest1 >= cellWidth)
            {
                rest2 = rest1 - cellWidth;
            }
            else
            {
                rest2 = rest1 + spaceWidth;
            }
            var margin = rest2 / 2;
            return (int)margin;
        }
    }

    public void AutoMargin()
    {
        if(this.IsAutoHorizenPadding)
        {
            var grid = this.LayoutGroup as GridLayoutGroup;
            var margin = this.AutoHorizenPadding;
            grid.padding.left = (int)margin;
            grid.padding.right = (int)margin;
            //Debug.Log($"auto padding: {margin}, contentWidth: {contentWidth}, cellWidth: {cellWidth}, spaceWidth: {spaceWidth}, rest1: {rest1}, rest2: {rest2}");
        }
    }

    public event Action<RectTransform, object> ViewCreated
    {
        add
        {
            this.DataListRoot.ViewCreated += value;
        }
        remove
        {
            this.DataListRoot.ViewCreated -= value;
        }
    }

    CancellationTokenSource cts;
    public void Reset(IList dataList, bool useAnimation = true)
    {
        this.DataListRoot.Reset(dataList);


        if (useAnimation && dataList.Count > 0)
        {
            var firstView = this.DataListRoot.GetView(0);
            var hasState = AnimatorUtil.HasState(firstView.gameObject, AnimatorUtil.EventType.Apear);
            if(hasState)
            {
                var listView = this.ViewList;
                var transformList = from view in listView select view.GetComponent<Transform>();
                cts?.Cancel();
                cts = new CancellationTokenSource();
                CodeAnimation.PlayApearSequencely(transformList.ToList(), cts.Token);
            }
            else
            {
                cts?.Cancel();
            }
        }
        else
        {
            cts?.Cancel();
        }
        
    }

    public async void Insert(int index, object data, bool useAnimation = true) => await this.InsertAsync(index, data, useAnimation);

    public async Task InsertAsync(int index, object data, bool useAnimation = true)
    {
        if(useAnimation)
        {
            this.DataListRoot.ForeachViewWrapper(wrapper =>
            {
                var w = wrapper.GetComponent<ListViewElementWrapper>();
                w.RecordView();
                w.MoveToTemp();
            });
        }

        this.DataListRoot.Insert(index, data);


        if(useAnimation)
        {
            var newView = this.DataListRoot.GetView(index);
            var canvasGroup = newView.gameObject.GetOrAddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            LayoutRebuilder.ForceRebuildLayoutImmediate(this.DataListRoot.rectTransform());
            this.DataListRoot.ForeachViewWrapper(wrapper =>
            {
                var w = wrapper.GetComponent<ListViewElementWrapper>();
                w.MoveBack();
                w.TweenChildToSelf();
            });

            await Task.Delay(200);
            canvasGroup.alpha = 1f;
            AnimatorUtil.SendEvent(newView.gameObject, AnimatorUtil.EventType.Apear);

        }
    }

    public async Task RemoveAtIndexListAsync(List<int> removeIndexList)
    {
        removeIndexList.Sort();
        if (removeIndexList.Count > 0)
        {
            for (int i = removeIndexList.Count - 1; i >= 1; i--)
            {
                this.RemoveAt(i, false);
            }
            var index = removeIndexList[0];
            await this.RemoveAtAsync(index);
        }
    }

    public async void RemoveAt(int index, bool useAnimatin = true) => await this.RemoveAtAsync(index, useAnimatin);
    public async Task RemoveAtAsync(int index, bool useAnimation = true)
    {
        if(useAnimation)
        {
            var view = this.DataListRoot.GetView(index);
            var c = view.gameObject.GetOrAddComponent<CanvasGroup>();
            c.interactable = false;
            await AnimatorUtil.SendEventThenWaitState(view.gameObject, AnimatorUtil.EventType.Disapear);
            c.interactable = true;

            this.DataListRoot.ForeachViewWrapper(wrapper =>
            {
                var w = wrapper.GetComponent<ListViewElementWrapper>();
                w.RecordView();
                w.MoveToTemp();
            });
        }

        this.DataListRoot.RemoveAt(index);
        if(useAnimation)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.DataListRoot.rectTransform());
            this.DataListRoot.ForeachViewWrapper(wrapper =>
            {
                var w = wrapper.GetComponent<ListViewElementWrapper>();
                w.MoveBack();
                w.TweenChildToSelf();
            });
            await Task.Delay(200);
        }
        
    }

    public List<RectTransform> ViewList => this.DataListRoot.ViewList;

    public int GetIndexByView(RectTransform view) => this.DataListRoot.GetIndexByView(view);

    public int Count => this.DataListRoot.Count;

    public object GetData(int index) => this.DataListRoot.GetData(index);

    public RectTransform GetView(int index) => this.DataListRoot.GetView(index);
}
