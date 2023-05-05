using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class CaptureScreenComponent : MonoBehaviour
{
    [Button]
    void OnCaptureScreen()
    {
        CaptureScreen.QuickCapure();
    }
    [Button]
    void OnCaptureSquare()
    {
        CaptureScreen.QuickMapCapure();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            CaptureScreen.QuickCapure();
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            CaptureScreen.QuickMapCapure();
        }

    }
}
