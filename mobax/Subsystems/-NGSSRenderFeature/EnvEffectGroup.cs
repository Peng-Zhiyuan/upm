using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScRender;
using UnityEngine;
using MathDL;
using Sirenix.OdinInspector;
using UnityEditor;

/*
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
*/

[Serializable]
public class EnvEffect
{
    public Material skyBox;
    public Material fogMat;
    public Texture2D lutTex;
    [Range(0, 1)]
    public float LUTStrength = 1;
    public Color shadowColor = defaultColor;
    private static Color defaultColor = new Color(1, 1, 1, 0);
    public float roleLightAtten = 1;
    public bool roleLightFreeDir = false;
    public Color roleLightColor = defaultColor;
    public float roleLightRotateY = 0;

    public float roleAdditionLightAtten = 1;
    public float roleAdditionLightMax = 1;
    //public Vector4 roleLightDir;

    public Color roleShadowColor = defaultColor;
    public Color roleSecondShadowColor = defaultColor;
    //public bool useSunPos = true;

    public Vector4 sunPos = new Vector4(11, 6, -60, 0); // Vector4.zero;
    public Color sunColor = defaultColor;
    private CameraCustomData customData;
    public Cubemap reflectCubeMap;

    public ColorAdjustments colorAdjustMent;

    public void SetSkyBox(Material skyBoxMat)
    {
        RenderSettings.skybox = skyBoxMat;
    }

    public void SetShadowColor(Color c)
    {
        Shader.SetGlobalColor("_GlobalShadowColor", c);
    }

    public void SetRoleShadowColor(Color c)
    {
        Shader.SetGlobalColor("_GRoleShadowColor", c);
    }

    public void SetRoleSecondShadowColor(Color c)
    {
        Shader.SetGlobalColor("_GRoleSecondShadowColor", c);
    }

    public void SetRoleLightAtten(float atten)
    {
        Shader.SetGlobalFloat("_GRoleLightAtten", atten);
    }

    public void SetRoleLightFreeDir(bool free)
    {
        Shader.SetGlobalFloat("_GRoleLightFreeDir", free ? 1 : 0);
    }

    public void SetSunPos(Vector4 pos, Color c)
    {
        Shader.SetGlobalVector("_GlobalSunPos", sunPos);
        Shader.SetGlobalColor("_GlobalSunColor", c);
        /*
        int lightLayer =  1 << LayerMask.NameToLayer("Default");
        var mainLights = Light.GetLights(LightType.Directional, lightLayer);
        if (mainLights.Length == 0) return;
        Shader.SetGlobalVector("_GlobalSunPos", mainLights[0].GetPosition());
        //Debug.LogError("mainLights[0].GetPosition():"+ mainLights[0].GetPosition());
        */
    }

    public void SetColorAdjustment(ColorAdjustments lightDir)
    {
        if (customData == null) return;
        //Debug.LogError("SetColorAdjustment");
        customData.colorAdjustments.Contrast = this.colorAdjustMent.Contrast;
        customData.colorAdjustments.openColorAdjustments = this.colorAdjustMent.openColorAdjustments;
        customData.colorAdjustments.HueShift = this.colorAdjustMent.HueShift;
        customData.colorAdjustments.PostExposure = this.colorAdjustMent.PostExposure;
        customData.colorAdjustments.ColorFilter = this.colorAdjustMent.ColorFilter;
        customData.colorAdjustments.Saturation = this.colorAdjustMent.Saturation;
        customData.colorAdjustments.fadeScene = this.colorAdjustMent.fadeScene;
    }

    public void SetRoleLightDir(Vector4 lightDir)
    {
        Shader.SetGlobalVector("_GRoleLightDir", lightDir);
    }

    public void SetRoleAdditionLightAtten(float atten)
    {
        Shader.SetGlobalFloat("_GRoleAdditionLightAtten", atten);
    }

    public void SetRoleAdditionLightMax(float maxAtten)
    {
        Shader.SetGlobalFloat("_GRoleAdditionLightMax", maxAtten);
    }

    public Camera GetMainCamera()
    {
        if (CameraManager.IsAccessable)
        {
            return CameraManager.Instance.MainCamera;
        }
        var camera = Camera.main;
        return camera;
    }

