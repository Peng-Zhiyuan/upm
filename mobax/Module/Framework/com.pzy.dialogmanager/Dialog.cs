using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public static partial class Dialog 
{
    /// <summary>
    /// 询问对话框，有确认和取消两个按钮
    /// </summary>
    /// <param name="title"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static async Task<bool> AskAsync(string title, string content)
    {
        var info = new DialogTask();
        info.style = DialogStyle.ComfirmCancel;
        info.title = title;
        info.content = content;
        var result = await DialogManager.ShowAsync(info);
        return result.choice;
    }

    /// <summary>
    /// 确认对话框，只会有 confirm 按钮
    /// </summary>
    /// <param name="title"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static async Task ConfirmAsync(string title, string content)
    {
        var info = new DialogTask();
        info.style = DialogStyle.ConfirmOnly;
        info.title = title;
        info.content = content;
        await DialogManager.ShowAsync(info);
    }

    public static async void Confirm(string title, string content)
    {
        await ConfirmAsync(title, content);
    }


    /// <summary>
    /// 确认对话框，但是没有按钮，点击任何地方关闭
    /// </summary>
    /// <param name="title"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static async Task ConfirmWithNoButton(string title, string content)
    {
        var info = new DialogTask();
        info.style = DialogStyle.NoButton;
        info.title = title;
        info.content = content;
        await DialogManager.ShowAsync(info);
    }

    
    public static async Task ReportExceptionAsync(ExceptionReportData data)
    {
        var dialogTask = new DialogTask();
        dialogTask.style = DialogStyle.ExceptionReport;
        dialogTask.exceptionReportData = data;
        await DialogManager.ShowAsync(dialogTask);
    }

}
