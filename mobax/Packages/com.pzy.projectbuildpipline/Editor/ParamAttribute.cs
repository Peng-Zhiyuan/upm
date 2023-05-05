using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ParamAttribute : Attribute
{
    public string key;
    public string defaultValue;
    public ParamAttribute(string key, string defaultValue)
    {
        this.key = key;
        this.defaultValue = defaultValue;
    }
}
