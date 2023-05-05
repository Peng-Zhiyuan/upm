using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CrossPlatform 
{
    public static CrossPlatformType platform = CrossPlatformType.Unity;

    public static TextAsset LoadTextAsset(string path)
    {
        if(platform == CrossPlatformType.Unity)
        {
            var asset = Resources.Load(path, typeof(TextAsset)) as TextAsset;
            return asset;
        }
        else
        {
            throw new Exception("[CrossPlatform] not suuport yet");
        }
    }
}
