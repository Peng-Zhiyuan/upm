using System.Collections.Generic;
using System.Threading.Tasks;
using CustomLitJson;

public static class HeroApi
{
    /**
     * 英雄升级
     * @param id 卡牌_id
     * @param to 到达的目标等级
     * @returns 
     */
    public static async Task LevelUpAsync(int id, int to)
    {
        var param = new JsonData
        {
            ["id"] = id,
            ["lv"] = to
        };
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "hero/lv", param);
        TrackManager.LevelUp(id.ToString(), to);
    }
    
    /**
     * 英雄等级重置
     * @param id 卡牌_id
     * @returns 
     */
    public static async Task LevelResetAsync(int id)
    {
        var param = new JsonData
        {
            ["id"] = id,
        };
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "hero/lvreset", param, isAutoShowReward: DisplayType.Show);
    }

    /**
     * 英雄突破
     * @param id 卡牌_id
     * @returns 
     */
    public static async Task BreakAsync(int id)
    {
        var param = new JsonData
        {
            ["id"] = id,
        };
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "hero/break", param);
        
        // 打点
        TrackManager.CustomReport("Advance", "hero", "" + id, "level", "" + id);
    }

    /**
     * 英雄升星
     * @param id 卡牌_id
     * @returns 
     */
    public static async Task StarUpAsync(int id)
    {
        var param = new JsonData
        {
            ["id"] = id,
        };
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "hero/star", param, isAutoShowReward: DisplayType.Show);
        
        // 打点
        TrackManager.CustomReport("Star_up", "hero", "" + id, "star", "" + id);
    }

    /**
     * 设置板娘形象
     * @param id 卡牌_id
     * @returns 
     */
    public static async Task SetDisplayHeroAsync(int id)
    {
        var param = new JsonData
        {
            ["id"] = id,
        };
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "hero/show", param);
        // 这里是临时的， 也就是同时会保存数据到头像icon中
        var param2 = new JsonData
        {
            ["icon"] = $"{id}",
        };
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "setting/role", param2);
        TrackManager.CustomReport("Poster", new Dictionary<string, string>()
        {
            {"hero", $"{id}"},
        });
    }

    /**
     * 英雄收藏
     * @param id 卡牌_id
     * @param on 是否喜欢
     * @returns
     */
    public static async Task LikeAsync(int id, bool on)
    {
        var param = new JsonData
        {
            ["id"] = id,
            ["t"] = on ? 1 : 0,
        };
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "hero/collect", param);
    }

    /** 英雄技能升级 */
    public static async Task SkillUpgradeAsync(int heroId, int skillType, int lv)
    {
        // 目前是刚好是减1就匹配， 如果之后不匹配了， 让后端将skillType跟表枚举对应
        var serverSkillType = skillType - 1;
        var param = new JsonData
        {
            ["id"] = heroId,
            ["skill"] = serverSkillType,
            ["lv"] = lv,
        };
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "hero/skill", param);
        TrackManager.CustomReport("skillUp", new Dictionary<string, string>
        {
            {"heroId", $"{heroId}"},
            {"skillId", $"{skillType}"},
        });
    }
    
    /**
     * 技能等级重置
     * @param id 卡牌_id
     * @returns 
     */
    public static async Task SkillResetAsync(int id)
    {
        var param = new JsonData
        {
            ["id"] = id,
        };
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "hero/skillreset", param, isAutoShowReward: DisplayType.Show);
    }

    /** 战力上报 */
    public static async void ReportPowerAsync(int heroId, int power)
    {
        var param = new JsonData
        {
            ["id"] = heroId,
            ["power"] = power,
        };
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "hero/power", param);
    }
}