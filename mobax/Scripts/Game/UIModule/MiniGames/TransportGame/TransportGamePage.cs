using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public partial class TransportGamePage : Page, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public TransportItem ItemPrefab;
    public TransportScoreEffectItem ScoreEffectItemPrefab;
    private int ItemGap = 200;

    private BehaviourPool<TransportItem> _itemPool;
    private BehaviourPool<TransportScoreEffectItem> _effectPool;
    private TransportItem _topItem;
    private TransportItem _candidateItem;
    private bool _gaming;
    private bool _accelerate;
    private RectTransform _canvasRt; //控件所在画布
    private Vector2 _startPos; // 鼠标初始位置
    private bool _dragging;
    private MutexComponent _highlightingArrow;
    private int _stage;
    private GameSlideRow _cfg;
    private int _missionId;
    private DateTime _recoverTime;
    private DateTime _stageEndTime;
    private DateTime _accelerateEndTime;
    private int _stageScore;
    private int _totalScore;
    private List<int> _scores;
    private int _level;
    private int _rightNum;
    private int _wrongNum;
    private int _bombNum;

    private RectTransform CanvasRt
    {
        get
        {
            if (null == _canvasRt)
            {
                _canvasRt = GetComponentInParent<Canvas>().transform as RectTransform;
            }

            return _canvasRt;
        }
    }

    private void Update()
    {
        if (!_gaming) return;
        if (_accelerate)
        {
            if (Clock.Now > _accelerateEndTime)
            {
                _accelerate = false;
                TransportItem.Speed = _cfg.Initspd;
            }
        }
        if (TransportItem.Pause)
        {
            if (Clock.Now < _recoverTime)
            {
                return;
            }
            TransportItem.Pause = false;
            Node_broken.SetActive(false);
        }
        if (Clock.Now >= _stageEndTime)
        {
            _gaming = false;
            TransportItem.Pause = true;
            _RecycleAll(_CheckStageMoveOn);
            return;
        }
        
        if (null == _topItem || _topItem.Passed >= ItemGap)
        {
            var offLength = null == _topItem ? 0 : _topItem.Passed - ItemGap;
            var newItem = _FetchItem();
            if (null != _topItem)
            {
                _topItem.Next = newItem;
            }
            _topItem = newItem;
            _topItem.SetStart(offLength);
        }

        if (null == _candidateItem)
        {
            _SetCandidate(_topItem);
        }
    }

    private void Awake()
    {
        TransportItem.AimLeft = Dest_left.rectTransform.anchoredPosition;
        TransportItem.AimRight = Dest_right.rectTransform.anchoredPosition;

        _itemPool = new BehaviourPool<TransportItem>();
        _itemPool.SetPrefab(ItemPrefab);
        _itemPool.SetParent(Node_props);

        _effectPool = new BehaviourPool<TransportScoreEffectItem>();
        _effectPool.SetPrefab(ScoreEffectItemPrefab);
    }

    private void OnEnable()
    {
        KeyRegister.Stuff.Register(KeyCode.LeftArrow, () =>
        {
            _InvokeMove(TransportDirection.Left);
            _HighlightArrow(Arrow_left);
        });
        KeyRegister.Stuff.Register(KeyCode.RightArrow, () =>
        {
            _InvokeMove(TransportDirection.Right);
            _HighlightArrow(Arrow_right);
        });
    }

    private void OnDisable()
    {
        KeyRegister.Stuff.Unregister(KeyCode.LeftArrow);
        KeyRegister.Stuff.Unregister(KeyCode.RightArrow);
    }

    public override void OnButton(string msg)
    {
        switch (msg)
        {
            case "back":
                _OnBack();
                break;
        }
    }
    
    public override void OnNavigatedTo(PageNavigateInfo navigateInfo)
    {
        if (navigateInfo.param is int missionId)
        {
            _stage = 1;
            _totalScore = 0;
            _rightNum = 0;
            _wrongNum = 0;
            _bombNum = 0;
            _missionId = missionId;
            _scores?.Clear();
            _StageGo();
        }
    }

    private async void _StageGo()
    {
        _gaming = false;
        _topItem = null;
        _candidateItem = null;
        _stageScore = 0;
        _level = -1;
        _cfg = StaticData.GameSlideTable.TryGet(_stage);
        UiUtil.SetSpriteInBackground(Dest_left, () => $"{_cfg.Left.Icon}.png");
        UiUtil.SetSpriteInBackground(Dest_right, () => $"{_cfg.Right.Icon}.png");
        
        var waitingTime = 4;
        Node_tip.SetActive(true);
        Cd_tip.SetEnd(waitingTime, @"{0:ss}", zeroString: "M4_minigame2_start".Localize());
        Txt_stage.SetLocalizer("M4_minigame2_stage".Localize($"{_stage}"));
        _RefreshScore();
        _RefreshStageProgress();
        await Task.Delay(waitingTime * 1000);
        Node_tip.SetActive(false);
        TransportItem.Pause = false;
        _gaming = true;
        
        // 计算结束时间和显示倒计时
        _stageEndTime = Clock.Now.AddSeconds(_cfg.Time);
        // 给个2秒加速的时间
        _accelerate = true;
        _accelerateEndTime = Clock.Now.AddSeconds(2);
        TransportItem.Speed = _cfg.Initspd + (_accelerate ? 800 : 0);
        Cd_stage.SetEnd(_stageEndTime);
    }

    public void OnClickStart()
    {
        // _stage = 1;
        // _StageGo();
    }
    
    //开始拖拽
    public void OnPointerDown(PointerEventData eventData)
    {
        //控件所在画布空间的初始位置
        Camera cam = eventData.pressEventCamera;
        //将屏幕空间鼠标位置eventData.position转换为鼠标在画布空间的鼠标位置
        RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasRt, eventData.position, cam,
            out var pos);
        _startPos = pos;
        _dragging = false;
    }
    
    // 拖拽过程中
    public void OnDrag(PointerEventData eventData)
    {
        Camera cam = eventData.pressEventCamera;
        //将屏幕空间鼠标位置eventData.position转换为鼠标在画布空间的鼠标位置
        RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasRt, eventData.position, cam,out var pos);
        if (_dragging)
        {
            if (_gaming)
            {
                _HighlightArrow(pos.x > _startPos.x ? Arrow_right : Arrow_left);
            }
        }
        else
        {
            if (Vector3.Distance(_startPos, pos) > 2)
            {
                _dragging = true;
            }
        }
    }
    
    // 结束拖拽
    public void OnPointerUp(PointerEventData eventData)
    {
        if (_dragging)
        {
            _dragging = false;

            if (_gaming)
            {
                _InvokeMove(Arrow_left == _highlightingArrow ? TransportDirection.Left : TransportDirection.Right);
                _HighlightArrow(null);
            }
        }
    }

    private void _HighlightArrow(MutexComponent arrow)
    {
        if (arrow != _highlightingArrow)
        {
            if (null != _highlightingArrow)
            {
                _highlightingArrow.Selected = false;
            }

            if (null != arrow)
            {
                arrow.Selected = true;
            }

            _highlightingArrow = arrow;
        }
    }

    private TransportItem _FetchItem()
    {
        var item = _itemPool.Get();
        // 随机一个图标出来
        var ran = Random.Range(0, 10000);
        string icon;
        var isBomb = false;
        if (ran <= _cfg.Left.Val)
        {
            icon = _cfg.Left.Icon;
        }
        else if (ran <= _cfg.Left.Val + _cfg.Right.Val)
        {
            icon = _cfg.Right.Icon;
        }
        else
        {
            icon = _cfg.Boom.Icon;
            isBomb = true;
        }

        item.Next = null;
        item.SetIcon(icon, isBomb);
        item.OnEnd = _OnItemEnd;
        item.OnDest = _OnItemDest;
        
        return item;
    }

    private void _InvokeMove(TransportDirection dir)
    {
        if (TransportItem.Pause) return;
        
        while (true)
        {
            if (!_candidateItem.ReachTarget)
            {
                if (!_candidateItem.Surpass)
                {
                    // 这种就是还没到位置的， 那么就遍历结束了
                    break;
                }
                _SetCandidate(_candidateItem.Next);
            }
            else
            {
                // 如果是炸弹
                if (_candidateItem.IsBomb)
                {
                    _candidateItem.Explode();
                    TransportItem.Pause = true;
                    Node_broken.SetActive(true);
                    _recoverTime = Clock.Now.AddSeconds(_cfg.Stop);
                    Cd_repair.SetEnd(_recoverTime, @"{0:ss}s");
                    ++_bombNum;
                }
                else
                {
                    _candidateItem.MoveTo(dir);
                    _SetCandidate(_candidateItem.Next);
                }
                break;
            }
        }
    }

    private void _OnItemEnd(TransportItem item)
    {
        item.Vanish(onComplete: () => _itemPool.Recycle(item));
        if (_candidateItem == item)
        {
            _SetCandidate(_candidateItem.Next);
        }
    }

    private void _OnItemDest(TransportItem item)
    {
        _OnItemEnd(item);
        // 然后还要计算分数
        switch (item.DestDir)
        {
            case TransportDirection.Left:
                _CheckScore(item.Icon, _cfg.Left.Icon, Spot_effectLeft);
                break;
            case TransportDirection.Right:
                _CheckScore(item.Icon, _cfg.Right.Icon, Spot_effectRight);
                break;
        }
    }

    private void _CheckScore(string itemIcon, string destIcon, Transform effectSpot)
    {
        int score;
        if (itemIcon == destIcon)
        {
            score = _cfg.Get;
            ++_rightNum;
        }
        else
        {
            score = -_cfg.Loss;
            ++_wrongNum;
        }

        _totalScore += score;
        _stageScore += score;
        _RefreshScore();
        
        // 加减分展示
        var scoreEffect = _effectPool.Get(effectSpot.parent);
        scoreEffect.SetPosition(effectSpot.position);
        scoreEffect.SetScore(score);
        scoreEffect.onEffectEnd =_OnScoreEffectEnd;

        // 速度调整计算
        if (score > 0)
        {
            if (_level + 1 < _cfg.Addspds.Count)
            {
                var nextSpeed = _cfg.Addspds[_level + 1];
                if (_stageScore >= nextSpeed.Score)
                {
                    ++_level;
                    TransportItem.Speed = _cfg.Initspd + nextSpeed.Add;
                }
            }
        }
        else
        {
            if (_level >= 0)
            {
                var curSpeed = _cfg.Addspds[_level];
                if (_stageScore < curSpeed.Score)
                {
                    var prevSpeedAdd = 0;
                    if (_level - 1 >= 0)
                    {
                        prevSpeedAdd = _cfg.Addspds[_level - 1].Add;
                    }
                    --_level;
                    TransportItem.Speed = _cfg.Initspd + prevSpeedAdd;
                }
            }
        }
    }

    private void _RefreshScore()
    {
        Txt_stageScore.text = $"{_stageScore}";
    }

    private void _RefreshStageProgress()
    {
        var total = StaticData.GameSlideTable.ElementList.Count;
        for (var i = 1; i < total; ++i)
        {
            var dot = Bar_progress.Find($"Dot_{i+1}");
            dot.SetActive(_stage > i);
        }
    }

    private void _SetCandidate(TransportItem item)
    {
        _candidateItem = item;
    }

    private async void _RecycleAll(Action recycleCompleted)
    {
        for (var index = 0; index < _itemPool.List.Count; index++)
        {
            var item = _itemPool.List[index];
            item.Vanish();
            await Task.Delay(50);
        }

        _itemPool.MarkClear();
        _itemPool.RecycleLeft();
        recycleCompleted?.Invoke();
    }

    private void _CheckStageMoveOn()
    {
        // 分数记录
        _scores ??= new List<int>();
        _scores.Add(_stageScore);
        // 判断下一关
        ++_stage;
        if (_stage > StaticData.GameSlideTable.ElementList.Count)
        {
            // 这里处理结束
            SeaBattleManager.SetBattleResult(_missionId, _scores);
            SeaBattleReportHelper.ReportTransport(_rightNum, _wrongNum, _bombNum);
            return;
        }
        _StageGo();
    }

    private void _OnScoreEffectEnd(TransportScoreEffectItem item)
    {
        _effectPool.Recycle(item);
    }

    private async void _OnBack()
    {
        if (_stage < StaticData.GameSlideTable.ElementList.Count)
        {
            if (!await Dialog.AskAsync("", "M4_minigame1_quit".Localize())) return;
        }
        UIEngine.Stuff.Back();
    }
}