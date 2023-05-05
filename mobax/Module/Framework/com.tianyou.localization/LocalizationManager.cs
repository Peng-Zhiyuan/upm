using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Sirenix.OdinInspector;
using System;
using Sirenix.OdinInspector;

public class LocalizationManager : StuffObject<LocalizationManager>
{

    [ShowInInspector, ReadOnly]
    string _language;

    public string Language
    {
        get
        {
            if(string.IsNullOrEmpty(_language))
            {
                return "";
            }
            return this._language;
        }
        set
        {
            this._language = value;
            Debug.Log("[LocalizationManager] localization language has been set to: " + value);
            LanguageChanged?.Invoke(this._language);
        }
    }

    public event Action<string> LanguageChanged;


    [ShowInInspector]
    public int CnWordCount
    {
        get
        {
            var dic = this.GetLanguageDic("cn");
            if(dic == null)
            {
                return 0;
            }
            return dic.Count;
        }
    }

    [ShowInInspector]
    public int EnWordCount
    {
        get
        {
            var dic = this.GetLanguageDic("en");
            if (dic == null)
            {
                return 0;
            }
            return dic.Count;
        }
    }

    [ShowInInspector]
    public int JpWordCount
    {
        get
        {
            var dic = this.GetLanguageDic("jp");
            if (dic == null)
            {
                return 0;
            }
            return dic.Count;
        }
    }



    private Dictionary<string, Dictionary<string, string>> languageToDicDic = new Dictionary<string, Dictionary<string, string>>();

    public void Clean()
    {
        languageToDicDic.Clear();
    }

    public void Add(string language, Dictionary<string, string> dic)
    {
        foreach(var kv in dic)
        {
            var key = kv.Key;
            var text = kv.Value;
            this.Add(language, key, text);
        }
    }

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


    private string CreateNotFoundResult(string key)
    {
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

    public bool HasText(string key)
    {
        key = key.Trim();
        var language = this.Language;
        var b = this.HasTextInLanguage(key, language);
        return b;
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
        key = key.Trim();
        var ret = this.GetTextAtCurrentLanguage(key);
        return ret;
    }

    public string GetText(string key, params object[] argList)
    {
        var msg = GetText(key);
        if(argList != null && argList.Length > 0)
        {
            var final = string.Format(msg, argList);
            return final;
        }
        else
        {
            return msg;
        }

    }



    //public void SetText(Text text, string key)
    //{
    //    text.text = GetText(key);
    //}

}
