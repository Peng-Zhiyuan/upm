using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class NGDistort : MonoBehaviour
{
    void OnEnable()
    {
        NGGlobalSettings.GrabObjectsCount++;
    }
    void OnDisable()
    {
        NGGlobalSettings.GrabObjectsCount--;
    }
}
