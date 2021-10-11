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
                language = "cn";
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


    public static Action<string> LanguageChanged;


    private static List<string> _languageList;
    public static List<string> LanguageList
    {
        get
        {
            if (_languageList == null)
            {
                _languageList = new List<string>();
                var str = LuncherManifestManager.Get("language.list");
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