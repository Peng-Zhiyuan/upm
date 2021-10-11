using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class LocalizationExtension
{
    public static void ToLocalFont(this Text text)
    {
        if(text == null)
        {
            return;
        }
        if(text.font == TextFontLocalizer.DefaultFont)
        {
            return;
        }

        var language = LocalizationManager.Stuff.Language;


        if (language == "tw")
        {
            // 繁体情况下使用缺省字体
            text.font = TextFontLocalizer.DefaultFont;
        }
        else
        {
            // cn 与 en 情况下保持设计时的字体
        }
    }

    public static void Localize(this Text text, string key)
    {
        TextLocalizer monoTextLocalizer = text.gameObject.GetComponent<TextLocalizer>();
        if(!monoTextLocalizer){
            monoTextLocalizer = text.gameObject.AddComponent<TextLocalizer>();
        }
        monoTextLocalizer.key = key;
        monoTextLocalizer.RefreshIfNeed(true);
    }

    public static void Localize(this Text text, string key, params string[] argList)
    {        
        TextLocalizer monoTextLocalizer = text.gameObject.GetComponent<TextLocalizer>();
        if(!monoTextLocalizer){
            monoTextLocalizer = text.gameObject.AddComponent<TextLocalizer>();
        }
        monoTextLocalizer.key = key;
        monoTextLocalizer.parameters = argList;
        monoTextLocalizer.RefreshIfNeed(true);
    }

    public static void PrefabToLocalFont(GameObject go){
        for(int i = 0; i < go.transform.childCount; i++){
            var child = go.transform.GetChild(i).gameObject;
            PrefabToLocalFont(child);
        }

        var text = go.GetComponent<Text>();
        var localFont = go.GetComponent<TextFontLocalizer>();
        if(text && !localFont){
            go.AddComponent<TextFontLocalizer>();
        }
    }

    public static string Localize(this string key)
    {
        if(key == null)
        {
            return "";
        }
        else if(key == "")
        {
            return "";
        }
        var ret = LocalizationManager.Stuff.GetText(key);
        return ret;
    }

    public static string Localize(this string key, params string[] paramList)
    {
        if (key == null)
        {
            return "";
        }
        else if (key == "")
        {
            return "";
        }
        var ret = LocalizationManager.Stuff.GetText(key, paramList);
        return ret;
    }
}
