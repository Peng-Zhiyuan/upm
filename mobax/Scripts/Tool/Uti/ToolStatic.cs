using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ToolStatic
{
    public enum  TOOL_TYPE
    {
        OTHER,
        ROLE_ROOM,
        FX_MAKER,
    }

    public static void Init(ToolBar bar)
    {
        _openList.Add(bar);
    }

    public static void Dock(ToolBar bar)
    {
        _dockList.Add(bar);
        _openList.Remove(bar);
        RefreshDockList();
    }

    public static void Open(ToolBar bar)
    {
        _openList.Add(bar);
        _dockList.Remove(bar);
        bar.ResetPos(false);
        //Greed(bar);
        RefreshDockList();
    }

    // static void Greed(ToolBar bar)
    // {
    //     for (int i = 0; i < _openList.Count; ++i)
    //     {
    //         if (_openList[i] == bar)
    //         {
    //             continue;
    //         }
    //     }
    //
    //     Rect _rect=bar.GetCompassRect();
    //     _rect.x = 0;
    //     _rect.y = -30;
    //     bool _fit = false;
    //     int _guide = 0;
    //     while (!_fit)
    //     {
    //         int _okNum = 0;
    //         for (int i = 0; i < _openList.Count; ++i)
    //         {
    //             if (_openList[i] == bar)
    //             {
    //                 continue;
    //             }
    //             if (_openList[i].GetCompassRect().Overlaps(_rect))
    //             {
    //                 Debug.Log($"{_rect} Overlaps {_openList[i].GetCompassRect()}");
    //                 continue;
    //             }
    //             else
    //             {
    //                 _okNum++;
    //             }
    //         }
    //
    //         if (_okNum >= _openList.Count - 1)
    //         {
    //             _fit = true;
    //         }
    //         else
    //         {
    //             if (_rect.y + _rect.height < Screen.height)
    //             {
    //                 _rect.y += _rect.height;
    //             }
    //             else
    //             {
    //                 _rect.y = 0;
    //                 _rect.x += _rect.width;
    //             }
    //         }
    //
    //         _guide++;
    //         if (_guide > 10000)
    //         {
    //             return;
    //         }
    //     }
    //     Debug.Log(_rect);
    //     _rect.y = -_rect.y;
    //     _rect.x = -_rect.x;
    //     bar.SetAnchoredPos(_rect.position);
    // }

    static void RefreshDockList()
    {
        var basePos = new Vector2(-210, 0);
        for (int i = 0; i < _dockList.Count; ++i)
        {
            var bias = new Vector2(i*(-120),0);
            _dockList[i].SetAnchoredPos(basePos+bias);
        }
    }

    public static void ResetAll()
    {
        for (int i = 0; i < _dockList.Count; ++i)
        {
            _openList.Add(_dockList[i]);
            _dockList[i].SwitchOpenLogic();
            _dockList[i].ResetPos(true);
        }
        _dockList.Clear();
    }
    public static void HideAll()
    {
        for (int i = 0; i < _openList.Count; ++i)
        {
            _dockList.Add(_openList[i]);
            _openList[i].SwitchOpenLogic();
        }
        _openList.Clear();
        RefreshDockList();
    }
    
    
    public static TOOL_TYPE _tooType = TOOL_TYPE.OTHER;
    static List<ToolBar> _dockList = new List<ToolBar>();
    static List<ToolBar> _openList = new List<ToolBar>();
}