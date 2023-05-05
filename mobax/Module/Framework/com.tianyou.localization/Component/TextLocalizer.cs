using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
public class TextLocalizer : MonoBehaviour
{
    public string key;
    public string[] parameters;

    [ShowInInspector, ReadOnly]
    private string lastLocalizeLanguage;

    Text text;

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

    public void Set(string key, params string[] parameters)
    {
        this.key = key;
        this.parameters = parameters;
        this.lastLocalizeLanguage = null;
        this.RefreshIfNeed();
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
        if (this.text == null)
        {
            return;
        }
        if(string.IsNullOrEmpty(this.key))
        {
            return;
        }
        string msg = LocalizationManager.Stuff.GetText(this.key, parameters);
        this.text.text = msg;
    }
}