using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ConsoleCommands]
public static class LoggerCommands
{
    public static string ShowUnityConsole_Help = "显示Unity的控制台，可以查看游戏的Log信息";
    public static void ShowUnityConsole()
    {
        UIEngine.Stuff.ShowFloating<LoggerFloating>();
    }
}
