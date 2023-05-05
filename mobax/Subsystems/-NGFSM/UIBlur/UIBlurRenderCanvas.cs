using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class UIBlurRenderCanvas : MonoBehaviour
{
    UniversalAdditionalCameraData _CameraData;
    private void Awake()
    {
        _CameraData = GetComponent<Canvas>().worldCamera.GetComponent<UniversalAdditionalCameraData>();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnEnable()
    {
        if (_CameraData == null)
            _CameraData = GetComponent<Canvas>().worldCamera.GetComponent<UniversalAdditionalCameraData>();
        _CameraData.SetRenderer(2);
    }

    private void OnDisable()
    {
        if (_CameraData == null)
            _CameraData = GetComponent<Canvas>().worldCamera.GetComponent<UniversalAdditionalCameraData>();
        _CameraData.SetRenderer(1);
    }
}
