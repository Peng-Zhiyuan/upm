using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DashboardSettings]
public class RemoteResDashboardSettings
{
    [DashboardGui(DashboardGuiType.Checkbox)]
    public static bool IsSimulationRemoteRes
    {
        get
        {
            var value = PlayerPrefsUtil.GetBool("RemoteResDashboardSettings.IsSimulationRemoteRes", false);
            return value;
        }
        set
        {
            PlayerPrefsUtil.SetBool("RemoteResDashboardSettings.IsSimulationRemoteRes", value);
        }
    }
}
