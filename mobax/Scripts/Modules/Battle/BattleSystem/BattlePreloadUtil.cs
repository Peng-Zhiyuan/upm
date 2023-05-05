using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.IO;
using BattleEngine.Logic;

public static class BattlePreloadUtil
{
    //Creature 模型
    //effect Manager 效果
    //audiomanager 音效
    private static LoadingFloating bttleLoading = null;

    public static async Task FinishLoading()
    {
        var floating = UIEngine.Stuff.FindFloating<LoadingFloating>();
        if (floating != null)
        {
            floating.Value = 1f;
            await floating.WaitProcessBarFullAsync();
            await Task.Delay(100);
            floating.Remove();
        }
    }

    /// <summary>
    /// 加载资源模型
    /// </summary>
    /// <param name="heroIDLst"></param>
    public static async Task PreLoadModel(List<int> heroIDLst)
    {
        var addressList = new List<string>();
        foreach (var id in heroIDLst)
        {
            var heroRow = StaticData.HeroTable.TryGet(id);
            if (heroRow == null)
            {
                continue;
            }
            addressList.Add(Path.ChangeExtension(heroRow.Model, "_low.prefab"));
            addressList.Add(Path.ChangeExtension(heroRow.Model, ".prefab"));
        }
        var bucket = BucketManager.Stuff.Battle;
        await bucket.AquireListIfNeedAsync<UnityEngine.Object>(addressList);
    }

    public static async Task PreLoadSkillRes(List<int> heroIDLst)
    {
        var addressList = new List<string>();
        List<int> skillIDLst = new List<int>();
        foreach (var id in heroIDLst)
        {
            var heroRow = StaticData.HeroTable.TryGet(id);
            if (heroRow == null)
            {
                continue;
            }
            skillIDLst.AddRange(heroRow.autoSkills);
            skillIDLst.Add(heroRow.assistSkill);
            skillIDLst.Add(heroRow.Ultimate);
        }
        string skillDataPath = "";
        SkillConfigObject skillConfigObject;
        Dictionary<string, string> resDic = new Dictionary<string, string>();
        for (int i = 0; i < skillIDLst.Count; i++)
        {
            if (skillIDLst[i] == 0)
            {
                continue;
            }
            //技能资源同ID相同,所以用1级的加载就可以
            SkillRow skillRow = SkillUtil.GetSkillItem(skillIDLst[i], 1);
            if (skillRow == null)
            {
                continue;
            }
            skillDataPath = string.Format(AddressablePathConst.SkillConfPath, skillRow.skillData);
            if (BucketManager.Stuff.Battle.Get<ScriptableObject>(skillDataPath, true) != null)
            {
                continue;
            }
            ScriptableObject config = await BucketManager.Stuff.Battle.GetOrAquireAsync<ScriptableObject>(skillDataPath);
            if (config == null)
            {
                continue;
            }
            skillConfigObject = config as SkillConfigObject;
            string[] needResPath = skillConfigObject.GetAllElementsResPath();
            for (int j = 0; j < needResPath.Length; j++)
            {
                if (resDic.ContainsKey(needResPath[j]))
                {
                    continue;
                }
                resDic[needResPath[j]] = needResPath[j];
                addressList.Add(needResPath[j]);
            }
        }
        await BucketManager.Stuff.Battle.AquireListIfNeedAsync<UnityEngine.Object>(addressList);
        addressList.Clear();
        for (int i = 0; i < heroIDLst.Count; i++)
        {
            var heroRow = StaticData.HeroTable.TryGet(heroIDLst[i]);
            if (heroRow == null)
            {
                continue;
            }
            addressList.Add(StrBuild.Instance.ToStringAppend(heroRow.Model, ".prefab"));
        }
        await BucketManager.Stuff.Battle.AquireListIfNeedAsync<UnityEngine.Object>(addressList);
    }

    public static async Task PreLoadSkillTimeLineRes(List<int> heroIDLst)
    {
        Dictionary<int, int> SPSSkillDic = new Dictionary<int, int>();
        foreach (var id in heroIDLst)
        {
            var heroRow = StaticData.HeroTable.TryGet(id);
            if (heroRow == null)
            {
                continue;
            }
            SPSSkillDic.Add(id, heroRow.Ultimate);
        }
        string skillDataPath = "";
        SkillConfigObject skillConfigObject;
        Dictionary<string, string> resDic = new Dictionary<string, string>();
        var data = SPSSkillDic.GetEnumerator();
        while (data.MoveNext())
        {
            if (data.Current.Value == 0)
            {
                continue;
            }
            SkillRow skillRow = SkillUtil.GetSkillItem(data.Current.Value, 1);
            if (skillRow == null)
            {
                continue;
            }
            skillDataPath = string.Format(AddressablePathConst.SkillConfPath, skillRow.skillData);
            ScriptableObject config = await BucketManager.Stuff.Battle.GetOrAquireAsync<ScriptableObject>(skillDataPath);
            if (config == null
                || config as SkillConfigObject == null)
            {
                continue;
            }
            skillConfigObject = config as SkillConfigObject;
            string[] needResPath = skillConfigObject.GetElementsResPath(SKILL_ACTION_ELEMENT_TYPE.TimeLine);
            for (int j = 0; j < needResPath.Length; j++)
            {
                if (resDic.ContainsKey(needResPath[j]))
                {
                    continue;
                }
                resDic[needResPath[j]] = needResPath[j];
                string timelinePath = AddressablePathConst.SkillEditorPathParse(needResPath[j]);
                SkillTimelineUnit timeLineUnit = await GameObjectPoolUtil.ReuseAddressableObjectAsync<SkillTimelineUnit>(BucketManager.Stuff.Battle, timelinePath);
                if (timeLineUnit == null)
                    continue;
                if (timeLineUnit.AvatarObj == null)
                {
                    HeroRow heroRow = StaticData.HeroTable.TryGet(data.Current.Key);
                    if (heroRow == null)
                    {
                        continue;
                    }
                    string modelPath = StrBuild.Instance.ToStringAppend(heroRow.Model, ".prefab");
                    HeroInfo heroInfo = HeroManager.Instance.GetHeroInfo(data.Current.Key);
                    if (heroInfo != null && heroInfo.Unlocked)
                    {
                        modelPath = StrBuild.Instance.ToStringAppend(RoleHelper.GetAvatarName(heroInfo), ".prefab");
                    }
                    await timeLineUnit.PlaySkillTimeLine(modelPath);
                }
                BucketManager.Stuff.Battle.Pool.Recycle(timeLineUnit);
            }
        }
    }
}