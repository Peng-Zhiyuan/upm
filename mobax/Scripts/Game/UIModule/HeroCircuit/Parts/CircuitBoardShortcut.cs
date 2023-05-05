using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 拼图面板浏览块
/// </summary>
public class CircuitBoardShortcut : MonoBehaviour
{
    public RectTransform container;
    public Transform spotsNode;
    public Transform circuitNode;
    public CircuitCellCore circuitPrefab;
    public CircuitCellSpot spotPrefab;
    
    private int _type;
    private int _starStage;
    private int _lvStage;
    private HeroInfo _heroInfo;
    private BehaviourPool<CircuitCellSpot> _spotPool;
    private BehaviourPool<CircuitCellCore> _circuitPool;

    public void SetInfo(HeroInfo heroInfo)
    {
        _heroInfo = heroInfo;

        _SetSpots();
        _PutCircuits();
    }

    private void Awake()
    {
        _spotPool = new BehaviourPool<CircuitCellSpot>();
        _spotPool.SetParent(spotsNode);
        _spotPool.SetPrefab(spotPrefab);
        _circuitPool = new BehaviourPool<CircuitCellCore>();
        _circuitPool.SetParent(circuitNode);
        _circuitPool.SetPrefab(circuitPrefab);
        // spotsNode的宽高也是能直接确定的
        container.sizeDelta = new Vector2(
            CircuitCellExt.MapSizeH * CircuitCellExt.UnitLength,
            CircuitCellExt.MapSizeV * CircuitCellExt.UnitLength);
    }
    
    /// <summary>
    /// 布置背景（可放区域和锁住区域）
    /// </summary>
    private void _SetSpots()
    {
        var type = _heroInfo.Job;
        var starStage = _heroInfo.CircuitStarStage;
        var lvStage = _heroInfo.CircuitLevelStage;
        if (type == _type && _starStage == starStage && _lvStage == lvStage) return;
        
        var halfH = CircuitCellExt.MapSizeH / 2;
        var halfV = CircuitCellExt.MapSizeV / 2;
        var arr = StaticData.PuzzleBgTable.TryGet(type).Colls;
        _spotPool.MarkClear();
        foreach (var row in arr)
        {
            var unlocked = false;
            switch ((CircuitUnlockType) row.Type)
            {
                case CircuitUnlockType.T0_Basic:
                // 如果星级小于当前值， 就是开放了
                case CircuitUnlockType.T1_Star when row.Stage <= starStage:
                // 如果等级小于当前值， 就是开放了
                case CircuitUnlockType.T2_Level when row.Stage <= lvStage:
                    unlocked = true;
                    break;
            }

            foreach (var rowNode in row.Dots)
            {
                var x = rowNode.X + halfH;
                var y = rowNode.Y + halfV;
                var spotCell = _spotPool.Get();
                spotCell.SetLocked(!unlocked);
                spotCell.rectTransform().anchoredPosition =
                    new Vector2((x + .5f) * CircuitCellExt.UnitLength, (y + .5f) * CircuitCellExt.UnitLength);
            }
        }
        _spotPool.RecycleLeft();
        
        _type = type;
        _starStage = starStage;
        _lvStage = lvStage;
    }

    /// <summary>
    /// 放置拼图
    /// </summary>
    private void _PutCircuits()
    {
        // 新的放上去
        var circuitInfos = HeroCircuitManager.GetCircuits(_heroInfo.HeroId);
        _circuitPool.MarkClear();
        foreach (var circuitInfo in circuitInfos)
        {
            // 开始放置实体
            var circuit = _circuitPool.Get();
            // 不需要有拖拽功能
            CircuitDragHelper.UnRegister(circuit);
            // 显示拼图块
            circuit.Render(circuitInfo);
            // 放置到正确的位置
            circuit.SetAnchoredPosition(CircuitCellExt.GetPos(circuitInfo));
        }
        _circuitPool.RecycleLeft();
    }
}
