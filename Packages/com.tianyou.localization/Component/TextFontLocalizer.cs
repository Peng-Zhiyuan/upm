using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class TextFontLocalizer : MonoBehaviour
{
    static Font _defaultFont = null;
    public static Font DefaultFont{
        get{
            if(_defaultFont == null){
                _defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            return _defaultFont;
        }
    }

    Text _text;
    Text Text{
        get{
            if(_text == null){
                _text = GetComponent<Text>();
            }
            return _text;
        }
    }

    [SerializeField]
    bool forceOriginalFont = false;

    void OnEnable()
    {
        if(forceOriginalFont){
            return;
        }
        Text.ToLocalFont();
        LanguageManager.LanguageChanged += Refresh;
    }

    void OnDisable(){
        if(forceOriginalFont){
            return;
        }
        LanguageManager.LanguageChanged -= Refresh;
    }

    void Refresh(string lang){
        if(forceOriginalFont){
            return;
        }
        Text.ToLocalFont();
    }


}