    public void SetRoleLightColor(Color lightColor)
    {
        Shader.SetGlobalVector("_GRoleLightColor", lightColor);
    }

    public void SetFogMat(Material fogMat)
    {
        var feature = RenderFeatureHandler.Ins.GetRenderFeatureByName("NGSceneEffectRenderFeature");
        if (null != feature)
        {
            (feature as NGSceneEffectRenderFeature).settings.fogMaterial = fogMat;
        }
    }

    public void SetLutMat(Texture2D lut2d, float strength = 1)
    {
        if (customData == null) return;
        if (lut2d != null)
        {
            customData.EnvLUT.Open = true;
            customData.EnvLUT.LUTTexture = lut2d;
            customData.EnvLUT.Strength = strength;
        }
        else
        {
            customData.EnvLUT.Open = false;
        }
    }

    public void OpenEffect(CameraCustomData cameraCustomData = null)
    {
        Debug.LogWarning("OpenEffect");
        customData = cameraCustomData;
        SetLutMat(lutTex, LUTStrength);
        SetFogMat(fogMat);
        SetSkyBox(skyBox);
        SetShadowColor(shadowColor);
        SetRoleLightAtten(roleLightAtten);
        SetRoleShadowColor(roleShadowColor);
        SetRoleSecondShadowColor(roleSecondShadowColor);
        Vector4 roleLightDir = Quaternion.Euler(0, roleLightRotateY * 30, 0) * Vector3.right;
        roleLightDir.w = roleLightRotateY > 0 ? 1 : 0;
        SetRoleLightDir(roleLightDir);
        SetRoleLightColor(roleLightColor);
        SetRoleLightFreeDir(roleLightFreeDir);
        SetSunPos(sunPos, sunColor);
        SetColorAdjustment(this.colorAdjustMent);
        RenderSettings.fog = false;
        if (reflectCubeMap != null)
        {
            RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;
            RenderSettings.customReflection = reflectCubeMap;
        }
        DynamicGI.UpdateEnvironment();
    }

    public void CloseEffect()
    {
        //Debug.LogError("CloseEffect");
        SetLutMat(null);
        SetFogMat(null);
        SetSkyBox(null);
        SetShadowColor(defaultColor);
        SetRoleLightAtten(1);
        SetRoleShadowColor(defaultColor);
        SetRoleSecondShadowColor(defaultColor);
        SetRoleLightDir(Vector4.zero);
        SetRoleLightColor(defaultColor);
        SetRoleLightFreeDir(false);
        /*      Camera camera = Camera.main;
                if (camera == null) return;
                var customData = camera.GetComponent<CameraCustomData>();*/
        if (customData != null)
        {
            customData.colorAdjustments.openColorAdjustments = false;
            customData = null;
        }
    }
}

public class EnvEffectGroup : StuffObject<EnvEffectGroup>
{
    public int curEffectIndex = 0;
    public List<EnvEffect> effects;

    private GameObject SelfGameObject;

    private void Awake()
    {
        SelfGameObject = this.gameObject;
    }

    public void OpenEffect(Camera camera = null)
    {
        Camera mainCamera = camera;
        if (mainCamera == null)
        {
            GameObject cameraObject = GameObject.FindWithTag("MainCamera");
            if (cameraObject != null)
            {
                mainCamera = cameraObject.GetComponent<Camera>();
            }
        }
        if (curEffectIndex < 0
            || curEffectIndex >= effects.Count)
        {
            throw new Exception("Maybe not find effect group setting!");
        }
        if (mainCamera != null)
        {
            var cameraCustomDtata = mainCamera.GetComponent<CameraCustomData>();
            effects[curEffectIndex].OpenEffect(cameraCustomDtata);
        }
        else
        {
            Debug.LogError("MainCamera is not find!");
        }
    }

    public void CloseEffect()
    {
        if (curEffectIndex < 0
            || curEffectIndex >= effects.Count) return;
        effects[curEffectIndex].CloseEffect();
    }

    public void OnDestroy()
    {
        this.CloseEffect();
    }

    public EnvEffect CurrentEffect
    {
        get { return effects[curEffectIndex]; }
    }

    void OnValidate()
    {
        if (SelfGameObject == null)
        {
            return;
        }
        //Debug.Log("OnValidate");
        OpenEffect();
    }
}