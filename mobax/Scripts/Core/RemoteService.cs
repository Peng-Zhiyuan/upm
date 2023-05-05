using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteService : Service
{
    public override void OnCreate()
    {
        Remote.getLanguageDelegate = GetLanguage;
    }

    static string GetLanguage()
    {
        return LocalizationManager.Stuff.Language;
    }
}
