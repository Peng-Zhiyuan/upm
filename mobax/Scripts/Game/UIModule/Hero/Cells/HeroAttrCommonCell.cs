using System;
using UnityEngine;
using UnityEngine.UI;

public class HeroAttrCommonCell: MonoBehaviour
{
    public Text nameTxt;
    public Text valTxt;
    
    private Color _defaultNameColor;
    private Color _defaultValColor;
    
    private void Awake()
    {
        _defaultNameColor = nameTxt.color;
        _defaultValColor = valTxt.color;
    }

    public void SetCommonInfo(HeroAttr attr, int val)
    {
        var conf = StaticData.HeroAttrTable.TryGet((int) attr);
        nameTxt.text = LocalizationManager.Stuff.GetText(conf.Desc);
        string valStr;
        if (conf.Ptype == 1)
        {
            // 千分比
            valStr = $"+{val / 10f}%";
        }
        else
        {
            valStr = $"+{val}";
        }
        valTxt.text = valStr;
    }

    public void SetReachedInfo(HeroAttr attr, int val, bool reached)
    {
        SetCommonInfo(attr, val);

        if (reached)
        {
            nameTxt.color = _defaultNameColor;
            valTxt.color = _defaultValColor;
        }
        else
        {
            nameTxt.color = Color.grey;
            valTxt.color = Color.grey;
        }
    }
}