using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class BaseLoader 
{
    public enum Schema
    {
        Unity2020
    }

    public static void Load(Schema schema, string baseAndroidProjectPath, string changeSourceAndroidProjectPath)
    {
        if(schema == Schema.Unity2020)
        {
            var chnageSourceUnityModule = $"{changeSourceAndroidProjectPath}/unityLibrary";
            var baseUnityModule = $"{baseAndroidProjectPath}/unityLibrary";
            //IOUtil.DeleteDirectoryIfExits(baseUnityModule);

            //var fromPath = new DirectoryInfo(chnageSourceUnityModule);
            //var toPath = new DirectoryInfo(baseUnityModule);
            //PShellUtil.CopyTo(fromPath, toPath);
            IOUtil.SyncDir(chnageSourceUnityModule, baseUnityModule, null, new string[] { "build" });
        }
    }
}
