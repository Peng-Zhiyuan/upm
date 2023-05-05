using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PositionLocalizer : SerializedMonoBehaviour
{
    [SerializeField, ReadOnly]
    Vector3 origin;
    public Dictionary<string, Vector3> languageToGameObjectDic = new Dictionary<string, Vector3>();

    private void Awake()
    {
        origin = this.transform.localPosition;
    }

    private void OnEnable()
    {
        var language = LocalizationManager.Stuff.Language;
        this.SetPositionAsLanguage(language);
    }

    void SetPositionAsLanguage(string language)
    {
        foreach (var kv in languageToGameObjectDic)
        {
            var lan = kv.Key;
            var lcoalPosition = kv.Value;
            if (lan == language)
            {
                this.transform.localPosition = lcoalPosition;
            }
            return;
        }

        this.transform.localPosition = origin;
    }
}
