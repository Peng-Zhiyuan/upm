using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using CustomLitJson;

public class DailyApi 
{
    public static async Task<List<DailyInfo>> RequestAsync(int startDateIndex)
    {
        //vi8i1s2i4i8
        // eg: [{"_id":"1848kx4xj92da","uid":"1848kx4","time":20220330,"Login":9},{"_id":"1848kx4xj92d9","uid":"1848kx4","time":20220329,"Login":24},{"_id":"1848kx4xj92d8","uid":"1848kx4","time":20220328,"Login":2},{"_id":"1848kx4xj92d5","uid":"1848kx4","time":20220325,"Login":40},{"_id":"1848kx4xj92d4","uid":"1848kx4","time":20220324,"Login":47},{"_id":"1848kx4xj92d3","uid":"1848kx4","time":20220323,"Login":7}]
        //"id, val, time"
        var jd = new JsonData();
        jd["time"] = startDateIndex;
        var data = await NetworkManager.Stuff.CallAsync<List<DailyInfo>>(ServerType.Game, "getter/daily", jd);
        return data;
    }
}
