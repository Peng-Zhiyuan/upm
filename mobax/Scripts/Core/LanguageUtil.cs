using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LanguageUtil 
{
    public static string LanguageCodeToDisplayName(string code)
    {
        var name = StaticData.LanguageTable.TryGet(code);
        if (name != null)
        {
            return name;
        }
        else
        {
            return $"({code})";
        }
    }

    static List<string> _processedLanguageList;
    public static List<string> ProcessedLanguageList
    {
        get
        {
            if (_processedLanguageList == null)
            {
                var list = new List<string>();
                var nativeList = LanguageManager.LanguageList;
                list.AddRange(nativeList);

                var jpEnabled = LanguageUtil.IsJpEnabled;
                if (!jpEnabled)
                {
                    list.Remove("jp");
                }
                _processedLanguageList = list;
            }
            return _processedLanguageList;

        }
    }

    public static string ProcessedLangauge
    {
        get
        {
            var nativeLanguage = LanguageManager.Language;

            var jpEnabled = LanguageUtil.IsJpEnabled;
            if (!jpEnabled)
            {
                if(nativeLanguage == "jp")
                {
                    nativeLanguage = "en";
                }
            }
            return nativeLanguage;
        }
    }

    static List<string> _languageOptionDisplayNameList;
    static public List<string> LanguageOptionDisplayNameList
    {
        get
        {
            if (_languageOptionDisplayNameList == null)
            {
                var ret = new List<string>();
                var list = ProcessedLanguageList;
                foreach (var code in list)
                {
                    var name = LanguageUtil.LanguageCodeToDisplayName(code);
                    ret.Add(name);
                }
                _languageOptionDisplayNameList = ret;
            }
            return _languageOptionDisplayNameList;
        }
    }

    public static int LanguageByIndex
    {
        get
        {
            var lan = LanguageManager.Language;
            var list = LanguageManager.LanguageList;
            var index = list.IndexOf(lan);
            return index;
        }
        set
        {
            var list = LanguageManager.LanguageList;
            var lan = list[value];
            LanguageManager.Language = lan;
        }
    }

    public static bool IsJpEnabled
    {
        get
        {
            var v = StaticData.BaseTable.TryGet("language_jp", 1);
            if(v == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
