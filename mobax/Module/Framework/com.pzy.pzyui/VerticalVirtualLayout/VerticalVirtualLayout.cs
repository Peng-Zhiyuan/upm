using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Threading.Tasks;


public partial class VerticalVirtualLayout : MonoBehaviour, ILayoutGroup
{
    public void SetLayoutHorizontal()
    {
        if(this.controlChildWidth)
        {
            // 更新所有子项目的宽度
            var width = this.ContentAllocableWidth;
            this.entryManager.ForeachView(view =>
            {
                var isExpandWidth = LayoutUtility.GetFlexibleWidth(view) != 0;
                var isForceExpandWidth = this.childForceExpandWidth;
                if (isExpandWidth || isForceExpandWidth)
                {
                    view.SetSizeDeltaX(width);
                }
            });
        }
        
    }

    public void SetLayoutVertical()
    {

    }

    public Action<object, RectTransform> onSetView;

    ScrollRect _scrollRect;
    ScrollRect ScrollRect
    {
        get
        {
            if(_scrollRect == null)
            {
                _scrollRect = GetComponent<ScrollRect>();
            }
            return _scrollRect;
        }
    }

    public RectTransform ViewPort
    {
        get
        {
            return ScrollRect.viewport;
        }
    }

    public RectTransform Content
    {
        get
        {
            return ScrollRect.content;
        }
    }

    public enum Aligment
    {
        UpperLeft,
        UpperCenter,
        UpperRight,
    }

    public RectTransform viewPrefab;
    public RectOffset Padding;
    public float spacing;
    public Aligment childAligment;
    public bool controlChildWidth = true;
    public bool childForceExpandWidth;

    private void Awake()
    {
        this.SetupScrollRect();

        // 隐藏预制件（如果是在场景中）
        this.viewPrefab.gameObject.SetActive(false);
    }

    void SetupScrollRect()
    {
        var scrollRect = this.ScrollRect;

        // 设置只能竖直滑动
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

    }

    // 重置 Content 使其与视图重叠
    void ResetContent()
    {
        this.Content.pivot = new Vector2(0, 1);
        this.Content.anchorMin = new Vector2(0, 1);
        this.Content.anchorMax = new Vector2(1, 1);
        this.Content.anchoredPosition = new Vector2(0, 0);
        this.Content.sizeDelta = this.ViewPort.rect.size;
        this.Content.offsetMax = new Vector2(0, 0);
        this.Content.offsetMin = new Vector2(0, 0);
    }

    [ShowInInspector, ReadOnly]
    EntryManager entryManager = new EntryManager();

    /// <summary>
    /// 重置所有数据
    /// </summary>
    /// <param name="dataList">新的数据列表</param>
    /// <param name="aligment">新数据列表的显示方式</param>
    public void Reset(IList dataList, ContentAligment aligment = ContentAligment.Head)
    {
        this.ResetData(dataList);
        this.ResetContentAndView(aligment);
    }

    /// <summary>
    /// 删除某个数据
    /// </summary>
    [ShowInInspector]
    public async Task DeleteAsync(int dataIndex)
    {
        var view = this.entryManager.RemoveAt(dataIndex);
        if(view != null)
        {
            this.Recycle(view);
        }
        ForceCreateViewAtTail();
        await this.TweenViewToAlignAtTop();
    }

    public int ViewToDataIndex(RectTransform view)
    {
        return this.entryManager.ViewToDataIndex(view);
    }

    bool stopAutoSnap;
    /// <summary>
    /// 移动所有视图，以上对齐
    /// </summary>
    async Task TweenViewToAlignAtTop()
    {
        stopAutoSnap = true;
        try
        {
            var startIndex = this.entryManager.FirstViewDataIndex;
            var endIndex = this.entryManager.LastViewDataIndex;
            var hotY = this.DecideStartYInTopAlign;
            DG.Tweening.Core.TweenerCore<Vector2, Vector2, DG.Tweening.Plugins.Options.VectorOptions> firstTween = null;
            for (int index = startIndex; index <= endIndex; index++)
            {
                var view = this.entryManager.GetView(index);
                if (view == null)
                {
                    // 这个条目没有视图，不管他
                    continue;
                }
                var tween = view.DOAnchorPosY(hotY, 0.2f);
                if (firstTween == null)
                {
                    firstTween = tween;
                }
                var viewHight = view.rect.height;
                hotY -= viewHight;
                hotY -= this.spacing;
            }
            if (firstTween != null)
            {
                await firstTween.AsyncWaitForCompletion();
            }
        }
        finally
        {
            stopAutoSnap = false;
        }

    }

