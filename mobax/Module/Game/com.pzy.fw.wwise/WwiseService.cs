using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public class WwiseService : StuffObject<WwiseService>
{
    public void OnCreate()
    {
//        if (DeveloperLocalSettings.IsUseWwise)
//        {
//            //AudioSettings.OnAudioConfigurationChanged += AudioSettings_OnAudioConfigurationChanged;
//#if BuildUpdate
//            Debug.Log("BuildUpdate!!!");
//            Type audioSettingsType = typeof(AudioSettings);
//            EventInfo audioConfigChangedEvent = audioSettingsType.GetEvent("OnAudioConfigurationChanged", BindingFlags.Static | BindingFlags.Public);
//            if (audioConfigChangedEvent != null && audioConfigChangedEvent.EventHandlerType != null)
//            {
//                MethodInfo audioConfigChangedHandler = typeof(WwiseService).GetMethod("AudioSettings_OnAudioConfigurationChanged", BindingFlags.Instance | BindingFlags.NonPublic);
//                Delegate audioConfigChangedDelegate = Delegate.CreateDelegate(audioConfigChangedEvent.EventHandlerType, this, audioConfigChangedHandler);
//                audioConfigChangedEvent.AddEventHandler(null, audioConfigChangedDelegate);
//            }
//#else
//            Debug.Log("BuildFull!!!");
//            AudioSettings.OnAudioConfigurationChanged += AudioSettings_OnAudioConfigurationChanged;
//#endif
//            WwiseUtil.SendEventByCurrentOutputDevice();

//        }
    }

    private void AudioSettings_OnAudioConfigurationChanged(bool deviceWasChanged)
    {
        if (deviceWasChanged)
        {
            //WwiseUtil.SendEventByCurrentOutputDevice();
        }
    }
}
