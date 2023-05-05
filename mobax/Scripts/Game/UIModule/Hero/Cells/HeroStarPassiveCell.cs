using BattleEngine.Logic;
using UnityEngine;

public partial class HeroStarPassiveCell : MonoBehaviour
{
    private HeroInfo _heroInfo;
    private int _skillId;
    private int _skillLevel;
    
    public void SetInfo(HeroInfo heroInfo, int skillId, bool locked = false)
    {
        heroInfo.StarSkillMap.TryGetValue(skillId, out var level);
        SetInfo(heroInfo, skillId, level, locked);
    }
    
    public void SetInfo(HeroInfo heroInfo, int skillId, int skillLevel, bool locked = false)
    {
        _heroInfo = heroInfo;
        _skillId = skillId;
        _skillLevel = skillLevel;
        
        var cfg = HeroSkillHelper.GetDefaultSkill(skillId);
        Txt_name.SetLocalizer(cfg.Name);
        Txt_level.text = $"<size=14>lv.</size>{skillLevel:00}";
        Lock_title.gameObject.SetActive(!locked);
        Label_title.SetLocalizer(cfg.skillType == (int) SKILL_TYPE.EXPLORER ? "M4_heroSkill_explorer" : "M4_heroSkill_passive");
    }

    public void RefreshLevel(int skillLevel)
    {
        Txt_level.text = $"<size=14>lv.</size>{skillLevel:00}";
    }

    public void OnClick()
    {
        UIEngine.Stuff.ShowFloating<PassiveSkillInfoFloating>(new PassiveSkillInfoParam
        {
            skillId = _skillId,
            skillLevel = _skillLevel,
        });
    }
}