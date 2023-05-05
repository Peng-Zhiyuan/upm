using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 这个拆分开来是为了和启动游戏里的可做统一
/// </summary>
public class LauncherGameIcon : MonoBehaviour
{
    private string _flag;
    
    public LauncherSoloComponent Selector;

    public string Flag
    {
        set
        {
            _flag = value;
            Selector.Selected = Array.IndexOf(LauncherGameData.ItemSource, value);
        }

        get => _flag;
    }
}