using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CircuitPieceImage : Image
{
    public static int Length = 53;
    
    /// <summary>
    /// 设置图片
    /// </summary>
    /// <param name="colorType"></param>
    /// <param name="mentorFlag"></param>
    public void Set(int colorType, int mentorFlag)
    {
        UiUtil.SetSpriteInBackground(this, () => $"circuit_{colorType}_{mentorFlag}.png",
            _ => SetNativeSize());
    }
}
