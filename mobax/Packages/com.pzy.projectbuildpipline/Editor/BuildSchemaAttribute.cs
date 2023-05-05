using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

public class BuildSchemaAttribute : Attribute
{
    public string msg;
    public BuildTarget buildTarget;

    public BuildSchemaAttribute(BuildTarget buildTarget, string msg)
    {
        this.buildTarget = buildTarget;
        this.msg = msg;
    }

    
}
