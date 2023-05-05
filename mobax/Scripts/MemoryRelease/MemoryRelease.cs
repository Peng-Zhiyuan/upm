using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MemoryRelease : StuffObject<MemoryRelease>
{
    public void Init()
    {
        Application.lowMemory += OnLowMemory;
    }

    private void OnLowMemory()
    {
        Debug.LogError("LowMemory detect!!!");
        Resources.UnloadUnusedAssets();
        GC.Collect();
        
    }
}
