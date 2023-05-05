using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public static class AudioManager 
{

    /// <summary>
    /// 在指定桶中寻找 clip 作为 bgm 播放，如果找不到则加载，如果找不到资源则不播放
    /// </summary>
    /// <param name="address"></param>
    /// <param name="bucketName"></param>
    public static async void PlayBgmInBackground(string address, string bucketName = "Main")
    {
       
        address = TryFixAddressExtension(address);
        var bucket = BucketManager.Stuff.GetBucket(bucketName);
        var clip = await bucket.GetOrAquireAsync<AudioClip>(address, true);

        AudioEngine.Stuff.StopGroup(AudioType.Bgm);
        if (clip != null)
        {
            AudioEngine.Stuff.Play(clip, 1f, AudioEngine.Parameter.SingleTaskEarly, AudioType.Bgm);
        }
        Debug.Log("[AudioManager] PlayBgmInBackground: " + address);
        
    }

    public static string TryFixAddressExtension(string address)
    {
        if (string.IsNullOrEmpty(address))
        {
            return "";
        }
        var ext = Path.GetExtension(address);
        if(string.IsNullOrEmpty(ext))
        {
            if (!address.EndsWith(".wav"))
            {
                var ret = Path.ChangeExtension(address, ".wav");
                //Debug.Log($"[AudioManager] {address} is not a valid clip address, use '{ret}' instead.");
                return ret;
            }
        }
        return address;
    }

    [Obsolete("此方法会在加载后播放音效，这会错过时机，更好的做法时预先加载到桶中，再使用同步方法 PlaySe() 播放")]
    public static async void PlaySeInBackground(string address, string bucketName = "Main",float volume = 1.0f, AudioEngine.Parameter param = AudioEngine.Parameter.SingleTaskLater)
    {
        
        if (string.IsNullOrEmpty(address))
        {
            return;
        } 


        address = TryFixAddressExtension(address);
        var bucket = BucketManager.Stuff.GetBucket(bucketName);
        var clip = await bucket.GetOrAquireAsync<AudioClip>(address, true);
        if (clip != null)
        {
            AudioEngine.Stuff.Play(clip, volume, param, AudioType.Se);
        }
        else
        {
            Debug.LogError($"[AudioManager] {address} is not find");
        }

        
    }

    public static void StopSe()
    {
        AudioEngine.Stuff.StopGroup(AudioType.Se);
    }

    /// <summary>
    /// 会在指定的桶中寻找 clip 进行播放，如果没找到则不播放
    /// </summary>
    /// <param name="address"></param>
    /// <param name="bucketName"></param>
    public static void PlaySe(string address, string bucketName = "Main", AudioEngine.Parameter param = AudioEngine.Parameter.SingleTaskLater)
    {
        address = TryFixAddressExtension(address);
        var bucket = BucketManager.Stuff.GetBucket(bucketName);
        var clip = bucket.Get(address, true) as AudioClip;
        if (clip != null)
        {
            AudioEngine.Stuff.Play(clip, 1f, param, AudioType.Se);
        }
        Debug.Log("[AudioManager] playSe: " + address);
    }

    public static void ResetVolume()
    {
        AudioEngine.Stuff.DeleteCacheForGroup(AudioType.Bgm);
        AudioEngine.Stuff.DeleteCacheForGroup(AudioType.Se);
    }

    public static string CurrentBgmName
    {
        get
        {
            return AudioEngine.Stuff.GetCurrentBGMName();
        }
    }

    public static float BgmVolume
    {
        get
        {
            return AudioEngine.Stuff.GetGroupVolume(AudioType.Bgm);
        }
        set
        {
            AudioEngine.Stuff.SetGroupVolume(AudioType.Bgm, value);
        }
    }

    public static float SeVolume
    {
        get
        {
            return AudioEngine.Stuff.GetGroupVolume(AudioType.Se);
        }
        set
        {
            AudioEngine.Stuff.SetGroupVolume(AudioType.Se, value);
        }
    }

    public static void PauseBgm()
    {
        AudioEngine.Stuff.PauseGroup(AudioType.Bgm);
    }

    public static void ResumeBgm()
    {
        AudioEngine.Stuff.ResumeGroup(AudioType.Bgm);
    }
}

public class AudioType
{
    public static string Se = "se";
    public static string Bgm = "bgm";

}