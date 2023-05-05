using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonSetting 
{
    public static void SetResolution()
    {
        Debug.LogError("w:"+Screen.currentResolution.width +" h:"+ Screen.currentResolution.height);
        //int width = Mathf.FloorToInt(1080f * Screen.currentResolution.width / Screen.currentResolution.height);
        //Screen.SetResolution(width, 1080, true, 30);
    }
}
