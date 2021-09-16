using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Reflection;
using System;

public class GameConsoleCmds
{
    // 给这个类中所有方法增加tags
    public static List<string> GameConsoleCmds_Tags = new List<string>(){"base", "console"};

    public static string Help_Help = "显示所有指令的帮助信息";
    public static List<string> Help_Alias = new List<string>(){"帮助", "妈！", "help"};

    public static void Help(){
        MainConsoleWind.ShowHelp();
    }
    
    public static string HelpHelp_Help = "显示所有指令的帮助信息（含别名）";
    public static List<string> HelpHelp_Alias = new List<string>(){"helphelp", "爸！"};
    public static bool HelpHelp_Hide = true;

    public static void HelpHelp(){
        MainConsoleWind.ShowHelp(true);
    }

    public static string HelpTag_Help = "显示带有某个标签的指令帮助信息";
    public static List<string> HelpTag_Alias = new List<string>(){"标签", "老妈！", "helptag"};

    public static void HelpTag(string tag){
        MainConsoleWind.ShowTagHelp(tag);
    }

    public static string HelpHelpTag_Help = "显示带有某个标签的指令帮助信息（含别名）";
    public static List<string> HelpHelpTag_Alias = new List<string>(){"老爸！", "helphelptag"};

    public static bool HelpHelpTag_Hide = true;
    public static void HelpHelpTag(string tag){
        MainConsoleWind.ShowTagHelp(tag, true);
    }

    public static List<string> HelpCmd_Alias = new List<string>(){"指令帮助", "这是啥", "我是谁"};
    public static string HelpCmd_Help = "显示指令的帮助信息";
    public static void HelpCmd(string funcName){
        var type = MainConsoleWind.GetCmdMethod(funcName);
        if(type != null){
            MainConsoleWind.ShowHelp(funcName, true);
        }
        else{
            var names = MainConsoleWind.GetHelpableMethods(true);
            var guess = MainConsoleWind.GetSimilarityStringList(funcName, names)[0];
            MainConsoleWind.NextGuessCmd = $"HelpCmd {guess}";
            MainConsoleWind.ShowStringToCmd($"Command [{funcName}] not found. Are you looking for [{guess}] ?", Color.red);
        }
    }

    public static List<string> ClearConsole_Alias = new List<string>(){"砸瓦鲁多", "clear"};
    public static string ClearConsole_Help = "清除控制台输出";
    public static void ClearConsole(){
        var floating = UIEngine.Stuff.FindFloating<GameDashboardFloating>();
        floating.mainConsoleWind.ClearConsole();
    }

    public static string ShowGameLog_Help = "控制台是否显示游戏Log（切换）";
    public static void ShowGameLog(){
        MainConsoleWind.ShowGameLog = !MainConsoleWind.ShowGameLog;
        MainConsoleWind.ShowStringToCmd($" -ShowGameLog : {MainConsoleWind.ShowGameLog}", Color.white);
    }

    public static string ShowGameLogStack_Help = "控制台是否显示游戏Log堆栈信息（切换）";

    public static void ShowGameLogStack(){
        MainConsoleWind.showLogStack = !MainConsoleWind.showLogStack;
        MainConsoleWind.ShowStringToCmd($" -ShowGameLogStack : {MainConsoleWind.showLogStack}", Color.white);
    }

    public static string ShowHierarchy_Help = "显示Unity的Hierarchy";

    public static void ShowHierarchy()
    {
        UIEngine.Stuff.RemoveFloating<GameDashboardFloating>();
        UIEngine.Stuff.ShowFloating<HierarchyFloating>();
    }

    public static string ShowUnityConsole_Help = "显示Unity的控制台，可以查看游戏的Log信息";
    public static void ShowUnityConsole()
    {
        UIEngine.Stuff.ShowFloating<GameUnityConsoleFloating>();
    }

    public static string HalfMode_Help = "半屏模式切换";
    public static List<string> HalfMode_Alias = new List<string>(){"half", "半屏", "半藏"};
    public static void HalfMode(){
        var floating = UIEngine.Stuff.FindFloating<GameDashboardFloating>();
        floating.HalfMode(!floating.halfMode);
        MainConsoleWind.ShowStringToCmd($" -HalfMode : {floating.halfMode}", Color.white);
    }

    public static string ShowConsoleError_Help = "显示最近发生的错误栈";

    public static void ShowConsoleError(){
        MainConsoleWind.ShowConsoleError();
    }

    //public static string Forward_Help = "显示Unity的控制台，可以查看游戏的Log信息";
    public static void Forward(string pageName)
    {
        UIEngine.Stuff.Forward(pageName);
    }

    public static void Back()
    {
        UIEngine.Stuff.Back();
    }
}
