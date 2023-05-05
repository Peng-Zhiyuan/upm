using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using CustomLitJson;

public static class VipApi 
{
    /// <summary>
    /// 被邀请人请求bpInfo
    /// </summary>
    /// <returns></returns>
    public static async Task<BPInfo> SubmitAsync(int vipId)
    {
        var jd = new JsonData();
        jd["id"] = vipId;
        var ret = await NetworkManager.Stuff.CallAsync<BPInfo>(ServerType.Game, "business/viper/submit", jd, isAutoShowReward: DisplayType.Show);
        return ret;
    }
}
