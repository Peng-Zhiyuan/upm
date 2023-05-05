using System;
using UnityEngine;
using System.Collections.Generic;

public static class LanguageManager
{
    public static string Language
    {
        get
        {
            var language = PlayerPrefs.GetString("language", "");
            if (language == "" || !IsLanguageValid(language))
            {
                language = Language_Default;
                if(!IsLanguageValid(language))
                {
                    throw new Exception($"default language {language} is invalid, forget to config in luncher-manifest.json?");
                }
            }
            return language;
        }
        set
        {
            var valid = IsLanguageValid(value);
            if (!valid)
            {
                throw new Exception($"language {value} is invalid, forget to config in luncher-manifest.json?");
            }
            PlayerPrefs.SetString("language", value);
            Debug.Log("[LucnherLanguageManager] language has been set to: " + value);
            LanguageChanged?.Invoke(value);
        }
    }

    public static string Language_Default
    {
        get
        {
            var systemLanguage = Application.systemLanguage;
            switch (systemLanguage)
            {
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                    return "cn";
                case SystemLanguage.English:
                    return "en";
                case SystemLanguage.Japanese:
                    return "jp";
                case SystemLanguage.ChineseTraditional:
                    return "tw";
                default:
                    return "en";
            }
        }
    }


    public static void DeleteCache()
    {
        PlayerPrefs.DeleteKey("language");
    }


    public static Action<string> LanguageChanged;


    private static List<string> _languageList;
    public static List<string> LanguageList
    {
        get
        {
            if (_languageList == null)
            {
                _languageList = new List<string>();
                var str = GameManifestManager.Get("language.list");
                Debug.Log("language.list: " + str);
                var parts = str.Split(',');
                foreach (var entrace in parts)
                {
                    var trimed = entrace.Trim();
                    _languageList.Add(trimed);
                }
            }
            return _languageList;
        }
    }

    public static bool IsLanguageValid(string language)
    {
        return LanguageList.Contains(language);
    }
}