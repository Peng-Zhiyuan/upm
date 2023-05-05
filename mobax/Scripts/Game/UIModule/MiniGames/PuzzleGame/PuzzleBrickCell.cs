using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleBrickCell : MonoBehaviour
{
    public const int DefaultWidth = 134;
    public const int DefaultHeight = 108;
    
    public Image brickImage;
    public int unitLength;
    public int fitMax;
    public bool clickable;
    
    private List<BlockSquare> _squares;
    private int _type;
    private int _shape;
    private float _alpha;
    private float _imageScale;

    public int LayerOffset { get; private set; }
    public int Type => _type;
    public int Shape => _shape;
    public float ImageScale => _imageScale;
    public Pos Pos { get; set; }
    public List<Pos> Nodes { get; private set; }

    public void Render(int type, int shape = 0, float alpha = 1.0f)
    {
        _type = type;
        _alpha = alpha;
        
        if (shape == 0) shape = type;
        SetShape(shape);
    }
    
    public void SetShape(int shape)
    {
        var puzzleShapeCfg = StaticData.PuzzleTypeTable.TryGet(shape);
        var size = CircuitCellExt.GetSize(puzzleShapeCfg.Nodes);
        UiUtil.SetAtlasSpriteInBackground(brickImage, "PageGroup6Atlas.spriteatlas", () => $"Brick_{shape}",
            _ => brickImage.SetNativeSize(), _alpha);
        _shape = shape;
        LayerOffset = puzzleShapeCfg.layerOffset;
        Nodes = puzzleShapeCfg.Nodes;
        
        // 显示scale
        if (unitLength == 0) unitLength = DefaultWidth;
        var maxLen = Math.Max(size.Width, size.Height);
        var baseScale = (float) unitLength / DefaultWidth;
        if (fitMax > 0 && maxLen > fitMax)
        {
            _imageScale = baseScale * fitMax / maxLen;
        }
        else
        {
            _imageScale = baseScale;
        }
        transform.SetLocalScale(Vector3.one * _imageScale); 

        var w = unitLength;
        if (clickable)
        {
            _ResetSquares();
            foreach (var puzzleNode in puzzleShapeCfg.Nodes)
            {
                _AddSquare((-size.Width + puzzleNode.X * 2) * w / 2, (-size.Height + puzzleNode.Y * 2) * w / 2, w);
            }
        }
    }
    
    private void _ResetSquares()
    {
        if (null == _squares)
        {
            _squares = new List<BlockSquare>();
        }
        else
        {
            _squares.Clear();
        }
    }
    
    private void _AddSquare(int x, int y, int len)
    {
        var square = new BlockSquare(x, y, len);
        _squares.Add(square);
    }
}

internal struct BlockSquare
{
    public int x;
    public int y;
    public int len;

    public BlockSquare(int x, int y, int len)
    {
        this.x = x;
        this.y = y;
        this.len = len;
    }
}