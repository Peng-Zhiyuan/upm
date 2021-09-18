using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;



public static class DialogManager
{
	private static Queue<DialogTask> dialogTaskQueue = new Queue<DialogTask> ();

	public static Func<DialogTask, Task<DialogResult>> OnProcessDialogTaskAsync;
	public static Func<string> OnGetUid;

	private static string Uid
    {
		get
        {
			if(OnGetUid == null)
            {
				throw new Exception("[DialogManager] OnGetUid not set yet!");
            }

			var uid = OnGetUid.Invoke();
			return uid;


        }
    }

	private static void SetSilentValue(string key, bool value)
    {
		PlayerPrefsUtil.SetBool($"silent.{key}.{Uid}" , value);
	}

	private static void SetSilentTimestampValue(string key, long timestamp)
	{
		PlayerPrefs.SetString($"silent.{key}.{Uid}.timestamp", timestamp.ToString());
	}

	private static bool GetSilentValue(string key)
	{
		var ret = PlayerPrefsUtil.GetBool($"silent.{key}.{Uid}", false);
		return ret;
	}

	private static long GetSilentTimestampValue(string key)
	{
		var str = PlayerPrefs.GetString($"silent.{key}.{Uid}.timestamp", "0");
		var timestamp = long.Parse(str);
		return timestamp;
	}

	private static void SetSilent(string key)
    {
		SetSilentValue(key, true);
		var timestam = DateManager.Stuff.TimestampSec;
		SetSilentTimestampValue(key, timestam);
	}

	private static bool IsSilent(string key)
    {
		try
        {
			var b = GetSilentValue(key);
			if (b)
			{
				var timestamp = GetSilentTimestampValue(key);
				var nowTimestamp = DateManager.Stuff.TimestampSec;
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
		catch(Exception e)
        {
			Debug.LogError(e.Message);
			return false;
        }
    }


	public static Task<bool> SlientableAskAsync(string silentKey, string content)
	{
		var isSilent = IsSilent(silentKey);
		if(isSilent)
        {
			var tcs2 = new TaskCompletionSource<bool>();
			tcs2.SetResult(true);
			return tcs2.Task;
		}

		var dialogTask = new DialogTask();
		dialogTask.style = DialogStyle.SilentableAsk;
		dialogTask.content = content;
		dialogTask.silentKey = silentKey;
		var tcs = new TaskCompletionSource<bool>();
		dialogTask.outputTcs = tcs;
		dialogTask.onResult = (result) =>
		{
			var b = result.choice;
			var isSilentChecked = result.isSilentChcked;

			if(isSilentChecked)
            {
				SetSilent(silentKey);
            }

			tcs.SetResult(b);
		};
		dialogTaskQueue.Enqueue(dialogTask);
		ShowAllDialogInfoSingleAsync();
		return tcs.Task;
	}

	public static Task<bool> AskAsync(string content)
    {
		var dialogTask = new DialogTask();
		dialogTask.style = DialogStyle.ComfirmCancel;
		dialogTask.content = content;
		var tcs = new TaskCompletionSource<bool>();
		dialogTask.outputTcs = tcs;
		dialogTask.onResult = (result) =>
		{
			var b = result.choice;
			tcs.SetResult(b);
		};
		dialogTaskQueue.Enqueue(dialogTask);
		ShowAllDialogInfoSingleAsync();
		return tcs.Task;
	}

	/// <summary>
	/// 推销购买的对话框，会包含 confirm 和 cancel 两个按钮
	/// </summary>
	/// <param name="content">内容</param>
	/// <returns></returns>
	public static Task<bool> AskToByAsync(string content)
	{
		var dialogTask = new DialogTask();
		dialogTask.style = DialogStyle.AskToBy;
		dialogTask.content = content;
		var tcs = new TaskCompletionSource<bool>();
		dialogTask.outputTcs = tcs;
		dialogTask.onResult = (result) =>
		{
			var b = result.choice;
			tcs.SetResult(b);
		};
		dialogTaskQueue.Enqueue(dialogTask); 
		ShowAllDialogInfoSingleAsync();
		return tcs.Task;
	}

	/// <summary>
	/// 确认对话框，只会有 confirm 按钮
	/// </summary>
	/// <param name="title"></param>
	/// <param name="content"></param>
	/// <returns></returns>
    public static Task ConfirmAsync(string content)
	{
		//Debug.Log($"[DialogManager] show {content}");
		var dialogTask = new DialogTask();
		dialogTask.style = DialogStyle.ConfirmOnly;
		dialogTask.content = content;
		var tcs = new TaskCompletionSource<bool>();
		dialogTask.outputTcs = tcs;
		dialogTask.onResult = (result) =>
		{
			var b = result.choice;
			tcs.SetResult(b);
		};
		dialogTaskQueue.Enqueue(dialogTask); 
		ShowAllDialogInfoSingleAsync();
		return tcs.Task;
	}

	public static Task ReportException(ExceptionReportData data)
    {
		var dialogTask = new DialogTask();
		dialogTask.style = DialogStyle.ExceptionReport;
		dialogTask.exceptionReportData = data;
		var tcs = new TaskCompletionSource<bool>();
		dialogTask.outputTcs = tcs;
		dialogTask.onResult = (result) =>
		{
			var b = result.choice;
			tcs.SetResult(b);
		};
		dialogTaskQueue.Enqueue(dialogTask);
		ShowAllDialogInfoSingleAsync();
		return tcs.Task;
	}


    public static async void Confirm(string content)
	{
		await ConfirmAsync(content);
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
		if(task == null)
		{
			task = ShowAllDialogInfoAsync();
		}
		await task;
		task = null;
	}

	private static async Task ShowAllDialogInfoAsync()
	{
		while(dialogTaskQueue.Count > 0)
		{
			var dialogTask = dialogTaskQueue.Dequeue();
			var dialogResult = await ProcessDialogTaskAsync(dialogTask);
			dialogTask.onResult(dialogResult);
		}
	}


	private async static Task<DialogResult> ProcessDialogTaskAsync(DialogTask dialogTaskInfo)
	{
		if(OnProcessDialogTaskAsync == null)
        {
			throw new Exception("[DialogManager] OnProcssDialogTakAsync not set yet!");
        }
		var result = await OnProcessDialogTaskAsync.Invoke(dialogTaskInfo);
		return result;
	}

}
