using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillMaker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var camerCtrl = FindObjectOfType<CustomCameraController>();
        if (camerCtrl != null)
        {
            var customCamera = camerCtrl.gameObject.AddComponent<CustomCameraCtrl>();
            customCamera.cameraCtrl = camerCtrl;
            camerCtrl.enabled = true;
        }
    }

}
