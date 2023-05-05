using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class BoxProjection : MonoBehaviour
{
    public ReflectionProbe probe;
    public Material mat;
    void OnEnable()
    {
        if (this.mat == null)
        {
            var render = this.gameObject.GetComponent<Renderer>();
            var mats = render.GetMaterials();
            if (mats.Length > 0)
            {
                mat = mats[0];
            }
        }
        if (probe == null)
        {
            mat.SetVector("_SpecCube0_ProbePosition", Vector4.zero);
        }
        else
        {
            Vector3 center = probe.bounds.center;// this.transform.position + probe.center;

            Vector3 BoxMin = probe.bounds.min;
            Vector3 BoxMax = probe.bounds.max;
            /*
            mat.SetVector("unity_SpecCube0_ProbePosition", new Vector4(center.x, center.y, center.z, 1));
            mat.SetVector("unity_SpecCube0_BoxMin", new Vector4(BoxMin.x, BoxMin.y, BoxMin.z, 1));
            mat.SetVector("unity_SpecCube0_BoxMax", new Vector4(BoxMax.x, BoxMax.y, BoxMax.z, 1));
            */

            mat.SetVector("_SpecCube0_ProbePosition", new Vector4(center.x, center.y, center.z, 1));
            mat.SetVector("_SpecCube0_BoxMin", new Vector4(BoxMin.x, BoxMin.y, BoxMin.z, 1));
            mat.SetVector("_SpecCube0_BoxMax", new Vector4(BoxMax.x, BoxMax.y, BoxMax.z, 1));
            Debug.Log("center:" + center + "  BoxMin:" + BoxMin + "  BoxMax:" + BoxMax);
        }
        
       

        //Debug.Log("probe.bounds.center:" + probe.bounds.center);
    }
}
