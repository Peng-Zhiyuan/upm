using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DashboardGuiAttribute : Attribute
{
    public int guiType;
    public DashboardGuiAttribute(int guiType)
    {
        this.guiType = guiType;
    }

}
