using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExceptionService : Service
{
    public override void OnCreate()
    {
        ExceptionManager.RegisterHook();
        ExceptionManager.codeToUserMessageDelegate = CodeToUserMessag;
    }

    static string CodeToUserMessag(string code)
    {
        var b = LocalizationManager.Stuff.HasText(code);
        if(!b)
        {
            return null;
        }
        else
        {
            var userMsg = code.Localize();
            return userMsg;
        }
    }
}
