using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ConsoleCommands]
public static class LocalizationCmds
{
    public static string ShowKeyLocalText_Help = "显示key在当前语言中的本地化字符";
    public static List<string> ShowKeyLocalText_Alias = new List<string>(){"local"};
    public static void ShowKeyLocalText(string key)
    {
        var lang = LocalizationManager.Stuff.Language;
        var result = key.Localize();
        MainConsoleWind.ShowStringToCmd($"{key} -> {result} ({lang})", Color.green);
    }
    
    public static string ShowGameLanguage_Help = "显示当前的游戏本地化语言";
    public static List<string> ShowGameLanguage_Alias = new List<string>(){"language", "语言"};
    public static void ShowGameLanguage()
    {
        var lang = LocalizationManager.Stuff.Language;
        MainConsoleWind.ShowStringToCmd(lang, Color.green);
    }

    public static string SetGameLanguage_Help = "设置当前的游戏本地化语言";
    public static List<string> SetGameLanguage_Alias = new List<string>(){"setlanguage", "设置语言"};
    public static void SetGameLanguage(string lang)
    {
        LanguageManager.Language = lang;
        MainConsoleWind.ShowStringToCmd($"Set language : {lang}", Color.green);
    }
}
