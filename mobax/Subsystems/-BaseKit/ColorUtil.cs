using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorUtil
{
    /// <summary>
    /// 副本聊天文字颜色
    /// </summary>
    public static Color ChatRaids = ColorUtil.HexToColor("88C3FF");

    /// <summary>
    /// 世界聊天文字颜色
    /// </summary>
    public static Color ChatWorld = ColorUtil.HexToColor("FCE3A0");

    /// <summary>
    /// 好友聊天文字颜色
    /// </summary>
    public static Color ChatFriend = ColorUtil.HexToColor("FEAAF3");

    /// <summary>
    /// 好友聊天文字颜色
    /// </summary>
    public static Color ChatGuild = ColorUtil.HexToColor("63d9ab");

    public static Color HexToColor(string hexColor)
    {
        if (string.IsNullOrEmpty(hexColor)) return new Color(0, 0, 0);
        var r = int.Parse(hexColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        var g = int.Parse(hexColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        var b = int.Parse(hexColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color(r / 255f, g / 255f, b / 255f);
    }

    public static Color SetAlpha(Color c, float alpha)
    {
        c.a = alpha;
        return c;
    }
}

public class Colors
{
    public static Color rulerBG = new Color(80f / 255f, 80f / 255f, 80f / 255f);
    public static Color timeLineBG = new Color(30f / 255f, 30f / 255f, 30f / 255f);
    public static Color rulerLine1 = new Color(0.8f, 0.8f, 0.8f, 0.8f);
    public static Color rulerLine2 = new Color(0.5f, 0.5f, 0.5f, 0.8f);
    public static Color rulerLine3 = new Color(0.5f, 0.5f, 0.5f, 0.3f);
    public static Color ChoosedFrame = new Color(0f, 1f, 0f, 0.5f);
    public static Color ActivationFrame = new Color(0f, 1f, 1f, 0.5f);

    public static Color PlotBubbleFrame = new Color(1f, 1f, 0f, 0.5f);
    public static Color PlotMaskFrame = new Color(0f, 1f, 0.5f, 0.5f);
    public static Color PlotMaskDissolve = new Color(1f, 0.5f, 0f, 0.5f);

    public static Color AnimationFrame = new Color(0f, 1f, 0f, 0.5f);
    public static Color AudioFrame = new Color(1f, 0.5f, 0f, 0.5f);
    public static Color EffectFrame = new Color(0f, 0.5f, 1f, 0.5f);
    public static Color ExplosionFrame = new Color(1f, 0.5f, 1f, 0.5f);
    public static Color AttackBoxFrame = new Color(1f, 0.5f, 1f, 0.5f);
    public static Color HurtBoxFrame = new Color(0f, 0.5f, 0.5f, 0.5f);
    public static Color TrajectoryFrame = new Color(0.5f, 0f, 1f, 0.5f);
    public static Color CameraAniFrame = new Color(0.5f, 0.5f, 1f, 0.5f);
    public static Color ScreenShakeFrame = new Color(0f, 1f, 0.5f, 0.5f);
    public static Color ScreenDimmingFrame = new Color(0f, 0.3f, 0.8f, 0.5f);
    public static Color ImmpulseFrame = new Color(1f, 0.3f, 0.8f, 0.5f);
    public static Color TimeStopFrame = new Color(1f, 0f, 1f, 0.5f);
    public static Color TeleportFrame = new Color(1f, 1f, 0f, 0.5f);
    public static Color FrameBreakFrame = new Color(1f, 0.3f, 0.6f, 0.5f);
    public static Color BattleEventFrame = new Color(0.3f, 0.4f, 0.18f, 0.5f);
    public static Color QteTurnFrame = new Color(1f, 0f, 1f, 0.5f);
    public static Color MoveFrame = new Color(1f, 0.3f, 0.5f, 0.5f);
    public static Color PreWaringFrame = new Color(1f, 0f, 0f, 0.5f);
    public static Color DotFrame = new Color(1f, 1f, 0f, 0.5f);
    public static Color ShieldWallFrame = new Color(0.5f, 0.8f, 0.8f, 0.5f);
    public static Color SummoningFrame = new Color(1f, 0.5f, 0.5f, 0.5f);
    public static Color BeatBackFrame = new Color(0.5f, 0.3f, 0.8f, 1f);
    public static Color ParabolaDirectionFrame = new Color(0.2f, 0.8f, 0.5f, 1f);
    public static Color JumpFrame = new Color(0.5f, 0.5f, 0.8f, 1f);
    public static Color AirBorneFrame = new Color(1f, 0f, 1f, 0.5f);
    public static Color TimeLineFrame = new Color(0.5f, 0.5f, 1f, 0.5f);
    public static Color VirtulCameraFrame = new Color(0.5f, 0.5f, 1f, 0.5f);
    public static Color FresnelFrame = new Color(0.3f, 0.2f, 1f, 0.5f);
}