using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ServiceDescriptionAttribute : Attribute
{
    public string des;

    public ServiceDescriptionAttribute(string msg)
    {
        this.des = msg;
    }
}
