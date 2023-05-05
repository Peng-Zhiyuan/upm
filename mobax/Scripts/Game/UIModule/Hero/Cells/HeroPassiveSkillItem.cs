using System.Linq;
using UnityEngine;

public partial class HeroPassiveSkillItem : MonoBehaviour
{
    private bool _unlocked;
    private HeroInfo _heroInfo;
    
    public void SetInfo(HeroInfo heroInfo, int skillId)
    {
        _heroInfo = heroInfo;
        _unlocked = heroInfo.StarSkillMap.TryGetValue(skillId, out var level);
        if (_unlocked)
        {
            var cfg = HeroSkillHelper.GetSkillConfig(skillId, level);
            Txt_name.SetLocalizer(cfg.Name);
            Txt_level.text = $"<size=14>lv.</size>{level:00}";
        }

        Flag.Selected = _unlocked;
    }

    public void OnClick()
    {
        if (!_unlocked)
        {
            ToastManager.ShowLocalize("M4_heroSkill_unlocked");
            return;
        }
        
        UIEngine.Stuff.ForwardOrBackTo<HeroSkillPop>(new HeroSkillPopParam {category = SkillCategoryEnum.Passive, heroInfo = _heroInfo});
    }
}