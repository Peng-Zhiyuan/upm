using System.Linq;
using UnityEngine;

public partial class CircuitInformationBrief : MonoBehaviour
{
    // 技能颜色
    private Color _defaultSkillDescColor;
    private HeroCircuitInfo _circuitInfo;
    
    public void SetInfo(HeroCircuitInfo circuitInfo)
    {
        var colorCfg = StaticData.PuzzleColorTable.TryGet(circuitInfo.Color);
        Cell_circuit.SetInfo(circuitInfo);
        Txt_circuitName.SetLocalizer("M4_circuit_words_item_format", colorCfg.Desc.Localize(), circuitInfo.Conf.Name.Localize());
        // 显示单块属性  
        _circuitInfo = circuitInfo;
        List_circuitAttrs.numItems = (uint) circuitInfo.Attrs.Count;
        // 显示技能相关
        var skillArray = StaticData.SkillTable.TryGet(circuitInfo.Skill);
        if (null != skillArray)
        {
            var skillCfg = skillArray.Colls.First();
            Txt_skillDesc.Text = skillCfg.Desc.Localize();
            Txt_skillDesc.Color = circuitInfo.SkillUnlocked ? _defaultSkillDescColor : Color.gray;
        }
        else
        {
            Txt_skillDesc.Text = "";
        }
    }

    public void RefreshShape()
    {
        Cell_circuit.RefreshShape();
    }

    private void Awake()
    {
        // 技能颜色
        _defaultSkillDescColor = Txt_skillDesc.Color;
        List_circuitAttrs.onItemRenderAction = _OnAttrRender;
    }
    
    private void _OnAttrRender(int index, Transform tf)
    {
        var attrItem = tf.GetComponent<HeroCircuitAttrItem>();
        attrItem.SetInfo(_circuitInfo, index);
    }
}