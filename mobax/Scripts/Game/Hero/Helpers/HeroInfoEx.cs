using System;
using System.Collections.Generic;
using BattleEngine.Logic;

public static class HeroInfoEx
{
    private static Dictionary<int, List<int>> _starSkillsMap;
    private static Dictionary<int, int> _specialSkillsMap;
    private static Dictionary<int, List<int>> _starIdsMap;

    public static (List<int>, int) GetStarSkillList(HeroInfo heroInfo)
    {
        _starSkillsMap ??= new Dictionary<int, List<int>>();
        _specialSkillsMap ??= new Dictionary<int, int>();

        if (!_starSkillsMap.TryGetValue(heroInfo.HeroId, out var list))
        {
            list = new List<int>();

            var cfg = heroInfo.FirstStarConfig;
            int explorerSkillId = 0;
            while (true)
            {
                bool skip = false;
                if (cfg.skillId == 0 || cfg.skillId == explorerSkillId)
                {
                    skip = true;
                }
                else
                {
                    var skillCfg = HeroSkillHelper.GetSkillConfig(cfg.skillId, cfg.skillLevel);
                    if (null == skillCfg)
                    {
                        throw new Exception($"【配置错误】没有找到skillId={cfg.skillId} and skillLevel={cfg.skillLevel}的技能配置");
                    }
                    if ((SKILL_TYPE) skillCfg.skillType == SKILL_TYPE.EXPLORER)
                    {
                        explorerSkillId = cfg.skillId;
                        skip = true;
                    }
                }

                if (!skip)
                {
                    if (!list.Contains(cfg.skillId))
                    {
                        list.Add(cfg.skillId);
                    }
                }
                if (cfg.Next == 0) break;
                cfg = StaticData.HeroStarTable.TryGet(cfg.Next);
                if (null == cfg) break;
            }

            _starSkillsMap[heroInfo.HeroId] = list;
            _specialSkillsMap[heroInfo.HeroId] = explorerSkillId;
        }
        
        return (list, _specialSkillsMap[heroInfo.HeroId]);
    }

    public static HeroStarRow PrevStarCfg(HeroInfo heroInfo, int starCfgId)
    {
        _starIdsMap ??= new Dictionary<int, List<int>>();
        
        if (!_starIdsMap.TryGetValue(heroInfo.HeroId, out var list))
        {
            list = new List<int>();
            var cfg = heroInfo.FirstStarConfig;

            while (true)
            {
                list.Add(cfg.Id);
                
                if (cfg.Next == 0) break;
                cfg = StaticData.HeroStarTable.TryGet(cfg.Next);
            }

            _starIdsMap[heroInfo.HeroId] = list;
        }

        var index = list.IndexOf(starCfgId);
        if (index <= 0)
        {
            return null;
        }

        return StaticData.HeroStarTable.TryGet(list[index - 1]);
    }
}