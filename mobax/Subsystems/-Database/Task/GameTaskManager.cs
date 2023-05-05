using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTaskManager
{
    static Dictionary<int, List<int>> chainIdToTaskIdList = null;

    public static void BuildCacheIfNeed()
    {
        if(chainIdToTaskIdList == null)
        {
            chainIdToTaskIdList = new Dictionary<int, List<int>>();
            var rowList = StaticData.TaskTable.ElementList;
            foreach(var row in rowList)
            {
                var chainId = row.Series;
                if(chainId == 0)
                {
                    continue;
                }
                var list = DictionaryUtil.GetOrCreateList(chainIdToTaskIdList, chainId);
                var taskId = row.Id;
                list.Add(taskId);
            }
        }
    }

    public static int GetNextTaskInChain(int taskId)
    {
        BuildCacheIfNeed();
        var taskRow = StaticData.TaskTable[taskId];
        var chainId = taskRow.Series;
        var list = DictionaryUtil.TryGet(chainIdToTaskIdList, chainId, null);
        if(list == null)
        {
            throw new System.Exception($"[GameTaskManager] chainId '{chainId}' not validate");
        }
        var targetIndex = -1;
        for(int i = 0; i < list.Count - 1; i++)
        {
            var one = list[i];
            if(one == taskId)
            {
                targetIndex = i + 1;
                break;
            }
        }
        if(targetIndex == -1)
        {
            return 0;
        }
        var nextTaskId = list[targetIndex];
        return nextTaskId;
    }
}
