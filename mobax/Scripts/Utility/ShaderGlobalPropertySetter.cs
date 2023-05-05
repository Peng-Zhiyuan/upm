using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderGlobalPropertySetter : MonoBehaviour
{
    public string PropertyName = "";
    [ColorUsage(true, true)]
    public Color EmissionColor;

    private void Start()
    {
        Refresh();
    }
    public void Refresh ()
    {
        Shader.SetGlobalColor(PropertyName, EmissionColor);
    }
}
