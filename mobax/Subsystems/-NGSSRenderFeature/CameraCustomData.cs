using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class ColorAdjustments
{
    public bool openColorAdjustments = false;
    public float PostExposure;

    [Range(-100f, 100f)] public float Contrast = 0;

    public Color ColorFilter = Color.white;

    [Range(-180, 180)] public float HueShift = 0;

    [Range(-100, 100)] public float Saturation = 0;

    [Range(0, 1)] public float fadeScene = 0;
}

public class CameraCustomData : MonoBehaviour
{
    public enum BloomQualityEnum
    {
        LowQuality = 0,
        HighQuality = 1,
    }

    [System.Serializable]
    public class BloomCustomSetting
    {
        public bool openBloom = true;
        //public BloomSettingEnum bloomState = BloomSettingEnum.Open;
        public BloomQualityEnum bloomQuality = BloomQualityEnum.LowQuality;

        [Range(0, 6)] public int Range = 4;

        [Range(0.0f, 5.0f)] public float BloomThreshold = 0.9f;

        [Range(0, 10f)] public float BloomIntensity = 0.6f;
    }
    /*
    public enum BloomSettingEnum
    {
        Off = 0,
        Open = 1,
        Custom = 2
    }
    */
    /*
    public bool openBloom = false;
    public bool openBlur = false;
    void Update()
    {
        this.Bloom.openBloom = openBloom;
        this.RadiaBlur.openBlur = openBlur;
        Debug.LogError("this.RadiaBlur.openBlur："+ this.RadiaBlur.openBlur);
    }
    */
   
    [System.Serializable]
    public class RadiaBlurSetting
    {
        public bool openBlur = false;
        public bool openSceneBlur = false;
        //[Range(0, 10f)] public float radiaBlurLevel = 10;
        [Range(0.0f, 1.0f)] public float radiaBlurCenterX = 0.5f;
        [Range(0.0f, 1.0f)] public float radiaBlurCenterY = 0.5f;
        //[Range(0.0f, 1.0f)] public float radiaBlurBufferRadius = 0;
    }

    [SerializeField]
    public RadiaBlurSetting RadiaBlur;


    [System.Serializable]
    public class FogSetting
    {
        public bool openFog = true;
    }
    [SerializeField]
    public FogSetting DepthHeightFog;

    [System.Serializable]
    public class CloudShadowSetting
    {
        public bool openCloudShadow = false;
        //public Vector4 StartXYSpeedXY;
        //public float scale;
        //public float worldSize;
    }
    [SerializeField]
    public CloudShadowSetting CloudShadow;
    /*
    [System.Serializable]
    public class GreyWhiteSetting
    {
        public bool openGreyWhite = false;
        public float greyWhiteAmount;
        public int greyWhiteSwitch;
        public int greyWhiteChange;
    }

    public GreyWhiteSetting GreyWhite;
    */
    [System.Serializable]
    public class BlackWhiteFlashSetting
    {
        public bool openBlackWhiteFlash = false;
        [Range(0.0f, 1.0f)] public float flashCenterX = 0.5f;
        [Range(0.0f, 1.0f)] public float flashCenterY = 0.5f;
        public bool flash = false;
    }

    public BlackWhiteFlashSetting BlackWhiteFlash;
    /*
    [System.Serializable]
    public class FocusLayerSetting
    {

        public bool showFocusLayer = false;

        [Range(0, 1)] public float fadeBlack = 0;
    }
    */

    public BloomCustomSetting Bloom;
    
    //public FocusLayerSetting Focus;

    public enum RoleSettingEnum
    {
        LowQuality = 0,
        MediumQuality = 1,
        HighQuality = 2,
    }

    public RoleSettingEnum Role = RoleSettingEnum.MediumQuality;

    [System.Serializable]
    public class OutlineSetting
    {
        [Range(0, 1)] public float OutLineWidth = 0.1f;

        [Range(0, 1)] public float CameraDisStrength = 0.2f;

        [Range(0, 1)] public float OutLineMin = 0.5f;

        [Range(1, 2)] public float OutLineMax = 2.0f;
    }

    public OutlineSetting Outline;
    /*
    [System.Serializable]
    public class ColorAdjustments 
    {
        //public bool usePostExposure = false;
        public float PostExposure;
        //public bool useContrast = false;
        [Range(-100, 100)] public float Contrast = 0;
        //public bool useColorFilter = false;
        public Color ColorFilter = Color.white;
        //public bool useHueShift = false;
        [Range(-100, 100)] public float HueShift = 0;
        //public bool useSaturation = false;
        [Range(-100, 100)] public float Saturation = 0;
    }
    */
   
    public ColorAdjustments colorAdjustments;

    
    [System.Serializable]    public class LUTSetting
    {
        public bool Open = false;
        public Texture2D LUTTexture;
        [Range(0, 1)] public float Strength = 1f;
    }
    
    public LUTSetting EnvLUT;

    public bool EnableGrabTexture = false;
    //public bool EnableSSRenderFeature = true;
/*
    private RenderTexture rtDepthTexture;
    private RenderTargetIdentifier rtDepth;

    public RenderTargetIdentifier GetDepthTexture(RenderTextureDescriptor descriptor)
    {
        if (rtDepthTexture == null)
        {
            descriptor.depthBufferBits = 0;
            descriptor.colorFormat = RenderTextureFormat.ARGB32;
            rtDepthTexture = RenderTexture.GetTemporary(descriptor);
            rtDepth = rtDepthTexture;
        }
        return rtDepth;
    }

    private void OnDisable()
    {       
        if (rtDepthTexture != null)
        {
            RenderTexture.ReleaseTemporary(rtDepthTexture);
            rtDepthTexture = null;
        }
    }
*/
    public void CopyData(CameraCustomData other)
    {
        Bloom = other.Bloom;
        Role = other.Role;
        EnableGrabTexture = other.EnableGrabTexture;
        Outline = other.Outline;
        //GreyWhite = other.GreyWhite;
        //Focus = other.Focus;
        BlackWhiteFlash = other.BlackWhiteFlash;
    }
}
