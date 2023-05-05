using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RemoteConf
{
    public int staticDataVersion;

    public bool isOpen;

    public string closeMsg;
    public string discord;

    public Dictionary<string, int> languageDataVersion;

    public int GetLanguageDataVersion(string language)
    {
        var b = languageDataVersion.TryGetValue(language, out var version);
        if(!b)
        {
            throw new Exception("[RemoteConf] language data version for: " + language + " not found");
        }
        return version;
    }
}
