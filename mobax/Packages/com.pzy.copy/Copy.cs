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

    public static bool IsDistribution()
    {
        if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
            var ret = NativeBridge.Call("CopyProxy", "IsDistribution");
            if(ret == "true")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        throw new System.Exception("IsDistribution not support in platform: " + Application.platform);
    }

}
