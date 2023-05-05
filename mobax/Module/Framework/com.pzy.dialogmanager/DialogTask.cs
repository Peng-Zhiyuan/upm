using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

public class DialogTask
{
	public DialogStyle style;

	public string content;

	public string title;

	// ExceptionReport 类型专用
	public ExceptionReportData exceptionReportData;

	// systemOpen 类型专用
	public int systemId;

	/// <summary>
	/// 如果填写，表示启动今日不再显示逻辑
	/// </summary>
	public string silentKey;


	/// <summary>
	/// 内部使用字段，传入时不需要填写
	/// 以后或许会移除此字段
	/// </summary>
	public Action<DialogResult> onResult;

	public object param;
}