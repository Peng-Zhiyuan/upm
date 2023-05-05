using System;
using System.Linq;
using BattleEngine.Logic;
using UnityEngine;

public partial class HeroActiveSkillDetailCell : MonoBehaviour
{
    public SkillRow SetInfo(HeroInfo heroInfo, int skillId, int skillLv = 0)
    {
        var skillCfg = Cell_skill.SetInfo(heroInfo, skillId, skillLv);
        Txt_skillType.SetLocalizer($"common_skillType_{skillCfg.skillType}");
        Txt_name.SetLocalizer(skillCfg.Name);
        Txt_skillDesc.Text = $"{skillCfg.Desc.Localize()}\n{skillCfg.Desc2.Localize()}";
        Txt_comboValue.text = $"{skillCfg.Combo}";
        Txt_stamValue.text = $"{skillCfg.stamRate}";

        return skillCfg;
    }

    public void ShowBottom()
    {
        Txt_skillDesc.ScrollToBottom();
    }
}