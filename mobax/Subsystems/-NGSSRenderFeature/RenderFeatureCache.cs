using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderFeatureCache
{
    public static void Release()
    {
        cacheCameraData.Clear();
    }

    private static Dictionary<Camera, CameraCustomData> cacheCameraData = new Dictionary<Camera, CameraCustomData>();
    public static CameraCustomData GetCustomData(Camera camera)
    {

        CameraCustomData data;
        if (cacheCameraData.TryGetValue(camera, out data))
        {
            if (data != null) return data;
        }
        CameraCustomData customData = camera.transform.GetComponent<CameraCustomData>();
        cacheCameraData[camera] = customData;
        return customData;
    }

}
