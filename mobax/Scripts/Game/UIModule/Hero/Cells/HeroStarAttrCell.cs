using UnityEngine;

public partial class HeroStarAttrCell : MonoBehaviour
{
    private int _attr;
    
    public void SetInfo(int attr, int value, bool locked = false)
    {
        _attr = attr;
        
        var conf = StaticData.HeroAttrTable.TryGet(attr);
        Txt_name.text = LocalizationManager.Stuff.GetText(conf.Desc);
        Txt_value.text = conf.Ptype == 1 ? $"+{value / 10f}%" : $"+{value}";;
        Lock_title.gameObject.SetActive(!locked);
    }

    public void RefreshValue(int value)
    {
        var conf = StaticData.HeroAttrTable.TryGet(_attr);
        Txt_value.text = conf.Ptype == 1 ? $"+{value / 10f}%" : $"+{value}";;
    }
}