using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Paste
{
    public static string PasteTextFromClipboard()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
            case RuntimePlatform.IPhonePlayer:
                    return NativeBridge.Call("PasteProxy", "Paste", "");
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.WindowsEditor:
                TextEditor t = new TextEditor();
                if (t.CanPaste())
                {
                    t.OnFocus();
                    t.Paste();
                    return t.text;
                }
                else
                {
                    return "";
                }
            default: return "";
        }
    }

}
