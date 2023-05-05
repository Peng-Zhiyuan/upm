using System;
using BattleEngine.Logic;
using UnityEngine;
using UnityEngine.Serialization;

public partial class HeroSkillCell : MonoBehaviour
{
    public SKILL_TYPE skillType;
    public int viewLevel;
    public Action<int, int> OnClick;
    
    private HeroInfo _heroInfo;
    private int _skillId;
    private int _skillLv;
    
    public SkillRow SetInfo(HeroInfo heroInfo)
    {
        var skillId = HeroSkillHelper.GetSkillId(heroInfo, skillType);
        return SetInfo(heroInfo, skillId, viewLevel);
    }

    public SkillRow SetInfo(HeroInfo heroInfo, int skillId, int skillLv = 0)
    {
        if (skillLv <= 0) skillLv = heroInfo.GetSkillLevel(skillId);
        var skillCfg = HeroSkillHelper.GetSkillConfig(skillId, skillLv);
        var icon = skillCfg.Icon;
        UiUtil.SetSpriteInBackground(Img_icon, () => $"{icon}.png");
        UiUtil.SetSpriteInBackground(Img_frame, () => HeroHelper.GetResAddress(heroInfo.HeroId, HeroResType.SkillFrame));

        var isCommonAtk = skillCfg.skillType == (int) SKILL_TYPE.ATK;
        Node_level.SetActive(!isCommonAtk);
        if (!isCommonAtk)
        {
            Text_level.text = $"{skillLv}";
        }
        _heroInfo = heroInfo;
        _skillId = skillId;
        _skillLv = skillLv;

        return skillCfg;
    }
    
    public void RefreshLevel()
    {
        Text_level.text = $"{_heroInfo.GetSkillLevel(_skillId)}";
    }

    public void OnCellClick()
    {
        OnClick?.Invoke(_skillId, _skillLv);
    }
}