using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class PlatformUtil 
{
    //public static TargetPlatform TargetPlatform
    //{
    //    get
    //    {
    //        #if UNITY_ANDROID
    //        return TargetPlatform.Android;
    //        #elif UNITY_IOS
    //        return TargetPlatform.iOS;
    //        #else
    //        return TargetPlatform.Standalone;
    //        #endif
            
    //        // if(Application.platform == RuntimePlatform.Android)
    //        // {
    //        //     return "Android";
    //        // }
    //        // else if(Application.platform == RuntimePlatform.IPhonePlayer)
    //        // {
    //        //     return "IOS";
    //        // }
    //        // else if(Application.platform == RuntimePlatform.WebGLPlayer)
    //        // {
    //        //     return "WebGL";
    //        // }
    //        // else if(Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
    //        // {
    //        //     return "Editor";
    //        // }
    //        // throw new Exception("unsupport platform: " + Application.platform);
    //    }

    //}

    public static String RuntimePlatformName
    {
        get
        {
            if(Application.platform == RuntimePlatform.Android)
            {
                return "android";
            }
            else if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return "ios";
            }
            else if(Application.platform == RuntimePlatform.WebGLPlayer)
            {
                return "webGl";
            }
            else if(Application.isEditor)
            {
                return "editor";
            }
            throw new Exception("unsupport platform: " + Application.platform);
        }

    }
}
