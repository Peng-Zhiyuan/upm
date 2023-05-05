using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System;


public class FontLocalizer : MonoBehaviour
{
    [ShowInInspector, ReadOnly]
    private string lastLocalizeLanguage;

    Text text;

    public static Func<string, Font> onGetFont;

    public bool enable = true;

    public static Font GetFont(string language)
    {
        if(onGetFont == null)
        {
            throw new Exception("[FontLocalizer] onGetFont not set yet");
        }
        var font = onGetFont.Invoke(language);
        return font;
    }

    void Awake()
    {
        var text = this.GetComponent<Text>();
        this.text = text;

    }

    void OnEnable()
    {
        RefreshIfNeed();
        LocalizationManager.Stuff.LanguageChanged += OnLanguageChange;
    }

    void OnDisable()
    {
        LocalizationManager.Stuff.LanguageChanged -= OnLanguageChange;
    }

    void OnLanguageChange(string lang)
    {
        RefreshIfNeed();
    }

    bool HasTextComponent
    {
        get
        {
            if (text != null)
            {
                return true;
            }
            return false;
        }
    }

    void RefreshIfNeed()
    {
        var hasTextComponent = this.HasTextComponent;
        if (!hasTextComponent)
        {
            return;
        }
        var currentLanguage = LocalizationManager.Stuff.Language;
        if (lastLocalizeLanguage == currentLanguage)
        {
            return;
        }

        Refresh();

        this.lastLocalizeLanguage = currentLanguage;
    }

    void Refresh()
    {
        if(!enable)
            return;
        
        if (this.text == null)
        {
            return;
        }

        var language = LocalizationManager.Stuff.Language;
        var font = GetFont(language);
        this.text.font = font;
    }

}
