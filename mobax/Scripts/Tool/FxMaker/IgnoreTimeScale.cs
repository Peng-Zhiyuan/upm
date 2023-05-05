using System;
using UnityEngine;

public class IgnoreTimeScale : MonoBehaviour
{
        private void Start()
        {
                ParticleSystem[] particles = this.GetComponentsInChildren<ParticleSystem>();
                foreach (var VARIABLE in particles)
                {
                    var mainModule = VARIABLE.main;
                    mainModule.useUnscaledTime = true;
                }
        }
}