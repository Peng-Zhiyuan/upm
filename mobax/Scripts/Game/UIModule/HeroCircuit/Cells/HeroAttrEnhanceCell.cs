using CustomLitJson;
using UnityEngine;

public partial class HeroAttrEnhanceCell : MonoBehaviour
{
    public void SetInfo(HeroCircuitInfo circuitInfo, int attrIndex)
    {
        var attrInfo = circuitInfo.Attrs[attrIndex];
        var attrId = (HeroAttr) attrInfo.TryGet("id", 0);
        var conf = StaticData.HeroAttrTable.TryGet((int) attrId);
        var currentVal = HeroCircuitHelper.GetAttrVal(circuitInfo, attrIndex);
        Txt_attrName.text = LocalizationManager.Stuff.GetText(conf.Desc);
        var valStr = conf.Ptype == 1 ? $"{currentVal / 10f}%" : $"{currentVal}";
        if (!HeroCircuitHelper.AttrMax(circuitInfo.Qlv, attrIndex))
        {
            Txt_attrVal_next.text = valStr;
            Node_attrNew.gameObject.SetActive(false);
            return;
        }
        
        Node_attrNew.gameObject.SetActive(true);
        Txt_attrVal_prev.text = valStr;
        var nextVal = HeroCircuitHelper.GetNextAttrVal(circuitInfo, attrIndex);
        var nextValStr = conf.Ptype == 1 ? $"{nextVal / 10f}%" : $"{nextVal}";
        Txt_attrVal_next.text = nextValStr;
    }
}