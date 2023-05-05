using System;
using UnityEngine;
using UnityEngine.UI;

public class LauncherGameItem : MonoBehaviour
{
    public LauncherGameIcon Icon;
    public Image SelectFrame;
    /** 位置信息 */
    public int Spot;

    /** 标记值 */
    public string Flag
    {
        get => Icon.Flag;
        set => Icon.Flag = value;
    }

    public bool Selected
    {
        set => SelectFrame.SetActive(value);
    }

    public RectTransform Rt
    {
        get => GetComponent<RectTransform>();
    }

    public Vector2 Pos
    {
        set => Rt.anchoredPosition = value;
    }
}