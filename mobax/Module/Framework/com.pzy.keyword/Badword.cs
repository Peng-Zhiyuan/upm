using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public static class Badword
{
    /// <summary>
    /// 向词库中增加关键字
    /// </summary>
    public static void Add(string word, MatchType matchType)
    {
        var libraryIndex = GetLibraryIndex(matchType);
        BadwordManager.Stuff.AddWorld(libraryIndex, word);
    }

    static int GetLibraryIndex(MatchType matchType)
    {
        if(matchType == MatchType.Fuzzy)
        {
            return 1;
        }
        else if(matchType == MatchType.HoleWord)
        {
            return 2;
        }
        else
        {
            throw new Exception("[Badword] not support type : " + matchType);
        }
    }

    /// <summary>
    /// 判断是包含任何关键字
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static bool HasAnyBadWord(string text)
    {
        var list = Match(text);
        if (list.Count == 0)
        {
            return false;
        }
        return true;
    }

    static List<MatchInfo> Match(string text)
    {
        var holeMatchLibrary = GetLibraryIndex(MatchType.HoleWord);
        var fuzzyMatchLibrary = GetLibraryIndex(MatchType.Fuzzy);
        var matchList1 = BadwordManager.Stuff.Match(holeMatchLibrary, text, MatchType.HoleWord);
        var matchList2 = BadwordManager.Stuff.Match(fuzzyMatchLibrary, text, MatchType.Fuzzy);
        var ret = new List<MatchInfo>();
        ret.AddRange(matchList1);
        ret.AddRange(matchList2);
        return ret;
    }

    /// <summary>
    /// 遮蔽其中的关键字
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string Replace(string text)
    {
        var matchList = Match(text);

        if (matchList.Count == 0)
        {
            return text;
        }
        var sb = new StringBuilder(text);
        foreach (var info in matchList)
        {
            var start = info.startIndex;
            var end = info.endIndex;
            for (int i = start; i <= end; i++)
            {
                sb[i] = '*';
            }
        }
        return sb.ToString();
    }
}
