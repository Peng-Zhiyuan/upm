using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class WwiseUtil
{
    //public static void InitDevicesDetector()
    //{
    //   /* if (DeveloperLocalSettings.IsUseWwise)
    //    {
    //        try
    //        {
    //            AudioSettings.OnAudioConfigurationChanged += AudioSettings_OnAudioConfigurationChanged;
    //            WwiseUtil.SendEventByCurrentOutputDevice();
    //        }
    //        catch(Exception e)
    //        {
    //            Debug.LogError("[WwiseUtil] device detector not init due to: " + e.Message);
    //        }
    //    }*/
    //}

    //private static void AudioSettings_OnAudioConfigurationChanged(bool deviceWasChanged)
    //{
    //    if (deviceWasChanged)
    //    {
    //        WwiseUtil.SendEventByCurrentOutputDevice();
    //    }
    //}

    //public static void SendEventByCurrentOutputDevice()
    //{
    //    var config = AudioSettings.GetConfiguration();
    //    if (config.speakerMode == AudioSpeakerMode.Stereo)
    //    {
    //        WwiseEventManager.SendEvent(TransformTable.UiControls, "PhoneSpeaker");
    //    }
    //    else
    //    {
    //        WwiseEventManager.SendEvent(TransformTable.UiControls, "Headphone");
    //    }
    //}
}
