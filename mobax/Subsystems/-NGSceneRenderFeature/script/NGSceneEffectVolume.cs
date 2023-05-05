using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


[System.Serializable, VolumeComponentMenu("NGSceneEffect")]
public class NGSceneEffectVolume : VolumeComponent, IPostProcessComponent
{
    
   [Tooltip("SceneEffect")]
   public BoolParameter EnableEffect = new BoolParameter(false);
    /*
   [Tooltip("�����ͼ")]
   public TextureParameter NoiseTexture = new TextureParameter(null,false);

   [Range(0, 1), Tooltip("����ɫ")]
   public ColorParameter FogColor = new ColorParameter(Color.white);

   [Tooltip("��ʼ���")]
   public FloatParameter DepthStart = new FloatParameter(0f);

   [Tooltip("�������")]
   public FloatParameter DepthEnd = new FloatParameter(6f);

   [Tooltip("��ʼ�߶�")]
   public FloatParameter HeightStart = new FloatParameter(0f);

   [Tooltip("�����߶�")]
   public FloatParameter HeightEnd = new FloatParameter(6f);

   [Range(0, 0.1f), Tooltip("��������")]
   public FloatParameter WorldPosScale = new FloatParameter(0.1f);

   [Range(0, 1f), Tooltip("X��ƫ��ϵ��")]
   public FloatParameter NoiseSpX = new FloatParameter(0.1f);

   [Range(0, 1f), Tooltip("Y��ƫ��ϵ��")]
   public FloatParameter NoiseSpY = new FloatParameter(0.1f);

   [Range(0, 30f), Tooltip("�������")]
   public FloatParameter DepthNoiseScale = new FloatParameter(2);

   [Range(0, 30f), Tooltip("�߶�����")]
   public FloatParameter HeightNoiseScale = new FloatParameter(2f);

   [Range(0, 0.1f), Tooltip("����ܶ�")]
   public FloatParameter DepthDensity = new FloatParameter(1f);

   [Range(0, 1f), Tooltip("�߶��ܶ�")]
   public FloatParameter HeightDensity = new FloatParameter(1f);

   [Range(0, 1), Tooltip("�������")]
   public FloatParameter DepthHeightRatio = new FloatParameter(0.75f);
   
      */
    public bool IsActive() => EnableEffect == true;

    public bool IsTileCompatible() => false;
  


}