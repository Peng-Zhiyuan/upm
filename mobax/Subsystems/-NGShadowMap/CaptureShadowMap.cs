using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScRender;
using UnityEngine;
using MathDL;
using Sirenix.OdinInspector;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

//[ExecuteInEditMode]
public class CaptureShadowMap : MonoBehaviour
{
    private Light mLight;
    private bool isAdd = false;
    CommandBuffer cb;
    public RenderTextureFormat format = RenderTextureFormat.Shadowmap;
    public LightEvent LightEvent = LightEvent.AfterShadowMap;
    public RenderTexture m_ShadowmapCopy;
    private void OnDisable()
    {
        if (isAdd)
        {
            isAdd = false;
            mLight.RemoveCommandBuffer(LightEvent, cb);
        }
    }
    void OnEnable()
    {
        //ShadowMapUtil.CreateLightCamera();
        this.UpdateShadowMap();
    }

    private void UpdateShadowMap()
    {
        if (mLight == null)
        {
            mLight = GetComponent<Light>();

        }
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene == gameObject.scene && mLight != null)
        {
             AddShadowBuffer();
        }
    }
    /*
    //Update is called once per frame
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        //Display the shadowmap in the corner.
        Camera.main.rect = new Rect(0, 0, 0.5f, 0.5f);
        Graphics.Blit(m_ShadowmapCopy, dest);
        Camera.main.rect = new Rect(0, 0, 1, 1);
        //Shader.SetGlobalTexture("_ShadowMap", shadowMap);//buildin
    }
    */

    /*
    private Texture2D RenderTextureToTexture2D(RenderTexture texture)
    {
        //RenderTexture RT = RenderTexture.active;
        var old_rt = RenderTexture.active;
        RenderTexture.active = texture;
        Texture2D texture2D = new Texture2D(texture.width, texture.height,TextureFormat.ARGB32, false);
        texture2D.ReadPixels(new Rect(0, 0, texture2D.width, texture2D.height), 0, 0);
        RenderTexture.active = old_rt;
        return texture2D;
    }
    */
    //private int m_sourceID = Shader.PropertyToID("_CustomShadowMap");
    void AddShadowBuffer()
    {
        if (!isAdd)
        {
            Debug.Log("AddShadowBuffer");
            RenderTargetIdentifier shadowmap = BuiltinRenderTextureType.CurrentActive;
            m_ShadowmapCopy = new RenderTexture(Screen.width, Screen.height, 0);
            Texture2D texture2D = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
            m_ShadowmapCopy.format = RenderTextureFormat.RInt;
            cb = new CommandBuffer();
            cb.SetShadowSamplingMode(shadowmap, ShadowSamplingMode.RawDepth);
            cb.Blit(shadowmap, new RenderTargetIdentifier(m_ShadowmapCopy));
          
            //if (m_ShadowmapCopy != null)
            //{
            //    PictureFactory.Stuff.SaveTexture(RenderTextureToTexture2D(m_ShadowmapCopy));
            //}
            //Shader.SetGlobalTexture("_CustomShadowMap", m_ShadowmapCopy.toTexture2D());
            Shader.SetGlobalTexture("_CustomShadowMap", texture2D);
            //Shader.SetGlobalFloat("_CustomVal", 0.1f);
            mLight.AddCommandBuffer(LightEvent, cb);
            PictureFactory.Stuff.SaveTexture(m_ShadowmapCopy.toTexture2D());

            var globalTexture = Shader.GetGlobalTexture("_CustomShadowMap");
            if (globalTexture != null)
            {
                //PictureFactory.Stuff.SaveTexture(globalTexture as Texture2D);
                Debug.Log("globalTexture is exist");
            }
           
            //var val = Shader.GetGlobalFloat("_CustomVal");
           // Debug.Log("val:"+ val);
        
            isAdd = true;
        }
    }
}