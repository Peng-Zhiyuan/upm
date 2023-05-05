using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;
public class QualityDetect : StuffObject<QualityDetect>
{
    private bool needDetectQuality = true;
    private Action<int> onTrack = null;
    public void Init(Action<int> onTrack)
    {
        //pipeline = ((UniversalRenderPipelineAsset)QualitySettings.renderPipeline);
        this.onTrack = onTrack;
        needDetectQuality = true;
    }

    private int detectCount = 0;
    private float fps = 0;
    private float total_fps = 0;
    private void DetectFrame()
    {
        if (needDetectQuality)
        {
            fps = 1.0f / Time.smoothDeltaTime;
            //if (!Battle.Instance.BattleStarted) return;
            detectCount++;
            total_fps += fps;
            if (detectCount > 20)
            {
                float average_fps = total_fps / detectCount;
                if (average_fps < 20)
                {
                    if (this.onTrack != null)
                    {
                        this.onTrack((int)average_fps);
                    }
                    needDetectQuality = false;
                    QualitySetting.SetQuality(DevicePerformanceLevel.Low);

                    Debug.LogError("frame < 20:" + DevicePerformanceLevel.Low);
                }
                else if (average_fps < 25)
                {
                    if (this.onTrack != null)
                    {
                        this.onTrack((int)average_fps);
                    }
                    int q = (int)DeveloperLocalSettings.GraphicQuality;
                    if (q > 1)
                    {
                        DevicePerformanceLevel qualityLevel = (DevicePerformanceLevel)(--q);
                        QualitySetting.SetQuality(qualityLevel);
                        Debug.LogError("frame < 25:" + qualityLevel);
                    }
                }
                else
                {
                    Debug.LogError("frame >= 30!");
                }
                needDetectQuality = false;
                DeveloperLocalSettings.QualityDetected = true;
                Destroy(this.gameObject);
            }
        }
        else 
        {
            Destroy(this.gameObject);
        }
    }

    float lastUpdateTime;
    const float INTERVAL = 0.1f;
    private void Update()
    {
        var now = Time.time;
        var delta = now - lastUpdateTime;
        if (delta >= INTERVAL)
        {
            lastUpdateTime = now;
            this.DetectFrame();
        }
    }
}
