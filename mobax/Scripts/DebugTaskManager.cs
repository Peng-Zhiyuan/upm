using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugTaskManager 
{
    public static void Add(string key, string des)
    {
        if(DeveloperLocalSettings.IsDevelopmentMode)
        {
            var f = UIEngine.Stuff.ShowFloatingImediatly<TaskFloating>(null, UILayer.ScreenLayer);
            f.Add(key, des);
        }
    }

    public static void Remove(string key)
    {
        if (DeveloperLocalSettings.IsDevelopmentMode)
        {
            var f = UIEngine.Stuff.FindFloating<TaskFloating>();
            if (f != null)
            {
                f.Remove(key);
            }
        }
    }
}
