using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = System.Object;

public partial class LauncherGameZone : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    // 宽度（包含间距）
    private const int ItemDistance = 184;
    
    public LauncherGameItem ItemPrefab;
    public Image ItemBg;
    public Transform LinePrefab;
    public Transform ItemsNode;
    public AudioSource AudioLine;

    public Action<List<int>, List<string>> OnEliminate { set; private get; }

    private int _baseOffsetX;
    private int _baseOffsetY;
    private List<LauncherGameItem> _stageItems;
    private Queue<LauncherGameItem> _itemsPool;
    private Queue<Transform> _linePool;
    private Dictionary<int, LauncherGameItem> _itemMap;
    private Dictionary<int, Transform> _lineMap;
    private Dictionary<int, Image> _itemBgMap;
    private int _currentSpot;
    private bool _locked;
    private bool _gameOver;
    private int _droppingNum;

    public void ResetMap()
    {
        LauncherGameManager.MakeRandomMap();

        foreach (var item in _stageItems)
        {
            _RecycleItem(item, true);
        }
        _stageItems.Clear();
        
        var items = LauncherGameManager.Items;
        for (var i = 0; i < items.Length; i++)
        {
            var itemView = _TakeItem();
            itemView.Flag = items[i];
            var (h, v) = LauncherGameManager.GetHV(i);
            itemView.Pos = _GetPos(h, v);
            _itemMap[i] = itemView;
        }

        _gameOver = false;
    }

    /// <summary>
    /// 锁住不给操作了
    /// </summary>
    public void GameOver()
    {
        _ClearRound();
        _gameOver = true;
    }

    public Image GetItemBg(int spot)
    {
        return _itemBgMap[spot];
    }
    
    private void Awake()
    {
        var numH = LauncherGameManager.NumH;
        var numV = LauncherGameManager.NumV;
        
        // 数值初始化
        _stageItems = new List<LauncherGameItem>(numH * numV);
        _itemsPool = new Queue<LauncherGameItem>();
        _linePool = new Queue<Transform>();
        _itemMap = new Dictionary<int, LauncherGameItem>();
        _lineMap = new Dictionary<int, Transform>();
        _itemBgMap = new Dictionary<int, Image>();
        _currentSpot = -1;
        
        // 背景初始化
        _baseOffsetX = -ItemDistance * (numH - 1) / 2;
        _baseOffsetY = -ItemDistance * (numV - 1) / 2;
        for (var v = 0; v < LauncherGameManager.NumV; ++v)
        {
            for (var h = 0; h < LauncherGameManager.NumH; ++h)
            {
                var spot = LauncherGameManager.GetSpot(h, v);
                var image = _itemBgMap[spot] = Instantiate(ItemBg, ItemBg.transform.parent);
                image.rectTransform.anchoredPosition = _GetPos(h, v);
            }
        }
        
        // 如果是在场景上的， 那就还得把它本身隐藏掉
        var bgScene = ItemBg.gameObject.scene;
        if (!string.IsNullOrEmpty(bgScene.name))
        {
            ItemBg.SetActive(false);
        }
        
        var lineScene = LinePrefab.gameObject.scene;
        if (!string.IsNullOrEmpty(lineScene.name))
        {
            LinePrefab.SetActive(false);
        }
    }

    private Vector2 _GetPos(int h, int v)
    {
        return new Vector2(_baseOffsetX + h * ItemDistance, _baseOffsetY + v * ItemDistance);
    }

    private void _HandleEliminate()
    {
        // 做消除逻辑
        if (LauncherGameManager.CanEliminate())
        {
            if (_locked)
            {
                // ToastManager.ShowLocalize("游戏盘尚未稳定，请稍候");
                return;
            }
            
            var links = LauncherGameManager.Links;
            var linkItems = links.ConvertAll(spot => LauncherGameManager.Items[spot]);
            // 开始执行消失逻辑
            foreach (var spot in links)
            {
                var itemView = _itemMap[spot];
                var itemTf = itemView.transform;
                itemTf.DOScale(0, .2f).OnComplete(() =>
                {
                    _RecycleItem(itemView);
                });
                // LauncherGameEffect.PlayEffect("fx_ui_Sgame_clear", itemTf.position);
            }
            LauncherGameManager.Eliminate();
            
            // 开始处理往下掉
            var drops = LauncherGameManager.Drops;
            foreach (var spot in drops.Keys)
            {
                var dropNum = drops[spot];
                var itemView = _itemMap[spot];
                // 掉到新位置后的h,v更新
                var (h, v) = LauncherGameManager.GetHV(spot);
                v -= dropNum;
                var newSpot = LauncherGameManager.GetSpot(h, v);
                // 更新字典对应的key
                _itemMap[newSpot] = itemView;
                // 展示掉落效果
                _DoDrop(itemView, h, v, dropNum);
            }
            _droppingNum += drops.Count;
            
            // 开始做填充往下掉
            var eliminates = LauncherGameManager.Eliminates;
            var items = LauncherGameManager.Items;
            foreach (var h in eliminates.Keys)
            {
                var eliminateNum = eliminates[h];
                for (var i = 0; i < eliminateNum; ++i)
                {
                    var v = LauncherGameManager.NumV - i - 1;
                    var spot = LauncherGameManager.GetSpot(h, v);
                    var itemView = _TakeItem();
                    itemView.Flag = items[spot];
                    // 先放高处往下掉
                    itemView.Pos = _GetPos(h, v + eliminateNum);
                    _itemMap[spot] = itemView;
                    _DoDrop(itemView, h, v, eliminateNum);
                }
                _droppingNum += eliminateNum;
            }
            // 锁住盘子
            _locked = true;
            // 执行消除回调
            OnEliminate?.Invoke(links, linkItems);
        }
        
        // 清掉回合信息
        _ClearRound();
        // 清除数据
        _currentSpot = -1;
    }

    private void _DoDrop(LauncherGameItem itemView, int h, int v, int dropHeight)
    {
        itemView.Rt.DOAnchorPos(_GetPos(h, v), .4f + .1f * dropHeight)
            .SetEase(Ease.OutBounce)
            .OnComplete(() =>
            {
                if (--_droppingNum <= 0)
                {
                    _locked = false;
                }
            });
        
    }

    private void _ClearRound()
    {
        var links = LauncherGameManager.Links;

        foreach (var spot in links)
        {
            _itemMap[spot].Selected = false;
            // 线也要清除
            if (_lineMap.TryGetValue(spot, out var line))
            {
                _RecycleLine(line);
            }
        }
        _lineMap.Clear();
        LauncherGameManager.ClearRound();
    }

    private LauncherGameItem _TakeItem()
    {
        var item = _itemsPool.Count > 0 ? _itemsPool.Dequeue() : Instantiate(ItemPrefab, ItemsNode);
        item.Selected = false;
        item.transform.localScale = Vector3.one;
        item.SetActive(true);
        _stageItems.Add(item);
        return item;
    }

    private void _RecycleItem(LauncherGameItem item, bool keepItem = false)
    {
        item.SetActive(false);
        _itemsPool.Enqueue(item);
        if (!keepItem)
        {
            _stageItems.Remove(item);
        }
    }
    
    private Transform _TakeLine()
    {
        var item = _linePool.Count > 0 ? _linePool.Dequeue() : Instantiate(LinePrefab, LinePrefab.parent);
        item.SetActive(true);
        return item;
    }

    private void _RecycleLine(Transform line)
    {
        line.SetActive(false);
        _linePool.Enqueue(line);
    }

    #region 拖拽逻辑部分
    
    private RectTransform _rectTransform; //控件所在画布

    private RectTransform CurrentRectTransform
    {
        get
        {
            if (null == _rectTransform)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            return _rectTransform;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        _HandleEventData(eventData, false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _HandleEventData(eventData, true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_gameOver) return;
        
        _HandleEliminate();
    }

    private void _HandleEventData(PointerEventData eventData, bool down)
    {
        if (_gameOver) return;
        
        Camera cam = eventData.pressEventCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(CurrentRectTransform, eventData.position, cam, out var downPos);

        var h = (int)(downPos.x - _baseOffsetX + (ItemDistance >> 1)) / ItemDistance;
        var v = (int)(downPos.y - _baseOffsetY + (ItemDistance >> 1)) / ItemDistance;
        var spot = LauncherGameManager.GetSpot(h, v);

        if (spot == -1 || spot == _currentSpot) return;
        _currentSpot = spot;
        // 开始做连线逻辑
        var links = LauncherGameManager.Links;
        var redoSpot = LauncherGameManager.Redo(spot);
        if (redoSpot != -1)
        {
            _itemMap[redoSpot].Selected = false;
            _RecycleLine(_lineMap[redoSpot]);
            _lineMap.Remove(redoSpot);
        } 
        else if (LauncherGameManager.LinkTo(spot))
        {
            var itemView = _itemMap[spot];
            itemView.Selected = true;
            LauncherGameEffect.PlayEffect("fx_ui_Sgame_line", itemView.transform.position);
            
            // 如果有两个以上了， 就开始连线
            if (links.Count > 1)
            {
                var line = _lineMap[spot] = _TakeLine();
                var (lineH, lineV) = LauncherGameManager.GetHV(spot);
                var lineRt = line.gameObject.GetComponent<RectTransform>();
                lineRt.anchoredPosition = _GetPos(lineH, lineV);
                var prevSpot = links[links.Count - 2];
                lineRt.rotation = Quaternion.Euler(new Vector3(
                    0, 0, LauncherGameManager.GetRotation(spot, prevSpot)));
                
                // 播放音乐
                AudioLine.Play();
            }
        }
    }

    #endregion
}