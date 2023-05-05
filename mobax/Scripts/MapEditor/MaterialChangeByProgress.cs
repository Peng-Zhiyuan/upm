using BattleSystem.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialChangeByProgress : MonoBehaviour
{

    Material Skybox;
    public AnimationCurve Curve;
    public Gradient FogGradient;
    
    void Start()
    {
        Material originalSkybox = RenderSettings.skybox;
        Material runtimeSkybox = Instantiate(originalSkybox);
        RenderSettings.skybox = runtimeSkybox;
        Skybox = runtimeSkybox;

    }

    void Update()
    {
        if (Skybox != null) {
            var car = SceneObjectManager.Instance.FindCreatureByConfigID(50000);
            if (car == null)
                return;
            var length = PathRoute.Ins.GetProgress(car.GetPosition ());
            var progress = length / PathRoute.Ins.GetPathLength();
            var v = Curve.Evaluate(progress);
            Skybox.SetFloat("_Exposure", v);

            var c = FogGradient.Evaluate(progress);
            RenderSettings.fogColor = c;
        }
    }
}
