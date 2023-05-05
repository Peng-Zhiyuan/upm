using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public static class HotSystemConfig
{
    // 从  $"{RemoteAddressableDirPath}/a.bytes" 加载程序集，不会缓存
    public static bool overrideAssemblyData;
}
