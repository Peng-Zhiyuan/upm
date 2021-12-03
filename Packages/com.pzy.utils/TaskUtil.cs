using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using System;
using UnityEngine.Scripting;


public static class TaskUtil
{
    /// <summary>
    /// 等待一个 task，用回调的方式传回结果（如果有）
    /// </summary>
    /// <param name="task"></param>
    /// <param name="action"></param>
    [Preserve]
    public static async void Await(Task task, Action<object> action)
    {
        await task;
        if(action != null)
        {
            var genericTypeDefinition = task.GetType().GetGenericTypeDefinition();
            if (genericTypeDefinition != null && genericTypeDefinition == typeof(Task<>))
            {
                var resultProperty = task.GetType().GetProperty("Result");
                var result = resultProperty.GetValue(task);
                action?.Invoke(result);
            }
            else
            {
                action?.Invoke(null);
            }
            
        }
    }
}
