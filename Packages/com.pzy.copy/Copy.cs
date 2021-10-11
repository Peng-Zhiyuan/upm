using UnityEngine;
using System.Collections;


public static class Copy
{
    public static void CopyTextToClipboard(string text)
    {
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
            case RuntimePlatform.IPhonePlayer:
            { 
                NativeBridge.Call("CopyProxy", "Copy", text);
                break;
            }
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.WindowsEditor:
                TextEditor t = new TextEditor();  
                t.text = text;
                t.OnFocus();  
                t.Copy();  
                break;
        }
    }

}
