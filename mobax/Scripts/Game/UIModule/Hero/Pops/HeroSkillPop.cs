using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class HeroSkillPop : Page
{
    private HeroInfo _heroInfo;
    private bool _passiveDataInitialized;
    private List<int> _passiveSkills;
    private List<int> _activeSkills;
    private int _explorerSkillId;
    private uint _passiveLen;

    public override void OnForwardTo(PageNavigateInfo info)
    {
        if (info.param is HeroSkillPopParam param)
        {
            _heroInfo = param.heroInfo;

            Tab_category.FocusOn((int) param.category);
        }
    }

    private void Awake()
    {
        Tab_category.OnTabChanged = _OnCategoryChanged;
        List_activeSkills.onItemRenderAction = _RenderActiveSkillCell;
        List_passiveSkills.onItemRenderAction = _RenderPassiveSkillCell;
    }

    private void _OnCategoryChanged(int tab)
    {
        Node_content.Selected = tab;
        switch ((SkillCategoryEnum) tab)
        {
            case SkillCategoryEnum.Active:
                _activeSkills = new List<int> {_heroInfo.CommonAtkID, _heroInfo.CommonSkillId, _heroInfo.UltimateId};
                List_activeSkills.numItems = (uint) _activeSkills.Count;
                break;
            case SkillCategoryEnum.Passive:
                _InitPassiveData();
                _passiveLen = (uint) _heroInfo.StarSkillMap.Keys.Count;
                List_passiveSkills.numItems = _passiveLen;
                break;
        }
    }

    private void _InitPassiveData()
    {
        if (_passiveDataInitialized) return;

        (_passiveSkills, _explorerSkillId) = HeroInfoEx.GetStarSkillList(_heroInfo);
        _passiveDataInitialized = true;
    }

    private void _RenderActiveSkillCell(int index, Transform tf)
    {
        var cell = tf.GetComponent<HeroActiveSkillOperationItem>();
        cell.SetInfo(_heroInfo, _activeSkills[index]);
    }

    private void _RenderPassiveSkillCell(int index, Transform tf)
    {
        int skillId;
        if (index == _passiveLen - 1 && _heroInfo.StarSkillMap.ContainsKey(_explorerSkillId))
        {
            skillId = _explorerSkillId;
        }
        else
        {
            skillId = _passiveSkills[index];
        }

        var cell = tf.GetComponent<HeroPassiveSkillDetailCell>();
        cell.SetInfo(_heroInfo, skillId);
    }
}

public class HeroSkillPopParam
{
    public SkillCategoryEnum category;
    public HeroInfo heroInfo;
}

public enum SkillCategoryEnum
{
    Active = 0,
    Passive
}