    float DecideStartYInTopAlign
    {
        get
        {
            var firstView = this.entryManager.FirstView;
            if(firstView != null)
            {
                var firstViewAnchordPosY = firstView.anchoredPosition.y;
                var b = IsHigherThanViewPortTop(firstViewAnchordPosY);
                if (b)
                {
                    return firstViewAnchordPosY;
                }
            }
            var y = this.ViewRectTopYInContent;
            y -= this.spacing;
            return y;
        }
    }

    void ResetData(IList dataList)
    {
        var removedViewList = this.entryManager.Reset(dataList);
        foreach (var view in removedViewList)
        {
            Recycle(view);
        }
    }

    public float MesureViewHeight(int dataIndex)
    {
        var data = this.entryManager.GetData(dataIndex);
        var view = CreateUnmanagedView(0, data);
        var height = view.rect.height;
        Recycle(view);
        return height;
    }

    public enum ContentAligment
    {
        Head,
        Tail,
    }

    float ViewRectHight => this.ViewPort.rect.height;

    /// <summary>
    /// 重置显示
    /// </summary>
    /// <param name="aligment">显示方式</param>
    void ResetContentAndView(ContentAligment aligment = ContentAligment.Head)
    {
        this.isContentUpperLocked = false;
        this.isContentLowerLocked = false;
        this.ResetContent();
        this.RemoveAllView();
        if (this.entryManager.Count == 0)
        {
            return;
        }
        if (aligment == ContentAligment.Head)
        {
            this.CreateView(0, -this.Padding.top, ContentAligment.Head);
        }
        else
        {
            var dataIndex = this.entryManager.LastDataIndex;
            var anchordPostionY = 0 - this.ViewRectHight + this.Padding.bottom;
            this.CreateView(dataIndex, anchordPostionY, ContentAligment.Tail);
        }
        this.SnapView(true);
    }

    public void AppendData(IList dataList, int startIndex, int length, bool useAnimation = true)
    {
        var count = this.entryManager.Count;
        if (count == 0)
        {
            this.Reset(dataList, ContentAligment.Tail);
            return;
        }
        // 动画只在追加一个数据的情况下，有可能播放
        if(length != 1)
        {
            useAnimation = false;
        }
        var isBefreLastViewVisible = this.entryManager.HasView(this.entryManager.Count - 1);

        for (int i = 0; i < length; i++)
        {
            var index = startIndex + i;
            var data = dataList[index];
            this.entryManager.Add(data);
        }
        this.isContentLowerLocked = false;
        this.SnapView(true);

        if(useAnimation)
        {
            var isPostLastViewVisible = this.entryManager.HasView(this.entryManager.Count - 1);
            if (isBefreLastViewVisible && !isPostLastViewVisible)
            {
                var lastViewHigeht = this.MesureViewHeight(this.entryManager.Count - 1);
                var spacing = this.spacing;
                var scrollHeight = lastViewHigeht + spacing;
                this.ScrollSnap(scrollHeight);
            }
        }
    }
    void ScrollSnap(float height)
    {
        var content = this.Content;
        var anchordPositonY = content.anchoredPosition.y;
        var newPosY = anchordPositonY + height;

        content.DOAnchorPosY(newPosY, 0.2f);
    }

    /// <summary>
    /// 为一个没有视图的数据创建视图
    /// </summary>
    /// <param name="dataIndex">已管理的数据的索引</param>
    /// <param name="anchordPostionY"> Content 的右上角是0，往下减少</param>
    /// <param name="aligment">指定控件上边界还是下边界对齐到指定位置</param>
    RectTransform CreateView(int dataIndex, float anchordPostionY, ContentAligment aligment)
    {
        var data = this.entryManager.GetData(dataIndex);
        var view = CreateUnmanagedView(anchordPostionY, data);
        this.entryManager.SetView(dataIndex, view);
        if(aligment == ContentAligment.Tail)
        {
            var height = view.rect.height;
            var p = view.anchoredPosition;
            p.y += height;
            view.anchoredPosition = p;
        }
        return view;
    }

