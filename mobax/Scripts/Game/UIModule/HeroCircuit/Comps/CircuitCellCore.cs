using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CircuitCellCore : MonoBehaviour, ICanvasRaycastFilter
{
    private const int StandardLength = 2;
    private const float BgAlpha = .2f;
    
    public Image blockImage;
    public Transform pieceNode;
    public CircuitPieceImage piecePrefab;
    public bool needFit;
    
    private List<CircuitBlockSquare> _squares;
    private HeroCircuitInfo _circuitInfo;
    private BehaviourPool<CircuitPieceImage> _piecePool;
    private int _circuitId;
    private int _colorVal;
    private int _mentorFlag;

    public HeroCircuitInfo CircuitInfo => _circuitInfo;
    
    public bool IsRaycastLocationValid(Vector2 sp, Camera cam)
    {
        if (null == _squares) return false;
            
        RectTransformUtility.ScreenPointToLocalPointInRectangle(this.rectTransform(), sp, cam, out Vector2 pos);
        // 只要在任意一个square中，就生效
        foreach (var square in _squares)
        {
            if (pos.x > square.X && pos.x < square.X + square.Len &&  pos.y > square.Y && pos.y < square.Y + square.Len)
            {
                return true;
            }
        }

        return false;
    }

    public void SetShape(int shapeId)
    {
        _SetShape(shapeId, _mentorFlag);
    }

    public void Render(HeroCircuitInfo circuitInfo, int? shape = null)
    {
        _circuitInfo = circuitInfo;
        SetColor(_circuitInfo.Color);
        _InternalRenderBlock(shape ?? circuitInfo.Shape, circuitInfo.Bind ? 1 : 0);
    }
    
    public void Render(int circuitId)
    {
        _circuitId = circuitId;
        var circuitRow = StaticData.PuzzleTable.TryGet(circuitId);
        // 得到形状配置
        if (null == circuitRow)
        {
            Debug.LogError($"【拼图】Circuit表中不存在拼图id={circuitId}");
            return;
        }
        
        foreach (var circuitShapeRow in StaticData.PuzzleShapeTable.ElementList)
        {
            if (circuitShapeRow.Qlv == circuitRow.Qlv)
            {
                // 算出技能颜色
                var circuitSkillRow = StaticData.PuzzlePoolTable.TryGet(circuitRow.Skill);
                var circuitSkillCfg = circuitSkillRow.Colls.First();
                var skillRow = StaticData.SkillTable.TryGet(circuitSkillCfg.Skill);
                SetColor(skillRow.Colls.First().colorType);
                // 设置拼图
                _InternalRenderBlock(circuitShapeRow.Id, circuitRow.Key);
                break;
            }
        }
    }

    public void ResetColor()
    {
        SetColor(CircuitInfo.Color);
    }

    public void SetColor(int colorVal)
    {
        _colorVal = colorVal;
        blockImage.color = CircuitCellExt.GetColor(colorVal, BgAlpha);
        
        var list = _piecePool.List;
        if (null != list)
        {
            foreach (var circuitPieceImage in list)
            {
                circuitPieceImage.color = Color.white;
            }
        }
    }

    public void SetGray()
    {
        blockImage.SetGraphicAlpha(BgAlpha * .2f);
        
        var list = _piecePool.List;
        if (null != list)
        {
            foreach (var circuitPieceImage in list)
            {
                circuitPieceImage.color = Color.gray;
            }
        }
    }

    public void SetAlpha(float alpha)
    {
        blockImage.SetGraphicAlpha(alpha * BgAlpha);

        var list = _piecePool.List;
        foreach (var circuitPieceImage in list)
        {
            circuitPieceImage.SetGraphicAlpha(alpha);
        }
    }

    private void Awake()
    {
        _piecePool = new BehaviourPool<CircuitPieceImage>();
        _piecePool.SetParent(pieceNode);
        _piecePool.SetPrefab(piecePrefab);
    }

    private void _InternalRenderBlock(int shapeId, int mentorFlag)
    {
        var circuitShapeCfg = StaticData.PuzzleShapeTable.TryGet(shapeId);
        UiUtil.SetSpriteInBackground(blockImage, () => $"circuit_bg_{circuitShapeCfg.Qlv}_{circuitShapeCfg.Shapetype}.png",
            _ => blockImage.SetNativeSize(), BgAlpha);
        _SetShape(shapeId, mentorFlag);

        // 计算是否要缩放
        var size = CircuitCellExt.GetSize(circuitShapeCfg.Dots);
        var maxLen = Math.Max(size.Width, size.Height);
        Vector3 scale;
        if (needFit && maxLen > StandardLength)
        {
            scale = Vector3.one * StandardLength / maxLen;
        }
        else
        {
            scale = Vector3.one;
        }
        blockImage.SetLocalScale(scale); 
        pieceNode.SetLocalScale(scale); 
    }

    private void _SetShape(int shapeId, int mentorFlag)
    {
        var circuitShapeCfg = StaticData.PuzzleShapeTable.TryGet(shapeId);
        var size = CircuitCellExt.GetSize(circuitShapeCfg.Dots);
        var rotation = Quaternion.Euler(0, 0, -circuitShapeCfg.Angle);
        blockImage.rectTransform.localRotation = rotation;
        
        var w = CircuitCellExt.UnitLength;
        _ResetSquares();
        _piecePool.MarkClear();
        foreach (var circuitNode in circuitShapeCfg.Dots)
        {
            var x = (-size.Width + circuitNode.X * 2) * w / 2;
            var y = (-size.Height + circuitNode.Y * 2) * w / 2;
            _AddSquare(x, y, w);
            // 放置pieces
            var piece = _piecePool.Get();
            piece.Set(_colorVal, mentorFlag);
            piece.rectTransform.anchoredPosition = new Vector2(x + w / 2, y + w / 2);
        }

        _piecePool.RecycleLeft();
        _mentorFlag = mentorFlag;
    }
    
    private void _ResetSquares()
    {
        if (null == _squares)
        {
            _squares = new List<CircuitBlockSquare>();
        }
        else
        {
            _squares.Clear();
        }
    }
    
    private void _AddSquare(int x, int y, int len)
    {
        var square = new CircuitBlockSquare(x, y, len);
        _squares.Add(square);
    }
}

internal readonly struct CircuitBlockSquare
{
    public int X { get; }
    public int Y { get; }
    public int Len { get; }

    public CircuitBlockSquare(int x, int y, int len)
    {
        X = x;
        Y = y;
        Len = len;
    }
}