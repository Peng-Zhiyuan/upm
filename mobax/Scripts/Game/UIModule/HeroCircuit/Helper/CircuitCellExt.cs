using System;
using System.Collections.Generic;
using Game.Hero.Base;
using UnityEngine;
using Object = UnityEngine.Object;

public enum CircuitBlockColorEnum
{
    Gray = 5,
    Preview = 6,
}

internal enum CircuitUnlockType
{
    T0_Basic = 0,
    T1_Star,
    T2_Level,
}

public static class CircuitCellExt
{
    /** 地图的横列 */
    public const int MapSizeH = 9;
    /** 地图的纵列 */
    public const int MapSizeV = 9;
    /** 单块拼图边长 */
    public const int UnitLength = 74;
    private static Dictionary<int, Color> _colorMap;

    /** 获取坐标 */
    public static Pos GetCoordinate(int shape, Vector2 pos)
    {
        var nodes = StaticData.PuzzleShapeTable.TryGet(shape).Dots;
        var size = GetSize(nodes);
        var coordinateX = _GetCoordinateValue(pos.x, size.Width, UnitLength); // 坐标x
        var coordinateY = _GetCoordinateValue(pos.y, size.Height, UnitLength); // 坐标y
        return new Pos(coordinateX, coordinateY);
    }
    
    /** 获取位置 */
    public static Vector2 GetPos(int shape, int putX, int putY)
    {
        var nodes = StaticData.PuzzleShapeTable.TryGet(shape).Dots;
        var size = GetSize(nodes);
        var w = UnitLength;
        var x = (putX + (size.Width - 1) / 2f) * w;
        var y = (putY + (size.Height - 1) / 2f) * w;

        return new Vector2(x, y);
    }

    public static Vector2 GetPos(HeroCircuitInfo circuitInfo)
    {
        var coordinate = circuitInfo.Coordinate;
        return GetPos(circuitInfo.Shape, coordinate.X, coordinate.Y);
    }

    public static Rectangle GetSize(List<Pos> nodes)
    {
        var rect = new Rectangle();
        foreach (var node in nodes)
        {
            if (node.X + 1 > rect.Width)
            {
                rect.Width = node.X + 1;
            }
            if (node.Y + 1 > rect.Height)
            {
                rect.Height = node.Y + 1;
            }
        }

        return rect;
    }

    /** 属性对应的颜色 */
    public static Color GetColor(int circuitType, float alpha = 1f)
    {
        _colorMap ??= new Dictionary<int, Color>();

        if (!_colorMap.TryGetValue(circuitType, out var color))
        {
            var colorStrCfg = StaticData.PuzzleColorTable.TryGet(circuitType);
            var colorVal = 0;
            if (colorStrCfg != null)
            {
                colorVal = Convert.ToInt32(colorStrCfg.Color, 16);
            }
            var r = ((colorVal & 0xff0000) >> 16) / 255f;
            var g = ((colorVal & 0x00ff00) >> 8) / 255f; 
            var b = ((colorVal & 0x0000ff) >> 0) / 255f;
            _colorMap[circuitType] = color = new Color(r, g, b, alpha);
        }

        return color;
    }

    // 吸附到正确的格子上
    private static int _GetCoordinateValue(float val, int size, int length)
    {
        int result;
        if (size % 2 == 0)
        {
            result = (int) Mathf.Floor(val / length) - (size - 2) / 2;
        }
        else
        {
            result = (int) Mathf.Round(val / length) - (size - 1) / 2;
        }

        return result;
    }
}