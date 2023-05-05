using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomLitJson;
using System;
using System.Threading.Tasks;

public static class ApiUtil
{
    /// <summary>
    /// 分页获取，已超级优化
    /// 内部会并发处理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="apiDelegate"></param>
    /// <param name="pageSize">每页的大小，0 表示由服务器决定</param>
    /// <param name="recordCount">最大数据条目, 0 表示获取所有</param>
    /// <returns></returns>
    public static async Task<List<T>> FetchByPage<T>(Func<JsonData, Task<NetPage<T>>> apiDelegate, int pageSize = 0, int recordCount = 0, JsonData extraArg = null)
    {
        var ret = new List<T>();
        var jd = new JsonData();
        jd["page"] = 1;
        jd["size"] = pageSize;
        jd["record"] = recordCount;
        if (extraArg != null)
        {
            var dic = extraArg.ToDictionary();
            foreach (var kv in dic)
            {
                var key = kv.Key;
                var value = kv.Value;
                jd[key] = value;
            }
        }
        var netPage = await apiDelegate.Invoke(jd);
        var itemInfoList = netPage.rows;
        if (itemInfoList != null)
        {
            ret.AddRange(itemInfoList);
        }
        var totalPageCount = netPage.total;
        var pageSize2 = netPage.size;
        var record = netPage.record;
        var update = netPage.update;
        var startIndex = 2;
        var taskList = new List<Task<NetPage<T>>>();
        for (int i = startIndex; i <= totalPageCount; i++)
        {
            var jd2 = new JsonData();
            jd2["page"] = i;
            jd2["size"] = pageSize2;
            jd2["record"] = record;
            jd2["update"] = update;
            if (extraArg != null)
            {
                var dic = extraArg.ToDictionary();
                foreach (var kv in dic)
                {
                    var key = kv.Key;
                    var value = kv.Value;
                    jd2[key] = value;
                }
            }
            var task = apiDelegate.Invoke(jd2);
            taskList.Add(task);
        }
        
        
        await Task.WhenAll(taskList);
        foreach (var task in taskList)
        {
            var netPage2 = task.Result;
            var infoList = netPage2.rows;
            if (infoList != null)
            {
                ret.AddRange(infoList);
            }
        }
        return ret;
    }
}