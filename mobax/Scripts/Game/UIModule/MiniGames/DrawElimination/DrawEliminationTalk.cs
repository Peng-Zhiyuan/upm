
using UnityEngine;
using UnityEngine.UI;

public static class DrawEliminationTalk
{
    private static Text _txt;
    private static TypeWriter _printer;
    
    public static void Bind(Text txt)
    {
        _txt = txt;
        _printer = txt.GetComponent<TypeWriter>() ?? txt.gameObject.AddComponent<TypeWriter>();
    }
    
    public static void Goes(DrawEliminationTalkType type)
    {
        _Say(GetWords(type));
    }

    public static string GetWords(DrawEliminationTalkType type)
    {
        var arrInfo = StaticData.SgameTrashTable.TryGet((int) type);
        var words = arrInfo.Colls[Random.Range(0, arrInfo.Colls.Count)].Des.Localize(Database.Stuff.roleDatabase.Me.name);
        return words;
    }

    private static void _Say(string words)
    {
        _printer.StopAni();
        _txt.text = "";
        _printer.PlayAni(words, .005f, 1);
    }
}

public enum DrawEliminationTalkType
{
    T1_Welcome = 1,
    T2_Matched,
    T3_UnMatched,
    T4_CustomerLeave
}