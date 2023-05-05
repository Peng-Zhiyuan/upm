using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
public class SkeletonGraphicUnit : MonoBehaviour
{
    private SkeletonGraphic skeletonGraphic;
    public bool shareMaterial = false;
    public bool isGrey = false;
    private void Awake()
    {
        if (shareMaterial) return;
        skeletonGraphic = GetComponent<SkeletonGraphic>();
        this.skeletonGraphic.material = new Material(this.skeletonGraphic.material);
        
        SetGrey(isGrey);
    }

    public void SetGrey(bool grey)
    {
        isGrey = grey;
        if(this.skeletonGraphic == null)
            return;
        
        if (grey)
        {
            this.skeletonGraphic.material.SetFloat("_GrayPhase", 1);
        }
        else
        {
            this.skeletonGraphic.material.SetFloat("_GrayPhase", 0);
        }
    }
}
