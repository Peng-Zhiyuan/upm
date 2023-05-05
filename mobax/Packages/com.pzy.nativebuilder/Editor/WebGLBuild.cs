using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System;
using NativeBuilder;

public static class WebGLBuild
{   
    public static void Build()
    {
        NativeBuilderUtility.Build("NativeBuilderProduct/webGL", BuildTarget.WebGL, BuildOptions.None);
    }
}