using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Threading.Tasks;

public static class LocalizationUtil 
{
    /// <summary>
    /// 按照 LanguageManager.Language 选择的语言，重置本地化系统
    /// </summary>
    /// <returns></returns>
    public static async Task ProcessAsync()
    {
        // 加载语言
        Debug.Log("[HotRoot] load language data");
        await LanguageDataRuntime.ReloadAsync();

        // 初始化本地化系统
        Debug.Log("[HotRoot] init localization");
       
        LoadLocalizationFormLanguageData();
        LocalizationManager.Stuff.Language = LanguageUtil.ProcessedLangauge;

    }

    static void LoadLocalizationFormLanguageData()
    {
        LocalizationManager.Stuff.Clean();

        var language = LanguageUtil.ProcessedLangauge;
        var type = typeof(LanguageData);
        var proerptyList = type.GetProperties(BindingFlags.Public | BindingFlags.Static);
        foreach (var p in proerptyList)
        {
            var name = p.Name;
            if (name.EndsWith("Table"))
            {
                var wraper = p.GetValue(null);
                var tt = wraper.GetType();
                var field = tt.GetField("table");
                var table = field.GetValue(wraper);
                var ttt = table.GetType();
                var pp = ttt.GetProperty("Dics");
                var dic = pp.GetValue(table);
                var languageDic = dic as Dictionary<string, string>;
                //Debug.Log("languageTable:" + name);
                if(languageDic == null)
                {
                    var e = new GameException(ExceptionFlag.None, $"localization table {name} error", "LANGUAGE_TABLE_ERROR");
                    throw e;
                }
                LocalizationManager.Stuff.Add(language, languageDic);
            }
        }
    }

}
