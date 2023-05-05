using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
public static class RenderExtend
{
    public static bool editMode = false;
    public static Material[] GetMaterials(this Renderer render)
    {

#if UNITY_EDITOR
        if ( !Application.isPlaying || editMode)
        {
           return render.sharedMaterials;
        }
        else
        {
            return render.materials;
        }
#else
        return render.sharedMaterials;
#endif
    }
    public static void SetMaterials(this Renderer render, Material[] mats)
    {
     
#if UNITY_EDITOR
        if (!Application.isPlaying || editMode)
        {
            render.sharedMaterials = mats;
        }
        else
        {
            render.materials = mats;
        }
#else
         render.sharedMaterials = mats;
#endif
    }
}