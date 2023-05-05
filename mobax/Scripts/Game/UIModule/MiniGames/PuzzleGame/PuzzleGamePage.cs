using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Game.Hero.Base;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public partial class PuzzleGamePage : Page
{
    private const int TotalStages = 3;
    
    public PuzzleBrickCell brickUnitPrefab;
    public PuzzleBrickCell brickBlockPrefab;
    public PuzzleBrickCell brickShadow;
    public RectTransform moveAnchor;
    
    private Rectangle RectUnit = new Rectangle { Width = 1, Height = 1 };
    private List<PickBrickItemInfo> _puzzleSource;
    private Dictionary<int, PuzzleBrickPickItem> _pickMap;
    private Dictionary<int, PuzzleBrickCell> _unitPuzzleMap;
    private GamePuzzleRow _puzzleCfg;
    private Queue<PuzzleBrickCell> _brickUnitPool = new Queue<PuzzleBrickCell>();
    private List<PuzzleBrickCell> _brickUnitUsedPool = new List<PuzzleBrickCell>();
    private Queue<PuzzleBrickCell> _brickBlockPool = new Queue<PuzzleBrickCell>();
    private List<PuzzleBrickCell> _onboardBricks = new List<PuzzleBrickCell>();
    private DateTime _stageStartTime;
    private DateTime _stageEndTime;
    private int _totalUsed;
    private int _missionId;
    private int _stage;
    private bool _stageSucceed;
    private bool _gameOver;
    private bool _pausing;
    private int[] _yNums;
    private List<int> _usedTimes = new List<int>();
    private List<int> _usedNum = new List<int>();

    public override void OnNavigatedTo(PageNavigateInfo navigateInfo)
    {
        if (navigateInfo.param is int missionId)
        {
            _gameOver = false;
            _stage = 0;
            _totalUsed = 0;
            _missionId = missionId;
            _usedTimes.Clear();
            _usedNum.Clear();
            _StageMoveOn();
        }
    }

    // private int vvv;
    public override void OnButton(string msg)
    {
        switch (msg)
        {
            case "back":
                break;
            case "test1":
                // if (!_gameOver)
                // {
                //     _gameOver = true;
                // }
                // else
                // {
                //     if (vvv >= _puzzleCfg.Field * _puzzleCfg.Field)
                //     {
                //         vvv = 0;
                //         _gameOver = false;
                //     }
                //
                //     var i = vvv % _puzzleCfg.Field;
                //     var j = vvv / _puzzleCfg.Field;
                //     
                //     if (!PuzzleGameManager.Instance.CheckOverlay(i, j))
                //     {
                //         var node = new Pos(i, j);
                //         var brick = _TakeBrickUnit();
                //         _PutBlockOn(brick, node, RectUnit);
                //         _unitPuzzleMap[node.Y * _puzzleCfg.Field + node.X] = brick;
                //         Debug.Log($"重新放一块上去： {i}, {j}");
                //         // 数据标识
                //         PuzzleGameManager.Instance.SetFlag(node);
                //     }
                //     else
                //     {
                //         Debug.Log($"该位置被占： {i}, {j}");
                //     }
                //     ++vvv;
                // }
                break;
        }
    }

    private void Awake()
    {
        // 默认要隐藏放置在舞台上的prefab
        var sceneBrickUnit = brickUnitPrefab.gameObject.scene;
        if (!string.IsNullOrEmpty(sceneBrickUnit.name))
        {
            brickUnitPrefab.SetActive(false);
            _brickUnitPool.Enqueue(brickUnitPrefab);
        }
        var sceneBrickBlock = brickBlockPrefab.gameObject.scene;
        if (!string.IsNullOrEmpty(sceneBrickBlock.name))
        {
            brickBlockPrefab.SetActive(false);
            _brickBlockPool.Enqueue(brickBlockPrefab);
        }

        // 初始化
        _pickMap = new Dictionary<int, PuzzleBrickPickItem>();
        _unitPuzzleMap = new Dictionary<int, PuzzleBrickCell>();
        List_puzzles.onItemRenderAction = _OnBricksRender;
        _ResetShadow();
    }

    private void Update()
    {
        if (_gameOver) return;
        if (_pausing) return;

        var now = Clock.Now;
        if (now >= _stageEndTime)
        {
            ToastManager.ShowLocalize("M4_minigame1_timeup");
            _stageSucceed = false;
            _StageMoveOn(true);
        }
    }

    private async void _StageMoveOn(bool needDelay = false)
    {
        if (_stage > 0)
        {
            // 先计算上一轮成绩
            var usedBlock = !_stageSucceed ? _puzzleCfg.Fail : _onboardBricks.Count;
            _totalUsed += usedBlock;
            _usedNum.Add(_onboardBricks.Count);
            // Debug.Log($"【stage {_stage}】用了{usedBlock}块， 当前共{_totalUsed}块");

            var costTime = 0;
            if (_stageSucceed)
            {
                costTime = (int) Mathf.Round((float) (Clock.Now - _stageStartTime).TotalSeconds);
            }
            _usedTimes.Add(costTime);
        }
        
        ++_stage;
        if (_stage > TotalStages)
        {
            _gameOver = true;
            ToastManager.ShowLocalize("M4_minigame1_gameover");
            Txt_cd.GetComponent<CountdownBehaviour>().Stop();
            SeaBattleManager.SetBattleResult(_missionId, _totalUsed);
            // 上报
            SeaBattleReportHelper.ReportPuzzle(_usedNum, _usedTimes);
            return;
        }

        if (needDelay)
        {
            _pausing = true;
            await Task.Delay(1000);
            _pausing = false;
        }

        _RefreshPuzzle();
        _RefreshStage();
    }

    private void _RefreshStage()
    {
        for (var index = 1; index < TotalStages; ++index)
        {
            var stage = index + 1;
            var dot = Bar_progress.Find($"Dot_{stage}");
            if (null == dot)
            {
                Debug.LogWarning($"【SeaBattle】不存在该阶段：{stage}");
            }
            else
            {
                dot.SetActive(stage <= _stage);
            }
        }

        _stageStartTime = Clock.Now;
        _stageEndTime = _stageStartTime.AddSeconds(_puzzleCfg.Time);
        Txt_cd.GetComponent<CountdownBehaviour>().SetEnd(_stageEndTime);
    }

    private void _RefreshPuzzle()
    {
        // 重置数据
        var man = PuzzleGameManager.Instance;
        _puzzleCfg = man.FetchPuzzle();
        
        // 初始化数据
        _yNums ??= new int[_puzzleCfg.Field];
        // 刷新界面信息
        _ClearOnboardBricks();
        // 重置地图
        for (var i = 0; i < _puzzleCfg.Field; ++i)
        {
            for (var j = 0; j < _puzzleCfg.Field;  ++j)
            {
                if (!man.CheckOverlay(i, j))
                {
                    var node = new Pos(i, j);
                    var brick = _TakeBrickUnit();
                    _PutBlockOn(brick, node, RectUnit);
                    _unitPuzzleMap[node.Y * _puzzleCfg.Field + node.X] = brick;
                    // Debug.Log($"重新放一块上去： {i}, {j}");
                    // 数据标识
                    man.SetFlag(node);
                }
            }
        }

        // 列表
        var items = _puzzleCfg.pickItems;
        _puzzleSource ??= new List<PickBrickItemInfo>(items.Count);
        for (var index = 0; index < items.Count; index++)
        {
            var pickBrickInfo = items[index];
            PickBrickItemInfo info;
            if (index >= _puzzleSource.Count)
            {
                info = new PickBrickItemInfo();
                _puzzleSource.Add(info);
            }
            else
            {
                info = _puzzleSource[index];
            }
            info.Num = pickBrickInfo.Num;
            info.Type = pickBrickInfo.Type;
        }

        _RefreshPuzzlePageList();
        _RefreshUsedCount();
        // 然后把不要的格子都隐藏掉
        for (var i = 0; i < _puzzleCfg.Nodes.Count; i++)
        {
            var node = _puzzleCfg.Nodes[i];
            var block = _GetBlockUnit(node);
            man.ClearFlag(node);
            _yNums[node.Y] -= 1;

            block.brickImage.DOFade(0, .4f).SetDelay(.02f * i).OnComplete(() =>
            {
                UiUtil.SetAlpha(block.brickImage, 1);
                block.SetActive(false);
                block.transform.SetParent(Node_recycler);
                _brickUnitUsedPool.Remove(block);
            });
        }
    }
    
    private void _RefreshPuzzlePageList()
    {
        _pickMap.Clear();
        List_puzzles.numItems = (uint) _puzzleSource.Count;
    }

    private PuzzleBrickCell _TakeBrickUnit()
    {
        var brick = _brickUnitPool.Count > 0 ? _brickUnitPool.Dequeue() : Instantiate(brickUnitPrefab, Node_puzzles);
        brick.SetActive(true);
        brick.transform.SetParent(Node_puzzles);
        _brickUnitUsedPool.Add(brick);
        
        return brick;
    }

    private PuzzleBrickCell _TakeBrickBlock()
    {
        var brick = _brickBlockPool.Count > 0 ? _brickBlockPool.Dequeue() : Instantiate(brickBlockPrefab, Node_puzzles);
        brick.transform.parent = Node_puzzles;
        brick.SetActive(true);
        
        return brick;
    }

    private void _RecycleBrickBlock(PuzzleBrickCell brick)
    {
        brick.SetActive(false);
        brick.transform.parent = Node_recycler;
        _brickBlockPool.Enqueue(brick);
    }
    
    private void _OnBricksRender(int index, Transform tf)
    {
        var cell = tf.GetComponent<PuzzleBrickPickItem>();
        var brickInfo = _puzzleSource[index];
        cell.SetInfo(brickInfo);
        _pickMap[brickInfo.Type] = cell;
        // 拖拽逻辑绑定
        var dragBehaviour = tf.GetComponent<DragBehaviour>();
        dragBehaviour.onClick = _OnBrickClick;
        dragBehaviour.onDragStart = _OnBrickDragged;
        dragBehaviour.onDragging = _OnBrickMoving;
        dragBehaviour.onDragEnd = _OnBrickDone;
    }

    private void _OnBrickClick(DragBehaviour behaviour)
    {
        var item = behaviour.GetComponent<PuzzleBrickPickItem>();
        item.Turn();
    }

    private void _OnBrickDragged(DragBehaviour behaviour)
    {
        var brickItem = behaviour.GetComponent<PuzzleBrickPickItem>();
        var block = _TakeBrickBlock();
        behaviour.DraggingRt = block.rectTransform();
        block.Render(brickItem.Type, brickItem.Shape);

        var settledBrickBehaviour = block.GetComponent<DragBehaviour>();
        settledBrickBehaviour.onDragStart = _OnSettledBrickDragged;
        settledBrickBehaviour.onDragging = _OnSettledBrickMoving;
        settledBrickBehaviour.onDragEnd = _OnSettledBrickDone;
        
        // 更新影子
        brickShadow.Render(brickItem.Type, brickItem.Shape, .5f);
    }
    
    private void _OnBrickMoving(DragBehaviour behaviour)
    {
        var blockRt = behaviour.DraggingRt;
        _ShadowFollow(blockRt);
    }
    
    private void _OnBrickDone(DragBehaviour behaviour)
    {
        var brickItem = behaviour.GetComponent<PuzzleBrickPickItem>();
        var blockRt = behaviour.DraggingRt;
        var block = blockRt.GetComponent<PuzzleBrickCell>();
        var coordinate = blockRt.anchoredPosition;
        var shape = block.Shape;
        var pos = _GetCoordinate(shape, coordinate);
        var nodes = StaticData.PuzzleTypeTable.TryGet(shape).Nodes;
        var size = CircuitCellExt.GetSize(nodes);
        if (_IsOutside(pos, size) || _CheckOverlay(shape, pos))
        {
            // 出界或者重叠则回收
            _RecycleBrickBlock(block);
            _ResetShadow();
            return;
        }

        _onboardBricks.Add(block);
        // 更新数量
        _RefreshUsedCount();
        brickItem.Increase(-1);
        // 位置放好
        _PutBlockOn(block, pos, size);
        // 标识占位
        _BlockFlagsOn(pos, nodes);
        // 隐藏影子
        _ResetShadow();

        if (PuzzleGameManager.Instance.Completed)
        {
            _stageSucceed = true;
            ToastManager.ShowLocalize("M4_minigame1_nextRound");
            _StageMoveOn(true);
        }
    }

    private void _OnSettledBrickDragged(DragBehaviour behaviour)
    {
        var block = behaviour.GetComponent<PuzzleBrickCell>();
        // 先标识拿掉
        _BlockFlagsOff(block.Pos, block.Shape);
        // 影子形状
        brickShadow.Render(block.Type, block.Shape, .2f);
    }

    private void _OnSettledBrickMoving(DragBehaviour behaviour)
    {
        var blockRt = behaviour.DraggingRt;
        _ShadowFollow(blockRt);
    }

    private void _OnSettledBrickDone(DragBehaviour behaviour)
    {
        var block = behaviour.GetComponent<PuzzleBrickCell>();
        var coordinate = block.rectTransform().anchoredPosition;
        var shape = block.Shape;
        var pos = _GetCoordinate(shape, coordinate);
        var nodes = StaticData.PuzzleTypeTable.TryGet(shape).Nodes;
        var size = CircuitCellExt.GetSize(nodes);
        if (_IsOutside(pos, size))
        {
            // 出界则回收
            var layer = block.Pos.Y + block.LayerOffset;
            _yNums[layer] -= 1;
            _RecycleBrickBlock(block);
            _onboardBricks.Remove(block);
            _RefreshUsedCount();
            foreach (var pickBrickItemInfo in _puzzleSource)
            {
                if (pickBrickItemInfo.Type == block.Type)
                {
                    // 增加一个回去了
                    pickBrickItemInfo.Num += 1;
                    if (_pickMap.TryGetValue(block.Type, out var cell))
                    {
                        cell.RefreshCount();
                    }
                    
                    break;
                }
            }
            return;
        }

        // 如果是重叠， 那么回到之前的位置上去
        if (_CheckOverlay(shape, pos))
        {
            pos = block.Pos;
        }
        // 位置放好
        _PutBlockOn(block, pos, size);
        // 标识占位
        _BlockFlagsOn(pos, nodes);
        // 隐藏影子
        _ResetShadow();
    }

    private void _ClearOnboardBricks()
    {
        foreach (var puzzleBrickCell in _onboardBricks)
        {
            var pos = puzzleBrickCell.Pos;
            // 然后这些位置也都要清掉标记
            foreach (var node in puzzleBrickCell.Nodes)
            {
                PuzzleGameManager.Instance.ClearFlag(pos.X + node.X, pos.Y + node.Y);
            }

            var layer = pos.Y + puzzleBrickCell.LayerOffset;
            _yNums[layer] -= 1;
            _RecycleBrickBlock(puzzleBrickCell);
        }

        _onboardBricks.Clear();
    }

    private void _RefreshUsedCount()
    {
        Txt_brickNum.text = $"{_onboardBricks.Count}";
    }
    
    private Pos _GetCoordinate(int shape, Vector2 pos)
    {
        var nodes = StaticData.PuzzleTypeTable.TryGet(shape).Nodes;
        var size = CircuitCellExt.GetSize(nodes);
        var coordinateX = _GetCoordinateValue(pos.x, size.Width, PuzzleBrickCell.DefaultWidth); // 坐标x
        var coordinateY = _GetCoordinateValue(pos.y, size.Height, PuzzleBrickCell.DefaultHeight); // 坐标y
        return new Pos(coordinateX, coordinateY);
    }

    private int _GetCoordinateValue(float val, int len, int defaultLength)
    {
        return (int) Mathf.Round(val / defaultLength - len / 2.0f);
    }

    private bool _IsOutside(Pos pos, Rectangle size)
    {
        return pos.X < 0 || pos.Y < 0 || pos.X + size.Width > _puzzleCfg.Field ||
               pos.Y + size.Height > _puzzleCfg.Field;
    }

    private bool _CheckOverlay(int shape, Pos pos, bool showOverlay = false)
    {
        var nodes = StaticData.PuzzleTypeTable.TryGet(shape).Nodes;
        var overlay = false;
        
        foreach (var node in nodes)
        {
            var x = pos.X + node.X;
            var y = pos.Y + node.Y;
            if (PuzzleGameManager.Instance.CheckOverlay(x, y))
            {
                if (!showOverlay) return true;

                Node_forbiddens.SetForbidden(x, y);
                overlay = true;
            }
        }

        return overlay;
    }

    private void _BlockFlagsOn(Pos pos, int shape)
    {
        var nodes = StaticData.PuzzleTypeTable.TryGet(shape).Nodes;
        _BlockFlagsOn(pos, nodes);
    }

    private void _BlockFlagsOn(Pos pos, List<Pos> nodes)
    {
        foreach (var node in nodes)
        {
            var x = pos.X + node.X;
            var y = pos.Y + node.Y;
            PuzzleGameManager.Instance.SetFlag(x, y);
        }
    }

    private void _BlockFlagsOff(Pos pos, int shape)
    {
        var nodes = StaticData.PuzzleTypeTable.TryGet(shape).Nodes;
        _BlockFlagsOff(pos, nodes);
    }
    
    private void _BlockFlagsOff(Pos pos, List<Pos> nodes)
    {
        foreach (var node in nodes)
        {
            var x = pos.X + node.X;
            var y = pos.Y + node.Y;
            PuzzleGameManager.Instance.ClearFlag(x, y);
        }
    }

    private void _ShadowFollow(RectTransform blockRt)
    {
        var block = blockRt.GetComponent<PuzzleBrickCell>();
        moveAnchor.position = blockRt.position;
        var pos = _GetCoordinate(block.Shape, moveAnchor.anchoredPosition);
        if (brickShadow.Pos == pos) return;

        _PutShadowOn(pos);
    }

    private void _ResetShadow()
    {
        brickShadow.Pos = Pos.Default;
        brickShadow.SetActive(false);
        Node_forbiddens.Clear();
    }

    private void _PutShadowOn(Pos pos)
    {
        var block = brickShadow;
        var nodes = StaticData.PuzzleTypeTable.TryGet(block.Shape).Nodes;
        var size = CircuitCellExt.GetSize(nodes);
        
        Node_forbiddens.Clear();
        if (_IsOutside(pos, size))
        {
            block.Pos = pos;
            block.SetActive(false);
        }
        else
        {
            _CheckOverlay(block.Shape, pos, true);
            block.SetActive(true);
            _PutBlockOn(block, pos, size, false);
        }
    }

    private void _PutBlockOn(PuzzleBrickCell block, Pos pos, Rectangle size, bool needReorder = true)
    {
        var x = (pos.X + size.Width / 2f) * PuzzleBrickCell.DefaultWidth;
        var y = (pos.Y + size.Height / 2f) * PuzzleBrickCell.DefaultHeight;
        block.SetAnchoredPosition(new Vector2(x, y));
        block.Pos = pos;

        if (needReorder)
        {
            // 然后放到正确的层级
            var num = 0;
            var layer = pos.Y + block.LayerOffset;
            for (var i = _puzzleCfg.Field - 1; i >= layer; --i)
            {
                num += _yNums[i];
            }
            block.transform.SetSiblingIndex(num);
            _yNums[layer] += 1;
        }
    }

    private PuzzleBrickCell _GetBlockUnit(Pos pos)
    {
        return _unitPuzzleMap[pos.Y * _puzzleCfg.Field + pos.X];
    }

    private async void _OnBack()
    {
        if (!_gameOver)
        {
            if (!await Dialog.AskAsync("", "M4_minigame1_quit".Localize())) return;
        }
        UIEngine.Stuff.Back();
    }
}

public class PickBrickItemInfo
{
    public int Type;
    public int Shape;
    public int Num;
}