using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;


public static class DialogManager
{
    private static Queue<DialogTask> dialogTaskQueue = new Queue<DialogTask>();

    public static Func<DialogTask, Task<DialogResult>> OnProcessDialogTaskAsync;
    public static Func<string> OnGetUid;

    private static string Uid
    {
        get
        {
            if (OnGetUid == null)
            {
                throw new Exception("[DialogManager] OnGetUid not set yet!");
            }

            var uid = OnGetUid.Invoke();
            return uid;
        }
    }

    static void SetSilentValue(string key, bool value)
    {
        PlayerPrefsUtil.SetBool($"silent.{key}.{Uid}", value);
    }

    static void SetSilentTimestampValue(string key, long timestamp)
    {
        PlayerPrefs.SetString($"silent.{key}.{Uid}.timestamp", timestamp.ToString());
    }

    static bool GetSilentValue(string key)
    {
        var ret = PlayerPrefsUtil.GetBool($"silent.{key}.{Uid}", false);
        return ret;
    }

    static long GetSilentTimestampValue(string key)
    {
        var str = PlayerPrefs.GetString($"silent.{key}.{Uid}.timestamp", "0");
        var timestamp = long.Parse(str);
        return timestamp;
    }

    static void SetSilent(string key)
    {
        SetSilentValue(key, true);
        var timestam = Clock.TimestampSec;
        SetSilentTimestampValue(key, timestam);
    }

    static bool IsSilent(string key)
    {
        try
        {
            var b = GetSilentValue(key);
            if (b)
            {
                var timestamp = GetSilentTimestampValue(key);
                var nowTimestamp = Clock.TimestampSec;
                var deltaSec = nowTimestamp - timestamp;
                var timespan = TimeSpan.FromSeconds(deltaSec);
                var totalDays = timespan.TotalDays;
                if (totalDays < 1)
                {
                    return true;
                }
            }

            return false;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return false;
        }
    }





    public static Task<DialogResult> ShowAsync(DialogTask dialogInfo)
    {
        var tcs = new TaskCompletionSource<DialogResult>();

        var silentKey = dialogInfo.silentKey;
        if(!string.IsNullOrEmpty(silentKey))
        {
            var isSilent = IsSilent(silentKey);
            if (isSilent)
            {
                var result = new DialogResult();
                result.choice = false;
                tcs.SetResult(result);
                return tcs.Task;
            }
        }


  
        dialogInfo.onResult = (result) =>
        {
            if (!string.IsNullOrEmpty(silentKey))
            {
                var isSilentChecked = result.isSilentChcked;

                if (isSilentChecked)
                {
                    SetSilent(silentKey);
                }
            }
            tcs.SetResult(result); 
        };
        dialogTaskQueue.Enqueue(dialogInfo);
        ShowAllDialogInfoSingleAsync();
        return tcs.Task;
    }


    public static bool IsInConversation
    {
        get
        {
            var b = task != null;
            return b;
        }
    }


    static Task task;

    private static async void ShowAllDialogInfoSingleAsync()
    {
        if (task == null)
        {
            task = ShowAllDialogInfoAsync();
        }

        await task;
        task = null;
    }

    private static async Task ShowAllDialogInfoAsync()
    {
        while (dialogTaskQueue.Count > 0)
        {
            var dialogTask = dialogTaskQueue.Dequeue();
            var dialogResult = await ProcessDialogTaskAsync(dialogTask);
            dialogTask.onResult(dialogResult);
        }
    }


    private static async Task<DialogResult> ProcessDialogTaskAsync(DialogTask dialogTaskInfo)
    {
        if (OnProcessDialogTaskAsync == null)
        {
            //throw new Exception("[DialogManager] OnProcssDialogTakAsync not set yet!");
            Debug.LogError("[DialogManager] OnProcssDialogTakAsync not set yet! default returns true");
            var result2 = new DialogResult();
            result2.choice = true;
            return result2;
        }

        var result = await OnProcessDialogTaskAsync.Invoke(dialogTaskInfo);
        return result;
    }
}