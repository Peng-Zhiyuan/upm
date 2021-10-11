using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GameObjectLocalizer : SerializedMonoBehaviour
{
    public GameObject defaultGameObject;
    public Dictionary<string, GameObject> languageToGameObjectDic = new Dictionary<string, GameObject>();

    private void OnEnable()
    {
        var language = LanguageManager.Language;
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
