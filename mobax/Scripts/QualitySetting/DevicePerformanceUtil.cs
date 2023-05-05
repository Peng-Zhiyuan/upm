using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DevicePerformanceUtil
{
    public static DevicePerformanceLevel GetDevicePerformanceLevel()
    {
        //Debug.Log("SystemInfo.operatingSystemFamily:" + SystemInfo.operatingSystemFamily);
        Debug.Log("SystemInfo.operatingSystem:" + SystemInfo.operatingSystem);
        Debug.Log("SystemInfo.systemMemorySize:" + SystemInfo.systemMemorySize);

        Debug.Log("SystemInfo.processorType:" + SystemInfo.processorType);
        Debug.Log("SystemInfo.processorFrequency:" + SystemInfo.processorFrequency);
        Debug.Log("SystemInfo.processorCount:" + SystemInfo.processorCount);
        
        //Debug.Log("SystemInfo.graphicsDeviceType:" + SystemInfo.graphicsDeviceType);
        Debug.Log("SystemInfo.graphicsDeviceName:" + SystemInfo.graphicsDeviceName);
        Debug.Log("SystemInfo.graphicsDeviceVersion:" + SystemInfo.graphicsDeviceVersion);
        Debug.Log("SystemInfo.graphicsMemorySize:" + SystemInfo.graphicsMemorySize);
        var deiveName = SystemInfo.graphicsDeviceName;
        if (StaticData.DeviceInfoTable.ContainsKey(deiveName))
        {
            DeviceInfoRow info = StaticData.DeviceInfoTable[deiveName];
            Debug.LogError("deiveName:"+ deiveName + " quality:" + info.Quality);
            return (DevicePerformanceLevel)(info.Quality + 1);
        }
        Debug.LogError("deiveName:" + deiveName + "default quality:" + DevicePerformanceLevel.Medium);
        return DevicePerformanceLevel.Hight;

        /*
        if (SystemInfo.graphicsDeviceVendorID == 32902)
        {
            return DevicePerformanceLevel.Low;
        }
        else 
        {
            
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (SystemInfo.processorCount <= 2)
#elif UNITY_STANDALONE_OSX || UNITY_IPHONE
            if (SystemInfo.processorCount < 2)
#elif UNITY_ANDROID
            if (SystemInfo.processorCount <= 4)
#endif
            {

                return DevicePerformanceLevel.Low;
            }
            else
            {

                int graphicsMemorySize = SystemInfo.graphicsMemorySize;
                int systemMemorySize = SystemInfo.systemMemorySize;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                if (graphicsMemorySize >= 4000 && systemMemorySize >= 8000)
                    return DevicePerformanceLevel.Hight;
                else if (graphicsMemorySize >= 2000 && systemMemorySize >= 4000)
                    return DevicePerformanceLevel.Medium;
                else
                    return DevicePerformanceLevel.Low;
#elif UNITY_STANDALONE_OSX || UNITY_IPHONE
            if (graphicsMemorySize >= 4000 && systemMemorySize >= 8000)
                    return DevicePerformanceLevel.Hight;
                else if (graphicsMemorySize >= 2000 && systemMemorySize >= 4000)
                    return DevicePerformanceLevel.Medium;
                else
                    return DevicePerformanceLevel.Low;
#elif UNITY_ANDROID
            if (graphicsMemorySize >= 6000 && systemMemorySize >= 8000)
                    return DevicePerformanceLevel.Hight;
                else if (graphicsMemorySize >= 2000 && systemMemorySize >= 4000)
                    return DevicePerformanceLevel.Medium;
                else
                    return DevicePerformanceLevel.Low;
#endif
            }
        }
        */
    }
}
public enum DevicePerformanceLevel
{
    Low = 1,
    Medium = 2,
    Hight = 3
}