/* Created:Loki Date:2022-10-17*/

using UnityEngine;
using BattleEngine.Logic;
using System.Collections.Generic;

public class HeroSkillViewCompent : MonoBehaviour
{
    private Dictionary<int, HeroSkillViewData> _viewDataDic = new Dictionary<int, HeroSkillViewData>();

    private bool _isPlaySkilling = false;
    public bool IsPlaying
    {
        get { return _isPlaySkilling; }
    }
    private float currentTime = 0.0f;
    private int curFrame = 0;
    private int lastFrame = -1;
    private HeroSkillViewData skillData;

    private System.Action delegatePlayEnd;

    public async void PlayHeroSkill(int skillID, int lv, System.Action callBack)
    {
        delegatePlayEnd = callBack;
        if (_viewDataDic.ContainsKey(skillID))
        {
            ExecuteSkillAbilityViewTask(_viewDataDic[skillID]);
            return;
        }
        SkillRow skillRow = SkillUtil.GetSkillItem(skillID, lv);
        if (skillRow == null)
        {
            return;
        }
        ScriptableObject config = await BucketManager.Stuff.Battle.GetOrAquireAsync<ScriptableObject>(string.Format(AddressablePathConst.SkillConfPath, skillRow.skillData));
        if (config == null)
        {
            Debug.LogError("技能表配置出错: " + skillRow.skillData);
            return;
        }
        HeroSkillViewData viewData = new HeroSkillViewData();
        viewData.InitData(this.gameObject, skillRow, config as SkillConfigObject);
        _viewDataDic.Add(skillRow.SkillID, viewData);
        ExecuteSkillAbilityViewTask(viewData);
    }

    private void ExecuteSkillAbilityViewTask(HeroSkillViewData data)
    {
        skillData = data;
        currentTime = 0;
        lastFrame = -1;
        var aa = StrBuild.Instance.ToStringAppend("skill_", data._skillRow.SkillID.ToString());
        WwiseEventManager.SendEvent(TransformTable.BeginSkillAbility, aa);
        _isPlaySkilling = true;
    }

    public void Update()
    {
        if (!IsPlaying
            || skillData == null)
        {
            return;
        }
        curFrame = Mathf.FloorToInt(currentTime * skillData.SkillFrameFPS);
        if (lastFrame != curFrame)
        {
            lastFrame = curFrame;
            ExecuteFrame(curFrame);
        }
        currentTime += Time.deltaTime;
    }

    private void ExecuteFrame(int frameIndex)
    {
        for (int i = 0; i < skillData.ActionTaskDataLst.Count; i++)
        {
            if (skillData.ActionTaskDataLst[i] == null)
            {
                continue;
            }
            if (frameIndex == skillData.ActionTaskDataLst[i].AbilityTaskData.startFrame)
            {
                skillData.ActionTaskDataLst[i].BeginExecute(frameIndex);
            }
            else if (frameIndex > skillData.ActionTaskDataLst[i].AbilityTaskData.startFrame
                     && frameIndex < skillData.ActionTaskDataLst[i].AbilityTaskData.endFrame)
            {
                skillData.ActionTaskDataLst[i].DoExecute(frameIndex);
            }
            else if (frameIndex == skillData.ActionTaskDataLst[i].AbilityTaskData.endFrame)
            {
                skillData.ActionTaskDataLst[i].EndExecute();
            }
        }
        if (frameIndex == skillData.TotalFrame)
        {
            _isPlaySkilling = false;
            skillData = null;
            currentTime = 0.0f;
            lastFrame = -1;
            gameObject.GetComponent<Animator>().CrossFade("stand", 0.2f);
            delegatePlayEnd?.Invoke();
        }
    }
}