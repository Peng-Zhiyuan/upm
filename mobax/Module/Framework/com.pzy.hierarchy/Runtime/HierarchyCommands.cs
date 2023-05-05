using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ConsoleCommands]
public class HierarchyCommands
{
    public static string ShowHierarchy_Help = "显示Unity的Hierarchy";
    public static void ShowHierarchy()
    {
        UIEngine.Stuff.RemoveFloating<ConsoleFloating>();
        UIEngine.Stuff.ShowFloating<HierarchyFloating>();
    }
}
