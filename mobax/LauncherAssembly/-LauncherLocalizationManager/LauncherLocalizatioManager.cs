using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LauncherLocalizatioManager : LauncherStuffObject<LauncherLocalizatioManager>
{
    Dictionary<string, Dictionary<string, string>> languageToDicDic = new Dictionary<string, Dictionary<string, string>>();

    public static T ListTryGet<T>(List<T> list, int index, T @default = default)
    {
        if (list == null)
        {
            return @default;
        }
        if (index < 0 || index >= list.Count)
        {
            return @default;
        }
        return list[index];
    }


    public void ReloadData()
    {
        var csvString = Resources.Load<TextAsset>("localization");
        var lineList = CsvUtil.ParseCsv(csvString.text);
        var titleLine = lineList[0];
        for(int i = 1; i < titleLine.Count; i++)
        {
            var language = titleLine[i];
            if(string.IsNullOrEmpty(language))
            {
                continue;
            }
            for(int j = 1; j < lineList.Count; j++)
            {
                var line = lineList[j];
                if(line.Count == 0)
                {
                    continue;
                }
                var key = line[0];
                if(string.IsNullOrEmpty(key))
                {
                    continue;
                }
                var msg = ListTryGet(line, i, "");
                this.Add(language, key, msg);
            }
        }
    }

    public string Language
    {
        get
        {
            var ret = LanguageManager.Language;
            return ret;
        }
    }
    
    public static string Get(string key)
    {
        if(key == null)
        {
            return "";
        }
        key = key.Trim();
        var language = LauncherLocalizatioManager.Stuff.Language;
        var dic = LauncherLocalizatioManager.Stuff.GetLanguageDic(language);
        if(dic.TryGetValue(key, out var ret))
        {
            return ret;
        }
        else
        {
            return "";
        }
        
    }

    void Add(string language, string key, string text)
    {
        var dic = this.GetOrCreateLanguageDic(language);
        dic[key] = text;
    }

    Dictionary<string, string> GetOrCreateLanguageDic(string language)
    {
        var dic = GetLanguageDic(language);
        if (dic == null)
        {
            dic = CreateLanguageDic(language);
        }
        return dic;
    }

    Dictionary<string, string> GetLanguageDic(string language)
    {
        Dictionary<string, string> dic;
        languageToDicDic.TryGetValue(language, out dic);
        return dic;
    }

    Dictionary<string, string> CreateLanguageDic(string language)
    {
        var dic = new Dictionary<string, string>();
        languageToDicDic[language] = dic;
        return dic;
    }
}
