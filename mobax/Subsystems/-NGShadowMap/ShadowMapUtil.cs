using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class ExtensionMethod
{
    public static Texture2D toTexture2D(this RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBA32, false);
        var old_rt = RenderTexture.active;
        RenderTexture.active = rTex;

        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();

        RenderTexture.active = old_rt;
        return tex;
    }
}

public class ShadowMapUtil 
{

    public static Camera CreateLightCamera()
    {
        GameObject goLightCamera = new GameObject("Shadow Camera");
        Camera LightCamera = goLightCamera.AddComponent<Camera>();
        LightCamera.backgroundColor = Color.black;
        LightCamera.clearFlags = CameraClearFlags.SolidColor;
        LightCamera.orthographic = true;
        LightCamera.orthographicSize = 6f;
        LightCamera.nearClipPlane = 0.3f;
        LightCamera.farClipPlane = 50;
        LightCamera.enabled = false;

        if (!LightCamera.targetTexture)
            LightCamera.targetTexture = CreateTextureFor(LightCamera);
        var lightDepthTexture = LightCamera.targetTexture;

        Shader.SetGlobalTexture("_LightDepthTexture", lightDepthTexture);
        PictureFactory.Stuff.SaveTexture(lightDepthTexture.toTexture2D());
        return LightCamera;
    }

    private static RenderTexture CreateTextureFor(Camera cam)
    {
        //   RenderTexture rt = new RenderTexture(Screen.width * qulity, Screen.height * qulity, 24, RenderTextureFormat.Default);
        RenderTexture rt = new RenderTexture(1024, 1024, 16, RenderTextureFormat.Default);
        rt.hideFlags = HideFlags.DontSave;

        return rt;
    }
}
