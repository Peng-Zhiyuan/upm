using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public static partial class Dialog
{

    /// <summary>
    /// 确认对话框，但是没有按钮，点击任何地方关闭
    /// </summary>
    /// <param name="systemId"></param>
    /// <returns></returns>
    public static async Task SystemOpenAsync(int systemId)
    {
        var info = new DialogTask();
        info.style = DialogStyle.SystemOpen;
        info.systemId = systemId;
        await DialogManager.ShowAsync(info);
    }

    /// <summary>
    /// 广告
    /// </summary>
    /// <param name="systemId"></param>
    /// <returns></returns>
    public static async Task<bool> AdAsync(AdPage.ContentType contentType)
    {
        var info = new DialogTask();
        info.style = DialogStyle.Ad;
        info.param = contentType;
        info.silentKey = contentType.ToString();
        var result = await DialogManager.ShowAsync(info);
        var choice = result.choice;
        return choice;
    }
}
