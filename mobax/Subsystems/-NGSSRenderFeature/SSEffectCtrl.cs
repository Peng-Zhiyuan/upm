using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
[ExecuteInEditMode]
public class SSEffectCtrl : MonoBehaviour
{
    public Camera targetCamera;
    private CameraCustomData customData;
    private Camera mainCamera = null;
    private int SCENE_LAYER;
    private int FOCUS_LAYER;
    public bool openBlur = false;
    public bool openSceneBlur = false;
    public bool openBlackWhiteFlash = false;
    [Range(0.0f, 1.0f)] public float flashCenterX = 0.5f;
    [Range(0.0f, 1.0f)] public float flashCenterY = 0.5f;
    public bool flash = false;
    public bool hideFog = false;

    [Range(0, 1)] public float fadeScene = 0;
    private void Awake()
    {
        SCENE_LAYER = LayerMask.NameToLayer("Default");
        FOCUS_LAYER = LayerMask.NameToLayer("Focus");
    }
    public bool hideSceneLayer = false;
    private int cameraCullingMask = 0;
    private void RefreshProperties()
    {
        if (customData != null)
        {
            customData.BlackWhiteFlash.openBlackWhiteFlash = openBlackWhiteFlash;

            customData.BlackWhiteFlash.flashCenterX = flashCenterX;
            customData.BlackWhiteFlash.flashCenterY = flashCenterY;

            customData.RadiaBlur.openBlur = openBlackWhiteFlash || openBlur;
            customData.RadiaBlur.openSceneBlur = openSceneBlur;
            customData.RadiaBlur.radiaBlurCenterX = flashCenterX;
            customData.RadiaBlur.radiaBlurCenterY = flashCenterY;

            customData.BlackWhiteFlash.flash = flash;

            // customData.Focus.showFocusLayer = showFocusLayer;
            // customData.Focus.fadeBlack = fadeBlack;

            // customData.colorAdjustments.openColorAdjustments = fadeScene > 0;
            customData.colorAdjustments.fadeScene = fadeScene;
            customData.DepthHeightFog.openFog = !hideFog;

        }

        if (mainCamera != null)
        {
            int sceneLayer = 1 << SCENE_LAYER;
            if (hideSceneLayer)
            {
                if (mainCamera.clearFlags == CameraClearFlags.SolidColor) return;
                mainCamera.cullingMask &= ~sceneLayer;
                mainCamera.backgroundColor = Color.black;
                mainCamera.clearFlags = CameraClearFlags.SolidColor;

            }
            else
            {
                if (mainCamera.clearFlags == CameraClearFlags.Skybox) return;
                mainCamera.cullingMask |= sceneLayer;
                mainCamera.clearFlags = CameraClearFlags.Skybox;
            }

            if (openBlackWhiteFlash)
            {
                int focusLayer = 1 << FOCUS_LAYER;
                if (mainCamera.cullingMask != focusLayer)
                {
                    cameraCullingMask = mainCamera.cullingMask;
                }
                mainCamera.cullingMask = focusLayer;
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
            }
            else
            {
                int focusLayer = 1 << FOCUS_LAYER;
                if (cameraCullingMask > 0 && mainCamera.cullingMask == focusLayer)
                {
                    mainCamera.cullingMask = cameraCullingMask;
                    mainCamera.clearFlags = CameraClearFlags.Skybox;
                }
            }

        }
    }
    public void OnDidApplyAnimationProperties()
    {
        Debug.Log("RefreshProperties");
        RefreshProperties();
    }
    public void OnValidate()
    {
        Debug.Log("OnValidate");
        RefreshProperties();
    }

    [ShowInInspector]
    public void ResetProperties()
    {
        if (customData != null)
        {
            customData.BlackWhiteFlash.openBlackWhiteFlash = false;
            customData.RadiaBlur.openBlur = false;
            //customData.Focus.showFocusLayer = false;
            customData.BlackWhiteFlash.flashCenterX = 0.5f;
            customData.BlackWhiteFlash.flashCenterX = 0.5f;
            customData.BlackWhiteFlash.flashCenterY = 0.5f;

            customData.RadiaBlur.radiaBlurCenterX = 0.5f;
            customData.RadiaBlur.radiaBlurCenterY = 0.5f;
            //customData.Focus.fadeBlack = 0;
            customData.BlackWhiteFlash.flash = flash;
            customData = null;
        }
        if (mainCamera != null)
        {
            mainCamera = null;
        }
    }

    void OnEnable()
    {
        if (targetCamera != null)
        {
            mainCamera = targetCamera;
        }
        else if (CameraManager.IsAccessable)
        {
            mainCamera = CameraManager.Instance.MainCamera;
        }
        else 
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            return;
        }
        customData = mainCamera.GetComponent<CameraCustomData>();
    }

    void OnDisable()
    {
        ResetProperties();
    }

        /*
        public CameraCustomData customData;
        void OnEnable()
        {
            GameObject gameObject = GameObject.Find("Main Camera");
            var mainCamera = gameObject.GetComponent<Camera>();
            var originCustomData = mainCamera.GetComponent<CameraCustomData>();
            if (originCustomData.target == null)
            {
                originCustomData.target = customData;
            }

        }

        void OnDisable()
        {
            GameObject gameObject = GameObject.Find("Main Camera");
            var mainCamera = gameObject.GetComponent<Camera>();
            var originCustomData = mainCamera.GetComponent<CameraCustomData>();
            if (originCustomData.target == customData)
            {
                originCustomData.target = null;
            }
        }
        */


    }
