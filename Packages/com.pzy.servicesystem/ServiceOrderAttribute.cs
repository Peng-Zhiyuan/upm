using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ServiceOrderAttribute : Attribute
{
    public int order;
    public ServiceOrderAttribute(int order)
    {
        this.order = order;
    }
}
