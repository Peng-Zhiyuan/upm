using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SdkMailInfo 
{
    public SdkMailInfoForOneLanguage chs;
    public SdkMailInfoForOneLanguage cht;
    public SdkMailInfoForOneLanguage eng;
    public SdkMailInfoForOneLanguage jpn;

    public SdkMailInfoForOneLanguage GetOneLanguageInfo(string gameLangaugeCode)
    {
        if(gameLangaugeCode == "cn")
        {
            return this.chs;
        }
        else if(gameLangaugeCode == "tw")
        {
            return this.cht;
        }
        else if(gameLangaugeCode == "jp")
        {
            return this.jpn;
        }
        else if(gameLangaugeCode == "en")
        {
            return this.eng;
        }
        throw new Exception("[SdkMailInfo] unsupport language code: " + gameLangaugeCode);
    }

    public string GetTitle(string gameLanguageCode)
    {
        var info = this.GetOneLanguageInfo(gameLanguageCode);
        if(info == null)
        {
            return "";
        }
        return info.title;
    }

    public string GetContent(string gameLanguageCode)
    {
        var info = this.GetOneLanguageInfo(gameLanguageCode);
        if(info == null)
        {
            return "";
        }
        return info.content;
    }
}

public class SdkMailInfoForOneLanguage
{
    public string title;
    public string content;
}
