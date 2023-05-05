using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CustomCoreComponentInspectorAttribute : Attribute
{
    public Type coreComponentType;
    public CustomCoreComponentInspectorAttribute(Type coreComponentType)
    {
        this.coreComponentType = coreComponentType;
    }
}
