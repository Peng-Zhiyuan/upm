using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ZoomBlur : VolumeComponent,IPostProcessComponent
{
    [Tooltip("这就他妈的是偏移幅度")]
    public ClampedFloatParameter focusPower = new ClampedFloatParameter(15f,0.0001f,30f);
    [Tooltip("值越大效果越好，但是别他妈的调太大，不然要炸")]
    public ClampedIntParameter focusDetail = new ClampedIntParameter(5,1,10);
    [Tooltip("模糊中心点，模糊中心坐标已经在屏幕的中心（0,0）")]
    public Vector2Parameter focusScreenPosition = new Vector2Parameter(Vector2.zero);
    [Tooltip("降采样，默认给了2已经很好，别逼逼效果差，不然手机上跑不动")]
    public ClampedIntParameter downsample = new ClampedIntParameter(1,1,10);
    [Tooltip("显示范围")]
    public BoolParameter onlyMask = new BoolParameter(false);
    [Tooltip("整体模糊强度")]
    public ClampedFloatParameter maskIntensity = new ClampedFloatParameter(1,0,1);
    [Tooltip("外圈的大小")]
    public ClampedFloatParameter outMaskLength = new ClampedFloatParameter(0f,-1f,2f);
    [Tooltip("外圈的羽化")]
    public ClampedFloatParameter outMaskSmooth = new ClampedFloatParameter(0f,0f,1f);
    [Tooltip("内圈的羽化")]
    public ClampedFloatParameter inMaskLength = new ClampedFloatParameter(0f,-1f,2f);
    [Tooltip("内圈的羽化")]
    public ClampedFloatParameter inMaskSmooth = new ClampedFloatParameter(0f,0f,1f);
    [Tooltip("参考宽屏分辨率")]
    public IntParameter referenceResolutionX = new IntParameter(1334);

    public bool IsActive() => focusPower.value > 0f;

    public bool IsTileCompatible() => false;

}
