using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class PostProcessHandler : MonoInstance<PostProcessHandler>
{
    RenderObjects FilterFeature;
    Blit ColorBlendBlitFeature;
    public Blit BlitFeature
    {
        get {
            if (ColorBlendBlitFeature == null)
                ColorBlendBlitFeature = RenderFeatureHandler.Ins.GetRenderFeatureByName("ColorBlendBlit") as Blit;
            return ColorBlendBlitFeature;
        }
    }

    Dictionary<GameObject, int> objLayerMap = new Dictionary<GameObject, int>();

    public void PlayFadeInColor (float duration, float stay, Color fadeinColor, List<GameObject> frontObjs)
    {
        if (FilterFeature == null)
            FilterFeature = RenderFeatureHandler.Ins.GetRenderFeatureByName("PostProcessingStencil") as RenderObjects;
        //FilterFeature.SetActive(true);
        BlitFeature.SetActive(true);

        var frontLayer = LayerMask.NameToLayer("AfterScreenFade");
        RestoreObjLayer();
        // reset layers
        if (frontObjs != null) {
            foreach (var go in frontObjs) {
                objLayerMap.Add(go, go.layer);
                var children = go.GetComponentsInChildren<Transform>();
                foreach (var child in children)
                    child.gameObject.layer = frontLayer;
            }
        }

        // set properties
        var coroutine = FadeCoroutine(duration, stay, fadeinColor);
        StartCoroutine (coroutine);
    }
    public void PlayFadeOutColor(float duration, float stay, Color fadeinColor, List<GameObject> frontObjs)
    {
        if (FilterFeature == null)
            FilterFeature = RenderFeatureHandler.Ins.GetRenderFeatureByName("PostProcessingStencil") as RenderObjects;
       
        //FilterFeature.SetActive(true);
        BlitFeature.SetActive(true);

        var frontLayer = LayerMask.NameToLayer("AfterScreenFade");
        RestoreObjLayer();
        // reset layers
        if (frontObjs != null) {
            foreach (var go in frontObjs) {
                objLayerMap.Add(go, go.layer);
                var children = go.GetComponentsInChildren<Transform>();
                foreach (var child in children)
                    child.gameObject.layer = frontLayer;
            }
        }

        // set properties
        var coroutine = FadeOutCoroutine(duration, stay, fadeinColor);
        StartCoroutine(coroutine);
    }

    bool IsInFadeing = false;
    float FadeinElapsed = 0;
    IEnumerator FadeCoroutine (float duration, float stay, Color color) 
    {
        if (IsInFadeing)
            yield break;
        IsInFadeing = true;
        FadeinElapsed = 0;

        var endAlpha = color.a;
        do {
            var progress = FadeinElapsed / duration;
            color.a = EasingFunc (progress) * endAlpha;
            BlitFeature.settings.blitMaterial.SetColor("_BlendColor", color);
            RenderFeatureHandler.Ins.RenderData.SetDirty();

            yield return null;
            FadeinElapsed += Time.deltaTime;
        } while (FadeinElapsed < duration);
        color.a = endAlpha;
        BlitFeature.settings.blitMaterial.SetColor("_BlendColor", color);
        RenderFeatureHandler.Ins.RenderData.SetDirty();

        do {
            yield return null;
            FadeinElapsed += Time.deltaTime;
        } while (FadeinElapsed < duration + stay);
        StartCoroutine (BlackScreen(0.3f));
        //FilterFeature.SetActive(false);

        // restore layers
        RestoreObjLayer();

        IsInFadeing = false;
    }
    float BlackElapsed = 0;
    IEnumerator BlackScreen (float duration)
    {
        BlackElapsed = 0;
        BlitFeature.settings.blitMaterial.SetColor("_BlendColor", Color.black);
        RenderFeatureHandler.Ins.RenderData.SetDirty();

        do {
            yield return null;
            BlackElapsed += Time.deltaTime;
        } while (BlackElapsed < duration);
    }
    
    bool IsInFadeingOut = false;
    IEnumerator FadeOutCoroutine(float duration, float stay, Color color)
    {
        if (IsInFadeingOut)
            yield break;
        IsInFadeingOut = true;
        FadeinElapsed = 0;

        BlitFeature.settings.blitMaterial.SetColor("_BlendColor", color);
        RenderFeatureHandler.Ins.RenderData.SetDirty();
        do {
            yield return null;
            FadeinElapsed += Time.deltaTime;
        } while (FadeinElapsed < stay);

        var beginAlpha = color.a;
        do {
            var progress = 1 - (FadeinElapsed - stay) / duration;
            color.a = EasingFunc(progress) * beginAlpha;
            BlitFeature.settings.blitMaterial.SetColor("_BlendColor", color);
            RenderFeatureHandler.Ins.RenderData.SetDirty();

            yield return null;
            FadeinElapsed += Time.deltaTime;
        } while (FadeinElapsed < duration + stay);
        color.a = 0;
        BlitFeature.settings.blitMaterial.SetColor("_BlendColor", color);
        RenderFeatureHandler.Ins.RenderData.SetDirty();

        

        //FilterFeature.SetActive(false);
        BlitFeature.SetActive(false);
        RenderFeatureHandler.Ins.RenderData.SetDirty();
        // restore layers
        RestoreObjLayer();

        IsInFadeingOut = false;
    }
    void RestoreObjLayer ()
    {
        foreach (var go in objLayerMap.Keys) {
            var children = go.GetComponentsInChildren<Transform>();
            foreach (var child in children)
                child.gameObject.layer = objLayerMap[go];
        }
        objLayerMap.Clear();
    }

    float EasingFunc (float x)
    {
        return Mathf.Pow(x, 4);
    }

    public static void SetPostEffectActive<T>(GameObject go, bool active) where T: VolumeComponent
    {
        var volume = go.GetComponent<Volume>();
        T volumeComp;
        var hasComp = volume.profile.TryGet<T>(out volumeComp);
        if (hasComp)
            volumeComp.active = active;
    }
    public static T GetPostEffect<T>(GameObject go) where T : VolumeComponent
    {
        var volume = go.GetComponent<Volume>();
        T volumeComp;
        var hasComp = volume.profile.TryGet<T>(out volumeComp);
        if (hasComp)
            return volumeComp;
        return null;
    }
}
