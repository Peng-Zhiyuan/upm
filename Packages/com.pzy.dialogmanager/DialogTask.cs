using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

public class DialogTask
{
	public DialogStyle style;
	//public TaskCompletionSource<DialogResult> tcs;
	public string content;

	// ExceptionReport 类型专用
	public ExceptionReportData exceptionReportData;

	public string silentKey;

	public object outputTcs;

	public Action<DialogResult> onResult;
}