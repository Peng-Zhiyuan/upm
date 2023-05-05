
using UnityEngine;
using UnityEngine.UI;

public static class LauncherGameTalk
{
    private static Text _txt;
    private static LauncherTypeWriter _printer;
    
    public static void Bind(Text txt)
    {
        _txt = txt;
        _printer = txt.GetComponent<LauncherTypeWriter>() ?? txt.gameObject.AddComponent<LauncherTypeWriter>();
    }
    
    public static void Goes(LauncherGameTalkType type)
    {
        _Say(GetWords(type));
    }

    public static string GetWords(LauncherGameTalkType type)
    {
        var words = "";
        if (LauncherGameData.TalkMap.TryGetValue((int) type, out var arr))
        {
            words = LauncherLocalizatioManager.Get(arr[Random.Range(0, arr.Length)]);
        }
        return words;
    }

    private static void _Say(string words)
    {
        _printer.StopAni();
        _txt.text = "";
        _printer.PlayAni(words, .005f, 1);
    }
}

public enum LauncherGameTalkType
{
    T1_Welcome = 1,
    T2_Matched,
    T3_UnMatched,
    T4_CustomerLeave
}