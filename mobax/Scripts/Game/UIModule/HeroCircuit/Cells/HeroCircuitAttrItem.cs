using System.Linq;
using UnityEngine;

public partial class HeroCircuitAttrItem : MonoBehaviour
{
    private Color _defaultNameColor;
    private Color _defaultValColor;
    
    private void Awake()
    {
        _defaultNameColor = Txt_attrName.color;
        _defaultValColor = Txt_attrVal.color;
    }

    public void SetInfo(HeroCircuitInfo circuitInfo, int attrIndex)
    {
        var attrInfo = circuitInfo.Attrs[attrIndex];
        var attrId = (HeroAttr) attrInfo.TryGet("id", 0);
        var conf = StaticData.HeroAttrTable.TryGet((int) attrId);
        var currentVal = HeroCircuitHelper.GetAttrVal(circuitInfo, attrIndex);
        Txt_attrName.SetLocalizer(conf.Desc);
        var valStr = conf.Ptype == 1 ? $"{currentVal / 10f}%" : $"{currentVal}";
        Txt_attrVal.text = valStr;

        var reached = circuitInfo.Qlv >= attrIndex + 2;
        if (reached)
        {
            Txt_attrName.color = _defaultNameColor;
            Txt_attrVal.color = _defaultValColor;
        }
        else
        {
            Txt_attrName.color = Color.grey;
            Txt_attrVal.color = Color.grey;
        }
    }

    public void SetAttrType(int attrId)
    {
        var cfg = StaticData.PuzzleAttrTable.TryGet(attrId * 100 + 1);
        var val = cfg.Vals.First();
        var conf = StaticData.HeroAttrTable.TryGet(attrId);
        Txt_attrName.SetLocalizer(conf.Desc);
        Txt_attrVal.text = conf.Ptype == 1 ? $"{val / 10f}%" : $"{val}";
    }
}