    public void ForeachView(Action<RectTransform> handler)
    {
        this.entryManager.ForeachView(handler);
    }


    void AllocateUppderContent(float size)
    {
        var rect = this.GetContentRectInWorld();
        var worldHight = SpaceUtil.TransformHeight(this.Content.transform, size);
        rect.yMax += worldHight;
        var visibleViewWorldPostionList = this.RecordVisibleViewWorldPostion();
        this.SetContentUsingWorldRect(rect);
        this.SetVisibleViewWorldPostion(visibleViewWorldPostionList);
    }

    void AllocateLowerContent(float size)
    {
        var oldSizeY = this.Content.sizeDelta.y;
        var newSizeY = oldSizeY + size;
        this.Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSizeY);
    }

    void SetContentTop(float yMax)
    {
        var rect = this.GetContentRectInWorld();
        rect.yMax = yMax;
        var visibleViewWorldPostionList = this.RecordVisibleViewWorldPostion();
        this.SetContentUsingWorldRect(rect);
        this.SetVisibleViewWorldPostion(visibleViewWorldPostionList);
    }

    void SetContentBottom(float yMinAtWorld)
    {
        var worldRect = this.GetContentRectInWorld();
        worldRect.yMin = yMinAtWorld;
        this.SetContentUsingWorldRect(worldRect);
    }

    bool IsContentContainsAnchordPostionY(float anchordPostionY)
    {
        if(anchordPostionY > 0)
        {
            return false; 
        }
        var contentHeight = this.Content.rect.height;
        if(anchordPostionY < -contentHeight)
        {
            return false;
        }
        return true;
    }

    Rect GetContentRectInWorld()
    {
        var rect = this.Content.rect;
        var min = rect.min;
        var max = rect.max;
        var worldMin = this.Content.TransformPoint(min);
        var worldMax = this.Content.TransformPoint(max);
        return new Rect(worldMin.x, worldMin.y, worldMax.x - worldMin.x, worldMax.y - worldMin.y);
    }

    List<Vector3> _tempWorldPostionList = new List<Vector3>();
    List<Vector3> RecordVisibleViewWorldPostion()
    {
        var startIndex = this.entryManager.FirstViewDataIndex;
        var endIndex = this.entryManager.LastViewDataIndex;
        _tempWorldPostionList.Clear();
        for (int index = startIndex; index <= endIndex; index++)
        {
            var view = this.entryManager.GetView(index);
            if(view == null)
            {
                continue;
            }
            var worldPosition = view.position;
            _tempWorldPostionList.Add(worldPosition);
        }
        return _tempWorldPostionList;
    }

    void SetVisibleViewWorldPostion(List<Vector3> worldPostionList)
    {
        if(worldPostionList.Count != this.entryManager.ViewCount)
        {
            throw new Exception("[VerticalVirtualLayout] view count unmatched");
        }
        var index = 0;
        this.entryManager.ForeachView(view =>
        {
            var viewPostion = worldPostionList[index];
            view.position = viewPostion;
            index++;
        });
    }

    readonly float SingleContentAllocateSize = 2000f;
    bool isContentUpperLocked;
    bool isContentLowerLocked;
    /// <summary>
    /// 保持所有可见视图的世界坐标不动
    /// 调整 Content 的尺寸，使其包含所有可见视图
    /// </summary>
    void SnapContent()
    {
        if(!isContentUpperLocked)
        {
            var topView = this.entryManager.FirstView;
            var isFirst = this.entryManager.FirstViewDataIndex == 0;
            if (isFirst)
            {
                var paddingTopAtWorld = SpaceUtil.TransformHeight(this.Content.transform, this.Padding.top);
                var topAtWorld = topView.position.y + paddingTopAtWorld;
                this.SetContentTop(topAtWorld);
                isContentUpperLocked = true;
            }
            else
            {
                // 确保可以滚动到上一个放置点的右下角
                var preViewBottomPos = this.GetPreViewBottomAnchordPostionY(topView);
                var contains = this.IsContentContainsAnchordPostionY(preViewBottomPos);
                if (!contains)
                {
                    this.AllocateUppderContent(SingleContentAllocateSize);
                }
            }

        }

        if(!isContentLowerLocked)
        {
            var bottomView = this.entryManager.LastView;
            var count = this.entryManager.Count;
            var isLast = this.entryManager.LastViewDataIndex == count - 1;  
            if (isLast)
            {
                var viewHeight = bottomView.rect.height;
                var viewHeightAtWorld = SpaceUtil.TransformHeight(bottomView.transform, viewHeight);
                var paddingAtWorld = SpaceUtil.TransformHeight(this.Content.transform, this.Padding.bottom);
                var bottomAtWorld = bottomView.position.y - viewHeightAtWorld - paddingAtWorld;
                this.SetContentBottom(bottomAtWorld);
                isContentLowerLocked = true;
            }
            else
            {
                // 确保可以滚动到下一个放置点
                var nextAnchordPos = this.GetNextViewTopAnchordPostionY(bottomView);
                var contains = this.IsContentContainsAnchordPostionY(nextAnchordPos);
                if (!contains)
                {
                    this.AllocateLowerContent(SingleContentAllocateSize);
                }
            }
        }
    }

    float GetPreViewBottomAnchordPostionY(RectTransform thisView)
    {
        var anchordPosition = thisView.anchoredPosition;
        var newPosY = anchordPosition.y + this.spacing; // +1 ?
        return newPosY;
    }

    float GetNextViewTopAnchordPostionY(RectTransform thisView)
    {
        var anchordPosition = thisView.anchoredPosition;
        var viewHeight = thisView.rect.height;
        var newPosY = anchordPosition.y - viewHeight - this.spacing; // -1 ?
        return newPosY;
    }

  


    void SetContentUsingWorldRect(Rect worldRect)
    {
        var leftTop = new Vector2(worldRect.xMin, worldRect.yMax);
        this.Content.position = leftTop;


        var localRect = SpaceUtil.InverseTransformRect(this.Content.transform, worldRect);
        var localHeight = localRect.height;
        
        this.Content.SetSizeDeltaY(localHeight);
    }

    void RemoveVisibleView(int removeDataIndex)
    {
       
        if(this.entryManager.ViewCount == 1)
        {
            return;
        }
        var view = this.entryManager.RemoveView(removeDataIndex);
        Recycle(view);
    }

    void RemoveAllView()
    {
        var viewList = this.entryManager.RemoveAllView();
        foreach(var view in viewList)
        {
            this.Recycle(view);
        }
    }

    /// <summary>
    /// 根据显示区域，在首尾创建或移除视图
    /// </summary>
    /// <param name="isForceSnapContent"></param>
    void SnapView(bool isForceSnapContent = false)
    {
        var changed = false;
        var isTailAnyCreated = this.CreateViewAtTailIfNeed();
        if (!isTailAnyCreated)
        {
            var b = this.RemoveViewAtTailIfNeed();
            if (b)
            {
                changed = true;
            }
        }
        else
        {
            changed = true;
        }

        var isHeadAnyCreated = this.CreateViewAtHeadIfNeed();
        if (!isHeadAnyCreated)
        {
            var b = this.RemoveViewAtHeadIfNeed();
            if (b)
            {
                changed = true;
            }
        }
        else
        {
            changed = true;
        }

        if (changed || isForceSnapContent)
        {
            this.SnapContent();
        }
    }

    void Update()
    {
        if(this.stopAutoSnap)
        {
            return;
        }
        this.SnapView();
    }

    bool RemoveViewAtHeadIfNeed()
    {
        if(!this.entryManager.HasAnyView)
        {
            return false;
        }
        var isAnyRemoved = false;
        var startDataIndex = this.entryManager.FirstViewDataIndex;
        var lastDataIndex = this.entryManager.LastViewDataIndex;
        for (int index = startDataIndex; index <= lastDataIndex; index++)
        {
            var view = this.entryManager.GetView(index);
            if(view == null)
            {
                continue;
            }
            var anchordPosition = view.anchoredPosition;
            anchordPosition.y -= view.rect.height;
            var isInvisible = this.IsHigherThanViewPortTop(anchordPosition.y);
            if (isInvisible)
            {
                RemoveVisibleView(index);
                isAnyRemoved = true;
            }
            else
            {
                break;
            }
        }
        return isAnyRemoved;
    }

    bool CreateViewAtHeadIfNeed()
    {
        if(!this.entryManager.HasAnyView)
        {
            return false;
        }
        var isAnyCreated = false;
        var firstDataIndex = this.entryManager.FirstViewDataIndex;
        var preDataIndex = firstDataIndex - 1;
        var hasPre = this.entryManager.IsDataIndexValid(preDataIndex);
        if (!hasPre)
        {
            return isAnyCreated;
        }
        var view = this.entryManager.FirstView;
        var pos = view.anchoredPosition;
        pos.y += this.spacing;


        for(int index = preDataIndex; index >= 0; index--)
        {
            var isInvisible = IsHigherThanViewPortTop(pos.y);
            if(isInvisible)
            {
                break;
            }

            //var data = this.entryManager.GetData(index);
            //var newView = InternalCreateView(0, data);

            //var height = newView.rect.height;
            //pos.y += height;
            //newView.anchoredPosition = pos;
            //this.entryManager.SetView(index, newView);

            var newView = this.CreateView(index, pos.y, ContentAligment.Tail);
            var height = newView.rect.height;
            pos.y += height;

            isAnyCreated = true;
            pos.y += this.spacing;
        }
        return isAnyCreated;
    }

    bool RemoveViewAtTailIfNeed()
    {
        if(!this.entryManager.HasAnyView)
        {
            return false;
        }
        var isAnyRemoved = false;
        var lastDataIndex = this.entryManager.LastViewDataIndex;
        var startDataIndex = this.entryManager.FirstViewDataIndex;
        for(int index = lastDataIndex; index >= startDataIndex; index--)
        {
            var view = this.entryManager.GetView(index);
            if(view == null)
            {
                continue;
            }
            var anchordPosition = view.anchoredPosition;
            var isInvisible = this.IsLowerThanViewPortBottom(anchordPosition.y);
            if (isInvisible)
            {
                RemoveVisibleView(index);
                isAnyRemoved = true;
            }
            else
            {
                break;
            }
        }
        return isAnyRemoved;
    }

    /// <summary>
    /// 无论是否可见，只要还有数据，强行在末尾创建下一个视图
    /// </summary>
    /// <returns></returns>
    bool ForceCreateViewAtTail()
    {
        if (!this.entryManager.HasAnyView)
        {
            return false;
        }
        var lastDataIndex = this.entryManager.LastViewDataIndex;
        var nextDataIndex = lastDataIndex + 1;
        var hasNext = this.entryManager.IsDataIndexValid(nextDataIndex);
        if (!hasNext)
        {
            return false;
        }
        var view = this.entryManager.LastView;
        var pos = view.anchoredPosition;
        pos.y -= view.rect.height;
        pos.y -= this.spacing;
        this.CreateView(nextDataIndex, pos.y, ContentAligment.Head);
        return true;
    }

    bool CreateViewAtTailIfNeed()
    {
        if(!this.entryManager.HasAnyView)
        {
            return false;
        }
        var lastDataIndex = this.entryManager.LastViewDataIndex;
        if(lastDataIndex == -1)
        {
            return false;
        }
        var nextDataIndex = lastDataIndex + 1;
        var hasNext = this.entryManager.IsDataIndexValid(nextDataIndex);
        if(!hasNext)
        {
            return false;
        }
        var view = this.entryManager.LastView;
        var pos = view.anchoredPosition;
        pos.y -= view.rect.height;
        pos.y -= this.spacing;
        var isInvisible = IsLowerThanViewPortBottom(pos.y);
        if(!isInvisible)
        {
            var startDataIndex = lastDataIndex + 1;
            var startPosition = pos;
            var (createStartIndex, createEndIndex, createdViewList) = GenerateViewStartFrom(startDataIndex, startPosition.y);
            this.entryManager.SetViewRange(createStartIndex, createdViewList);
            return true;
        }
        return false;
    }

    List<RectTransform> tempCreatedViewList = new List<RectTransform>();
    (int startIndex, int endIndex, List<RectTransform> createdViewList) GenerateViewStartFrom(int startDataIndex, float startAnchordPostionY)
    {
        var retStartIndex = -1;
        var retEndIndex = -1;
        tempCreatedViewList.Clear();
        var anchordPostionY = startAnchordPostionY;
        bool first = true;
        var count = this.entryManager.Count;
        for (int dataIndex = startDataIndex; dataIndex < count; dataIndex++)
        {
            if(first)
            {
                first = false;
                retStartIndex = dataIndex;
                retEndIndex = dataIndex;
            }
            else
            {
                anchordPostionY -= this.spacing;
                retEndIndex = dataIndex;
            }
            //var view = this.CreateViewForDataIndex(dataIndex, anchordPostionY);
            var data = this.entryManager.GetData(dataIndex);
            var view = CreateUnmanagedView(anchordPostionY, data);

            tempCreatedViewList.Add(view);
            var viewHeight = view.rect.height;
            anchordPostionY -= viewHeight;
            var isInvisible = IsLowerThanViewPortBottom(anchordPostionY);
            if(isInvisible)
            {
                break;
            }
        }
        return (retStartIndex, retEndIndex, tempCreatedViewList);
    }


    bool IsLowerThanViewPortBottom(float anchordPostiionY)
    {
        var rect = this.Content.rect;
        var point = rect.center;
        point.y = anchordPostiionY;
        var worldPoint = this.Content.TransformPoint(point);
        var pointInView = this.ViewPort.InverseTransformPoint(worldPoint);
        var rectInView = this.ViewPort.rect;
        if (pointInView.y < rectInView.y)
        {
            return true;
        }
        return false;
    }

    bool IsHigherThanViewPortTop(float anchordPostiionY)
    {
        var rect = this.Content.rect;
        var point = rect.center;
        point.y = anchordPostiionY;
        var worldPoint = this.Content.TransformPoint(point);
        var pointInView = this.ViewPort.InverseTransformPoint(worldPoint);
        var rectInView = this.ViewPort.rect;
        if (pointInView.y > rectInView.y + rectInView.height)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// ViewRect 的上边界在 Content 中的 Y 坐标
    /// </summary>
    float ViewRectTopYInContent
    {
        get
        {
            var rectInView = this.ViewPort.rect;
            var topInView = rectInView.position.y + rectInView.height;
            var topInWorld = this.ViewPort.TransformPoint(0, topInView, 0).y;
            var topInContent = this.Content.InverseTransformPoint(0, topInWorld, 0).y;
            return topInContent;
        }
    }

    bool IsAnchordPositionYInViewPort(float anchordPostiionY)
    {
        var rect = this.Content.rect;
        var point = rect.center;
        point.y = anchordPostiionY;
        var worldPoint = this.Content.TransformPoint(point);
        var pointInView = this.ViewPort.InverseTransformPoint(worldPoint);
        var rectInView = this.ViewPort.rect;
        var inView = rectInView.Contains(pointInView);
        return inView;
    }


    List<RectTransform> pool = new List<RectTransform>();
    void Recycle(RectTransform view)
    {
        pool.Add(view);
        AnimatorUtil.ResetToDefaultState(view.gameObject);

        view.gameObject.SetActive(false);
    }

    RectTransform Reuse()
    {
        if(pool.Count == 0)
        {
            var view = GameObject.Instantiate(this.viewPrefab);
            return view;
        }
        else
        {
            var index = pool.Count - 1;
            var view = pool[index];
            pool.RemoveAt(index);
            return view;
        }
    }




    /// <summary>
    /// 在 Content 指定位置创建视图
    /// </summary>
    /// <param name="anchordPostionY">Content 的上边位置总是为 0</param>
    /// <returns></returns>
    public RectTransform CreateUnmanagedView(float anchordPostionY, object data)
    {
        var anchordPostion = GetAndhrodPostion(anchordPostionY);
        var view = this.Reuse();
        view.SetParent(this.Content, false);
        view.pivot = this.GetPivot();
        var anchor = this.GetAnchor();
        view.anchorMin = anchor;
        view.anchorMax = anchor; 
        view.anchoredPosition = anchordPostion;



        view.gameObject.SetActive(true);
        this.onSetView?.Invoke(data, view);


        this.ControlSize(view);
        return view;
    }

    DrivenRectTransformTracker tracker;
    public void ControlSize(RectTransform view)
    {
        // 如果有其他控制器，先取消
        var contentSizeFitter = view.GetComponent<ContentSizeFitter>();
        if (contentSizeFitter != null)
        {
            contentSizeFitter.enabled = false;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(view);
        ControlWidthIfNeed(view);
        LayoutRebuilder.ForceRebuildLayoutImmediate(view);
        ControlHeight(view);

    }

    public void ControlHeight(RectTransform view)
    {
        // 申明控制宽度
        tracker.Add(this, view, DrivenTransformProperties.SizeDeltaY);

        // 设置高度
        var preferredHeight = LayoutUtility.GetPreferredSize(view, 1);
        view.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);
    }

    public void ControlWidthIfNeed(RectTransform view)
    {
        // 配置指定不修改视图宽度
        if(!this.controlChildWidth)
        {
            return;
        }

        // 申明控制宽度
        tracker.Add(this, view, DrivenTransformProperties.SizeDeltaX);

        // 设置宽度
        var width = GetAllocateWidth(view);
        view.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }

    public float GetAllocateWidth(RectTransform view)
    {
        var minWidth = LayoutUtility.GetMinSize(view, 0);
        var preferredWidth = LayoutUtility.GetPreferredSize(view, 0);
        var flexible = LayoutUtility.GetFlexibleSize(view, 0);

        var allocableWidth = this.ContentAllocableWidth;
        var width = 0f;
        if (allocableWidth >= preferredWidth)
        {
            if (flexible > 0 || this.childForceExpandWidth)
            {
                width = allocableWidth;
            }
            else
            {
                width = preferredWidth;
            }
        }
        else
        {
            width = allocableWidth;
        }

        if (width < minWidth)
        {
            width = minWidth;
        }
        return width;
    }

    public float ContentAllocableWidth
    {
        get
        {
            var rectWidth = this.Content.rect.width;
            var rest = rectWidth - this.Padding.left - this.Padding.right;
            return rest;
        }
    }

    Vector2 GetAndhrodPostion(float anchordPostionY)
    {
        var x = this.GetAnchordPositionX();
        var ret = new Vector2(x, anchordPostionY);
        return ret;
    }

    Vector2 GetAnchor()
    {
        if (this.childAligment == Aligment.UpperLeft)
        {
            return new Vector2(0, 1);
        }
        else if (this.childAligment == Aligment.UpperRight)
        {
            return new Vector2(1, 1);
        }
        else if (this.childAligment == Aligment.UpperCenter)
        {
            return new Vector2(0.5f, 1);
        }
        throw new Exception("[VerticalVritualLayout] not support aligment: " + this.childAligment);
    }

    Vector2 GetPivot()
    {
        if (this.childAligment == Aligment.UpperLeft)
        {
            return new Vector2(0, 1);
        }
        else if (this.childAligment == Aligment.UpperRight)
        {
            return new Vector2(1, 1);
        }
        else if (this.childAligment == Aligment.UpperCenter)
        {
            return new Vector2(0.5f, 1);
        }
        throw new Exception("[VerticalVritualLayout] not support aligment: " + this.childAligment);
    }

    // 任何视图的锚点化位置的 X，仅由对齐方式决定
    float GetAnchordPositionX()
    {
        if (this.childAligment == Aligment.UpperLeft)
        {
            return this.Padding.left;
        }
        else if (this.childAligment == Aligment.UpperRight)
        {
            return -this.Padding.right;
        }
        else if (this.childAligment == Aligment.UpperCenter)
        {
            return 0;
        }
        throw new Exception("[VerticalVritualLayout] not support aligment: " + this.childAligment);
    }

}
