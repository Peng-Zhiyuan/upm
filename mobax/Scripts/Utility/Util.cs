using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using System.Reflection;


// layer层级
public enum ELayerMask
{
    Default = 0,
    TransparentFX = 1,
    IgnoreRaycast = 2,
    Light = 3,
    Water = 4,
    UI = 5,
    Road = 6,
    Ground = 7,
    Role = 8,
    RoleShadowCaster = 9,
    CameraCheck = 10,
    TimeLineLight = 11,
    CameraOcc = 12,
    MiniCamera = 13,
}

public class LagacyUtil
{
    /// <summary>
    /// 设置指定root节点内所有节点(包括root根节点)的layer
    /// </summary>
    /// <param name="root"></param>
    /// <param name="layer"></param>
    public static void SetRootLayer(GameObject root, ELayerMask layer)
    {
        foreach (var trans in root.transform.GetComponentsInChildren<Transform>(true))
        {
            trans.gameObject.layer = (int) layer;
        }
    }

    public static int Random(int min, int max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    public static float Random(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    public static void SetRenderTexture(Transform cam, Transform image)
    {
        Camera oldcam = cam.GetComponent<Camera>();
        // 创建RenderTexture并绑定到摄像机上
        RenderTexture _renderTexture = new RenderTexture(1080, 1920, 8, RenderTextureFormat.Default);
        oldcam.targetTexture = _renderTexture;

        image.GetComponent<RawImage>().texture = _renderTexture;
    }

    public static void SetRenderTexture(Transform cam, Transform image, Vector2Int sizeDelta)
    {
        Camera oldcam = cam.GetComponent<Camera>();
        // 创建RenderTexture并绑定到摄像机上
        RenderTexture _renderTexture = new RenderTexture(sizeDelta.x, sizeDelta.y, 8, RenderTextureFormat.Default);
        oldcam.targetTexture = _renderTexture;

        image.GetComponent<RawImage>().texture = _renderTexture;
    }

    public static void PlayAnimation(GameObject go, string anim)
    {
        Animator animator = go.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.Play(anim, 0, 0);
            animator.Update(0);
        }
    }


    public static float GetClipLength(Animator animator, string clip)
    {
        if (null == animator || string.IsNullOrEmpty(clip) || null == animator.runtimeAnimatorController)
            return 0;
        RuntimeAnimatorController ac = animator.runtimeAnimatorController;
        AnimationClip[] tAnimationClips = ac.animationClips;
        if (null == tAnimationClips || tAnimationClips.Length <= 0) return 0;
        AnimationClip tAnimationClip;
        for (int tCounter = 0, tLen = tAnimationClips.Length; tCounter < tLen; tCounter++)
        {
            tAnimationClip = ac.animationClips[tCounter];
            if (null != tAnimationClip && tAnimationClip.name == clip)
                return tAnimationClip.length;
        }

        return 0F;
    }
}