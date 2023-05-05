using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ServiceDescription("将保存的 fps 设置到运行时")]
public class FpsService : Service
{
    public override void OnCreate()
    {
        var settingFps = DeveloperLocalSettings.FPS;
        Application.targetFrameRate = settingFps;
    }
}
