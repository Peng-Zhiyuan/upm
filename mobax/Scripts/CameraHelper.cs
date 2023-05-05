using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class CameraHelper : MonoBehaviour
{
    public float Distance;
    public float OffsetY;
    
    [ShowInInspector]
    public void ResetCameraDistance()
    {
        CameraSetting.Ins.SetDistance(Distance);
    }
    
    [ShowInInspector]
    public void ResetCameraY()
    {
        CameraSetting.Ins.SetOffsetY(OffsetY);
    }
}
