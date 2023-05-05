using UnityEngine.UI;

using UnityEngine;

public partial class HeroAttrDetailItem : MonoBehaviour
{
    public void SetInfo(HeroInfo heroInfo, HeroAttrRow attrRow, int index)
    {
        // 名称
        Txt_attrName.text = LocalizationManager.Stuff.GetText(attrRow.Desc);
        // icon
        var bg = GetComponent<Image>();
        bg.enabled = index % 2 == 0;
        // 数值
        var val = heroInfo.GetAttrFinal((HeroAttr) attrRow.Id);
        string valStr;
        switch (attrRow.Ptype)
        {
            case 1: 
                // 千分比， 除以1000后要再乘以100，所以就是除以10
                valStr = $"{val / 10f}%";
                break;
            default:
                valStr = $"{val}";
                break;
        }

        Txt_attrVal.text = valStr;
    }
}