using UnityEngine;
using UnityEngine.UI;
using System;

//[AddComponentMenu ("UI/UILocalization", 10)]
[Obsolete("已过时，使用 TextLocalizer 代替")]
public class UILocalization : MonoBehaviour
{
    protected Text mText;
    public string KeyString;
    //public UIFont CustomFont;
    public Text UIText
    {
        get
        {
            if (mText == null)
                mText = gameObject.GetComponent<Text>();
            return mText;
        }
    }

    protected void Awake()
    {
        //UpdateFont();
        if (!string.IsNullOrEmpty(KeyString))
        {
            string message = LocalizationManager.Stuff.GetText(KeyString);
            if (!message.Contains("{0}"))
            {
                UIText.text = message;
            }
            else
            {
                UIText.text = "";
            }
        }
    }

    // public void UpdateFont()
    // {
    //     if (CustomFont != null) 
    //     {
    //         UIText.font = CustomFont.UseFont;
    //     }
    // }

    public void ChangeArg(params object[] arglist)
    {
        var msg = LocalizationManager.Stuff.GetText(this.KeyString, arglist);
        UIText.text = msg;
    }

    public void ChangeKey(string key, params object[] arglist)
    {
        this.KeyString = key;
        ChangeArg(arglist);
    }
}