using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DashboardGuiAttribute : Attribute
{
    public DashboardGuiType guiType;
    public DashboardGuiAttribute(DashboardGuiType guiType)
    {
        this.guiType = guiType;
    }

}
