using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffectOffset : MonoBehaviour
{
    /*  private Transform effRoot;
        public Transform EffectRoot
        {
            get
            {
                if(effRoot == null)
                {
                    effRoot = this.transform;
                }
                return effRoot;
            }
        }*/

    [HideInInspector]
    public float offset = 1;

    public void OnEnable()
    {
        // this.transform.localPosition = Vector3.zero;
        // var selfPos = this.transform.position;
        // if (Camera.main == null) return;
        // var cameraPos = Camera.main.transform.position;
        // var dir = (cameraPos - selfPos).normalized;
        //
        // this.transform.position += dir * offset;
    }
}