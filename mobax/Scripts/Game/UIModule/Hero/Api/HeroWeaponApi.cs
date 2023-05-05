using System.Collections.Generic;
using System.Threading.Tasks;
using CustomLitJson;

public static class HeroWeaponApi
{
    /**
     * 武器升级
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
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "weapon/lv", param);
        // TrackManager.LevelUp(id.ToString(), to);
    }
    
    /**
     * 武器等级重置
     * @param id 卡牌_id
     * @returns 
     */
    public static async Task LevelResetAsync(int id)
    {
        var param = new JsonData
        {
            ["id"] = id,
        };
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "weapon/lvreset", param, isAutoShowReward: DisplayType.Show);
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
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "weapon/break", param);
        
        // 打点
        // TrackManager.CustomReport("Advance", "hero", "" + id, "level", "" + id);
    }
}