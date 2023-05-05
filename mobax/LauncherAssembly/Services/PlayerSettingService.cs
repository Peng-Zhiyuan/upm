using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ServiceDescription("在启动设置屏幕为不自动熄屏")]
public class PlayerSettingService : Service
{
    public override void OnCreate()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        //Application.targetFrameRate = DeveloperLocalSettings.FPS;

        /*     float[] distances = new float[32];
             distances[LayerMask.NameToLayer("grass")] = 50;
             Camera.main.layerCullSpherical = true;
             Camera.main.layerCullDistances = distances;*/
        //var urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
        //urpAsset.renderScale = 0.9f;
        //Debug.LogError(" urpAsset.renderScale:" + urpAsset.renderScale);
        //int width = 1080;
        //Screen.SetResolution();
    }


}
