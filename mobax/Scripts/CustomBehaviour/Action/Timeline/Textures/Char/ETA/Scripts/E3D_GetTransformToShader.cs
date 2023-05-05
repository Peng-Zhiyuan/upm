using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[HelpURL("https://www.element3ds.com/thread-216743-1-1.html")]
#if UNITY_EDITOR 
[ExecuteInEditMode]
#endif
public class E3D_GetTransformToShader : MonoBehaviour
{
    [HideInInspector]
    public List<E3DGetTransformToShaderInfo> infoLists = new List<E3DGetTransformToShaderInfo>();

    [HideInInspector]
    public Renderer[] childRenderers;
    [HideInInspector]
    public Renderer selfRender;

    [HideInInspector]
    public List<Material> selfMaterialsList = new List<Material>();
    [HideInInspector]
    public List<Material> materialsList = new List<Material>();

    [HideInInspector]
    public List<Material> runSelfMaterialsList = new List<Material>();
    [HideInInspector]
    public List<Material> runMaterialsList = new List<Material>();
    void Start()
    {
        if (Application.isEditor && Application.isPlaying == false)
        {
            GetNoRunMaterialsList();
        }
        else
        {
            GetRunMaterialsList();
        }
    }

    void Update()
    {
        TransformInfoChange(Time.deltaTime);
    }

    public void TransformInfoChange(float deltaTime)
    {
        foreach (var one in infoLists)
        {
            if (one == null || one.gameObject == null) continue;
            Vector3 ret = GetTranformTypeInfo(one.gameObject, one.transformType);
            if (Application.isEditor && Application.isPlaying == false)
            {
                if (one.applyChild)
                {
                    foreach (var item in materialsList)
                    {
                        if (item.HasProperty(one.propertyName))
                        {
                            item.SetVector(one.propertyName, ret);
                        }
                    }
                }
                else
                {
                    foreach (var item in selfMaterialsList)
                    {
                        if (item.HasProperty(one.propertyName))
                        {
                            item.SetVector(one.propertyName, ret);
                        }
                    }
                }

            }
            else
            {
                if (one.applyChild)
                {
                    foreach (var item in runMaterialsList)
                    {
                        if (item.HasProperty(one.propertyName))
                        {
                            item.SetVector(one.propertyName, ret);
                        }
                    }
                }
                else
                {
                    foreach (var item in runSelfMaterialsList)
                    {
                        if (item.HasProperty(one.propertyName))
                        {
                            item.SetVector(one.propertyName, ret);
                        }
                    }
                }
            }
        }
    }


    public Vector3 GetTranformTypeInfo(GameObject obj, E3DGetTransformType type)
    {
        Vector3 retValue = Vector3.zero;
        if (obj)
        {
            switch (type)
            {
                case E3DGetTransformType.Position:
                    retValue = obj.transform.position;
                    break;
                case E3DGetTransformType.Rotation:
                    retValue = obj.transform.eulerAngles;
                    break;
                case E3DGetTransformType.Scale:
                    retValue = obj.transform.localScale;
                    break;
                default:
                    break;
            }
        }
        return retValue;
    }

    public void GetNoRunMaterialsList()
    {
        selfMaterialsList.Clear();
        materialsList.Clear();

        childRenderers = transform.GetComponentsInChildren<Renderer>();
        selfRender = GetComponent<Renderer>();
        if (selfRender)
        {
            Material[] selfMats = selfRender.sharedMaterials;
            if (selfMats.Length > 0)
            {
                for (int i = 0; i < selfMats.Length; i++)
                {
                    Material mat = selfMats[i];
                    if (mat)
                    {
                        selfMaterialsList.Add(mat);
                        materialsList.Add(mat);
                    }
                }
            }
        }
        if (childRenderers.Length > 0)
        {
            for (int i = 0; i < childRenderers.Length; i++)
            {
                //=================================================================================//
                int matLength = childRenderers[i].sharedMaterials.Length;
                if (matLength > 0)
                {
                    for (int j = 0; j < matLength; j++)
                    {
                        Material mat = childRenderers[i].sharedMaterials[j];
                        if (mat)
                        {
                            materialsList.Add(mat);
                        }
                    }
                }
                //=================================================================================//
            }
        }

    }

    public void GetRunMaterialsList()
    {
        runSelfMaterialsList.Clear();
        runMaterialsList.Clear();

        childRenderers = transform.GetComponentsInChildren<Renderer>();
        selfRender = GetComponent<Renderer>();

        if (selfRender)
        {
            Material[] selfMats = selfRender.materials;
            if (selfMats.Length > 0)
            {
                for (int i = 0; i < selfMats.Length; i++)
                {
                    Material mat = selfMats[i];
                    if (mat)
                    {
                        runSelfMaterialsList.Add(mat);
                        runMaterialsList.Add(mat);
                    }
                }
            }
        }
        if (childRenderers.Length > 0)
        {
            for (int i = 0; i < childRenderers.Length; i++)
            {
                //=================================================================================//
                int matLength = childRenderers[i].materials.Length;
                if (matLength > 0)
                {
                    for (int j = 0; j < matLength; j++)
                    {
                        Material mat = childRenderers[i].materials[j];
                        if (mat)
                        {
                            runMaterialsList.Add(mat);
                        }
                    }
                }
                //=================================================================================//
            }
        }

    }

}


