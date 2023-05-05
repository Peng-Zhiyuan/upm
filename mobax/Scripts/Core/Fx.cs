using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fx : MonoBehaviour
{
    private string fxName = "";

    void Awake()
    {
        fxName = this.gameObject.name.Split('(')[0].Replace("$", "");
    }

    private void OnEnable()
    {
        if (DeveloperLocalSettings.IsUseWwise)
        {
            //WwiseEventManager.OnEffectStart(fxName);
            WwiseEventManager.SendEvent(TransformTable.EffectStart, fxName);
        }
    }

    private void OnDisable()
    {
        if (DeveloperLocalSettings.IsUseWwise)
        {
            //WwiseEventManager.OnEffectEnd(fxName);
            WwiseEventManager.SendEvent(TransformTable.EffectEnd, fxName);
        }
    }
}