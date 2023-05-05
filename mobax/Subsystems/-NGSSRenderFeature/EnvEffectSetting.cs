
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScRender;
using UnityEngine;
using MathDL;
using Sirenix.OdinInspector;

//[ExecuteInEditMode]
public class EnvEffectSetting:StuffObject<EnvEffectSetting>
{

    public List<Material> skyBoxes = new List<Material>();
    public List<Material> lutMats = new List<Material>();
    public List<Material> fogMats = new List<Material>();
    public List<Color> shadowColorList = new List<Color>();
    public bool openWithLoad = true;
    private Color defaultShadowColor = new Color(0,0,0,0);
    public void OpenEffect()
    {
        //Debug.LogError("OpenEffect");
        SwitchFogMat(fogIndex);
        SwitchLutMat(lutIndex);
        SwitchSkyBox(skyBoxIndex);
        SwitchShadowColor(shadowColorIndex);
    }

    public void CloseEffect()
    {
       // Debug.LogError("CloseEffect");
        SetLutMat(null);
        SetFogMat(null);
        SetSkyBox(null);
        SetShadowColor(defaultShadowColor);
    }


    public void OnEnable()
    {
        if (!openWithLoad) return;
        this.OpenEffect();
    }

    public void OnDisable()
    {
        if (!openWithLoad) return;
        this.CloseEffect();
    }
    

    
    [SerializeField, SetProperty("ShadowColor")]
    private int shadowColorIndex = 0;
    public int ShadowColorIndex
    {
        get
        {
            return shadowColorIndex;
        }

        private set
        {
            this.SwitchShadowColor(value);
        }
    }


    [SerializeField, SetProperty("SkyBoxIndex")]
    private int skyBoxIndex = 0;
    public int SkyBoxIndex
    {
        get
        {
            return skyBoxIndex;
        }

        private set
        {
            this.SwitchSkyBox(value);
           
        }
    }



    [SerializeField, SetProperty("LutIndex")]
    private int lutIndex = 0;
    public int LutIndex
    {
        get
        {
            return lutIndex;
        }

        private set
        {
            this.SwitchLutMat(value);

        }
    }

    [SerializeField, SetProperty("FogIndex")]
    private int fogIndex = 0;
    public int FogIndex
    {
        get
        {
            return fogIndex;
        }

        private set
        {
            this.SwitchFogMat(value);
        }
    }


    public void SwitchSkyBox(int index)
    {
        if (skyBoxes.Count > index && index >= 0)
        {
            SetSkyBox(skyBoxes[index]);
        }
        else
        {
            SetSkyBox(null);
        }
    }

     public void SwitchShadowColor(int index)
    {
        if (shadowColorList.Count > index && index >= 0)
        {
            SetShadowColor(shadowColorList[index]);
        }
        else
        {
            SetShadowColor(defaultShadowColor);
        }
    }


    public void SwitchLutMat(int index)
    {
        if (lutMats.Count > index && index >= 0)
        {
            SetLutMat(lutMats[index]);
        }
        else
        {
            SetLutMat(null);
        }
    }

    public void SwitchFogMat(int index)
    {
        if (fogMats.Count > index && index >= 0)
        {
            SetFogMat(fogMats[index]);
        }
        else
        {
            SetFogMat(null);
        }
    }

    private void SetSkyBox(Material skyBoxMat)
    {
        RenderSettings.skybox = skyBoxMat;
    }

    private void SetShadowColor(Color c)
    {
        Debug.LogError("SetShadowColor:"+c);
        Shader.SetGlobalColor("_GlobalShadowColor", c);
    }


    private void SetFogMat(Material fogMat)
    {
        var feature = RenderFeatureHandler.Ins.GetRenderFeatureByName("DepthHeightFogRenderFeature");
        if (null != feature)
        {
            (feature as DepthHeightFogRenderFeature).settings.fogMaterial = fogMat;
            feature.SetActive(fogMat != null);
        }
    }

    private void SetLutMat(Material lutmat)
    {
        var camera = Camera.main;
        if (camera != null)
        {
            var customData = camera.GetComponent<CameraCustomData>();
            if (customData != null)
            {
                if (lutmat != null)
                {
                    customData.EnvLUT.Open = true;
                    customData.EnvLUT.LUTTexture = lutmat.mainTexture as Texture2D;
                }
                else
                {
                    customData.EnvLUT.Open = false;
                }

            }

        }
        //CameraCustomData customData = renderingData.cameraData.camera.transform.GetComponent<CameraCustomData>();
        //var feature = RenderFeatureHandler.Ins.GetRenderFeatureByName("RoleRenderFeature");
        //if (null != feature)
       // {
       //     Debug.Log("RoleRenderFeature");
        //    (feature as RoleRenderFeature).settings.lutMaterial.SetTexture("_LUT",lutmat.mainTexture);
            //feature.SetActive(enabled);
       // }
    }


}
