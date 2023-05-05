using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using CustomLitJson;

public static class TrackApi
{
    static JsonData DictionaryToJsonData<T>(Dictionary<string, T> dic)
    {
        var paramdDicJd = new JsonData();
        foreach (var kv in dic)
        {
            var key = kv.Key;
            var value = kv.Value;
            if(typeof(T) == typeof(string))
            {
                var stringValue = value as string;
                paramdDicJd[key] = stringValue;
            }
            else if(typeof(T) == typeof(int))
            {
                var intValue = (int)(object)value;
                paramdDicJd[key] = intValue;
            }
            else
            {
                throw new System.Exception("unsuuport");
            }
        }
        return paramdDicJd;
    }

    public static async void StatisticsInBackground(string eventName, Dictionary<string, int> intDic = null, Dictionary<string, string> stringDic = null)
    {
        var jd = new JsonData();
        jd["k"] = eventName;
        if (intDic?.Count > 0)
        {
            var param = DictionaryToJsonData(intDic);
            jd["v"] = param;
        }
        if (stringDic?.Count > 0)
        {
            var param = DictionaryToJsonData(stringDic);
            jd["attach"] = param;
        }
        if (LoginManager.Stuff.session != null)
        {
            await NetworkManager.Stuff.CallAsync<NetPage<ItemInfo>>(ServerType.Game, "statistics", jd, null, false);
        }
    }
}