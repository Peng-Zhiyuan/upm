using System.Linq;
using UnityEngine;

public partial class HeroPassiveSkillDetailCell : MonoBehaviour
{
    public void SetInfo(HeroInfo heroInfo, int skillId)
    {
        if (heroInfo.StarSkillMap.TryGetValue(skillId, out var level))
        {
            var cfg = HeroSkillHelper.GetSkillConfig(skillId, level);
            Txt_name.SetLocalizer(cfg.Name);
            Txt_level.text = $"Lv.{level:00}";
            Txt_skillDesc.Text = cfg.Desc.Localize();
        }
    }
    
    public void SetInfo(int skillId, int skillLevel)
    {
        var cfg = HeroSkillHelper.GetSkillConfig(skillId, skillLevel);
        Txt_name.SetLocalizer(cfg.Name);
        Txt_level.text = $"Lv.{skillLevel:00}";
        Txt_skillDesc.Text = cfg.Desc.Localize();
    }
}


