using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


[System.Serializable, VolumeComponentMenu("DepthHeightFog")]
public class DepthHeightFogVolume : VolumeComponent, IPostProcessComponent
{
    
   [Tooltip("是否开启效果")]
   public BoolParameter EnableEffect = new BoolParameter(false);
    /*
   [Tooltip("噪点贴图")]
   public TextureParameter NoiseTexture = new TextureParameter(null,false);

   [Range(0, 1), Tooltip("雾颜色")]
   public ColorParameter FogColor = new ColorParameter(Color.white);

   [Tooltip("起始深度")]
   public FloatParameter DepthStart = new FloatParameter(0f);

   [Tooltip("结束深度")]
   public FloatParameter DepthEnd = new FloatParameter(6f);

   [Tooltip("起始高度")]
   public FloatParameter HeightStart = new FloatParameter(0f);

   [Tooltip("结束高度")]
   public FloatParameter HeightEnd = new FloatParameter(6f);

   [Range(0, 0.1f), Tooltip("坐标缩放")]
   public FloatParameter WorldPosScale = new FloatParameter(0.1f);

   [Range(0, 1f), Tooltip("X轴偏移系数")]
   public FloatParameter NoiseSpX = new FloatParameter(0.1f);

   [Range(0, 1f), Tooltip("Y轴偏移系数")]
   public FloatParameter NoiseSpY = new FloatParameter(0.1f);

   [Range(0, 30f), Tooltip("深度缩放")]
   public FloatParameter DepthNoiseScale = new FloatParameter(2);

   [Range(0, 30f), Tooltip("高度缩放")]
   public FloatParameter HeightNoiseScale = new FloatParameter(2f);

   [Range(0, 0.1f), Tooltip("深度密度")]
   public FloatParameter DepthDensity = new FloatParameter(1f);

   [Range(0, 1f), Tooltip("高度密度")]
   public FloatParameter HeightDensity = new FloatParameter(1f);

   [Range(0, 1), Tooltip("高深比例")]
   public FloatParameter DepthHeightRatio = new FloatParameter(0.75f);
   
      */
    public bool IsActive() => EnableEffect == true;

    public bool IsTileCompatible() => false;
  


}