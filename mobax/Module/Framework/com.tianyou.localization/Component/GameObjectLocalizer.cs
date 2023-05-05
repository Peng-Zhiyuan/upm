using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GameObjectLocalizer : MonoBehaviour
{
    public GameObject defaultGameObject;

    [ShowInInspector, ReadOnly]
    Dictionary<string, GameObject> languageToGameObjectDic = new Dictionary<string, GameObject>();

    private void Awake()
    {
        {
            var t = this.transform.Find("en");
            if( t != null)
            {
                languageToGameObjectDic["en"] = t.gameObject;
            }

        }
        {
            var t = this.transform.Find("cn");
            if(t != null)
            {
                languageToGameObjectDic["cn"] = t.gameObject;
            }

        }
        {
            var t = this.transform.Find("jp");
            if (t != null)
            {
                languageToGameObjectDic["jp"] = t.gameObject;
            }
        }
        {
            var t = this.transform.Find("tw");
            if (t != null)
            {
                languageToGameObjectDic["tw"] = t.gameObject;
            }
        }

    }

    private void OnEnable()
    {
        var language = LocalizationManager.Stuff.Language;
        this.SetOnlyGameObjectOfLanguageActive(language);
        LanguageManager.LanguageChanged += OnLanguageChanged;
    }

    void OnDisable()
    {
        LanguageManager.LanguageChanged -= OnLanguageChanged;
    }

    void OnLanguageChanged(string language)
    {
        this.SetOnlyGameObjectOfLanguageActive(language);
    }

    void SetOnlyGameObjectOfLanguageActive(string language)
    {
        var foundLanguage = false;
        GameObject selectedGo = null;
        foreach (var kv in languageToGameObjectDic)
        {
            var lan = kv.Key;
            var go = kv.Value;
      
            if (lan == language)
            {
                go.SetActive(true);
                foundLanguage = true;
                selectedGo = go;
            }
            else
            {
                if(selectedGo != go)
                {
                    go.SetActive(false);
                }
            }
        }

        if(defaultGameObject != null)
        {
            if (!foundLanguage)
            {
                defaultGameObject.SetActive(true);
            }
            else
            {
                if(selectedGo != defaultGameObject)
                {
                    defaultGameObject.SetActive(false);
                }
            }
        }
    }
}
