using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Sirenix.OdinInspector;

public class LocalizationManager : StuffObject<LocalizationManager>
{

    public string Language
    {
        get
        {
            var luncherLanguage = LanguageManager.Language;
            return luncherLanguage;
        }
    }


    private Dictionary<string, Dictionary<string, string>> languageToDicDic = new Dictionary<string, Dictionary<string, string>>();





    public void Add(string language, string key, string text)
    {
        var dic = this.GetOrCreateLanguageDic(language);
        dic[key] = text;
    }

    private Dictionary<string, string> GetOrCreateLanguageDic(string language)
    {
        var dic = GetLanguageDic(language);
        if (dic == null)
        {
            dic = CreateLanguageDic(language);
        }
        return dic;
    }

    private Dictionary<string, string> GetLanguageDic(string language)
    {
        Dictionary<string, string> dic;
        languageToDicDic.TryGetValue(language, out dic);
        return dic;
    }

    private Dictionary<string, string> CreateLanguageDic(string language)
    {
        var dic = new Dictionary<string, string>();
        languageToDicDic[language] = dic;
        return dic;
    }

    public string GetKeyByValueAtLanguage(string value, string language = "cn")
    {
        var dic = this.GetLanguageDic(language);
        if (dic == null)
        {
            return value;
        }
        string key1 = dic.FirstOrDefault(lan => lan.Value == value).Key;
        if (key1 == "")
        {
            return value;
        }
        return key1;
    }

    private string CreateNotFoundResult(string key)
    {
        //var ret = $"NotFound:{key}";
        //return ret;
        return key;
    }

    public string GetTextAtLanguage(string key, string language, string secondLanguage)
    {
        var has = HasTextInLanguage(key, language);
        if (has)
        {
            var dic = this.GetLanguageDic(language);
            var ret = dic[key];
            return ret;
        }

        if (!string.IsNullOrEmpty(secondLanguage))
        {
            var has2 = HasTextInLanguage(key, secondLanguage);
            if (has2)
            {
                var dic = this.GetLanguageDic(secondLanguage);
                var ret = dic[key];
                return ret;
            }
        }

        var notFound = CreateNotFoundResult(key);
        return notFound;
    }

    bool HasTextInLanguage(string key, string language)
    {
        var dic = this.GetLanguageDic(language);
        if (dic == null)
        {
            return false;
        }
        var contains = dic.ContainsKey(key);
        if (!contains)
        {
            return false;
        }
        var text = dic[key];
        if (text == "" || text == null)
        {
            return false;
        }
        return true;
    }

    private string GetTextAtCurrentLanguage(string key)
    {
        var language = this.Language;
        var ret = this.GetTextAtLanguage(key, language, "");
        return ret;
    }

    public string GetText(string key)
    {
        if (key == null)
        {
            return "";
        }
        if (key == "")
        {
            return "";
        }
        var ret = this.GetTextAtCurrentLanguage(key);
        return ret;
    }

    public string GetText(string key, params string[] argList)
    {
        var ret = GetTextWithFormat(key, argList);
        return ret;
    }


    string GetTextWithFormat(string key, params string[] argList)
    {
        var msg = GetText(key);
        var final = string.Format(msg, argList);
        return final;
    }


    public void SetText(Text text, string key)
    {
        text.text = GetText(key);
    }

}
