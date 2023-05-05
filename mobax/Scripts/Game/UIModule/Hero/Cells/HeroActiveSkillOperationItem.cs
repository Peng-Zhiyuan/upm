using System;
using System.Linq;
using BattleEngine.Logic;
using UnityEngine;

public partial class HeroActiveSkillOperationItem : MonoBehaviour
{
    private HeroInfo _heroInfo;
    private int _skillId;
    private int _skillLv;
    private bool _canUp;
    
    public void SetInfo(HeroInfo heroInfo, int skillId)
    {
        var skillCfg = Item_skillDetail.SetInfo(heroInfo, skillId);
        _canUp = skillCfg.skillType != (int) SKILL_TYPE.ATK;
        Button_levelUp.SetActive(_canUp);
        if (_canUp)
        {
            var isMax = HeroSkillHelper.LevelMax(skillId, skillCfg.Level);
            Label_levelUp.SetLocalizer(isMax ? "M4_level_max" : "M4_level_up");
        }

        _heroInfo = heroInfo;
        _skillId = skillId;
        _skillLv = skillCfg.Level;
    }

    public void OnClickLevelUp()
    {
        // if (HeroSkillHelper.LevelMax(_skillId, _skillLv))
        // {
        //     ToastManager.ShowLocalize("M4_max_already");
        //     return;
        // }
        
        UIEngine.Stuff.ForwardOrBackTo<HeroSkillUpPopV2>(new HeroSkillUpPopParam
        {
            heroInfo = _heroInfo,
            skillId = _skillId,
            skillLv = _skillLv,
        });
    }

    private void OnEnable()
    {
        HeroNotifier.OnChange += _OnChanged;
    }

    private void OnDisable()
    {
        HeroNotifier.OnChange -= _OnChanged;
    }
    
    private void _OnChanged(HeroNotifyEnum notifyEnum)
    {
        switch (notifyEnum)
        {
            case HeroNotifyEnum.Skill:
            case HeroNotifyEnum.SkillReset:
                _RefreshView();
                break;
        }
    }

    private void _RefreshView()
    {
        if (!_canUp) return;
        
        SetInfo(_heroInfo, _skillId);
    }
}