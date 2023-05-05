using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LauncherTextLocalizer : MonoBehaviour
{
    public string key; 

    Text _text;
    public Text Text
    {
        get
        {
            if (_text == null)
            {
                _text = GetComponent<Text>();
            }
            return _text;
        }
    }

    private void Start()
    {
        if (Text == null || string.IsNullOrEmpty(key))
        {
            return;
        }
        this.Text.text = LauncherLocalizatioManager.Get(key);
    }
}
