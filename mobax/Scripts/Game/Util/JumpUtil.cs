using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class JumpId
{
    public const int GuildMainPage = 31;
    public const int SDKDiscord = 100;
    public const int SDKQuestion = 101;
}

public static class JumpUtil
{
    public static async void Jump(JumpInfo jumpInfo)
    {
        var functionId = jumpInfo.Id;
        if (!SystemOpenManager.Stuff.CheckSystemOpen(functionId, true)) return;
        var param = jumpInfo.Parm;
        Debug.Log($"[JumpUtil] jump [{functionId} {param}]");

        // pzy：
        // 101 是问卷
        if (functionId == JumpId.SDKQuestion)
        {
            if (!Database.Stuff.activeDatabase.HasFlag(StrBuild.Instance.ToStringAppend("zx01"), 0))
            {
                ReserchManager.Stuff.ShowReserchIfNeed(true);
            }
            return;
        }
        if (functionId == JumpId.SDKDiscord)
        {
            if (!Database.Stuff.activeDatabase.HasFlag(StrBuild.Instance.ToStringAppend("zx01"), 1))
            {
                var ret = await ActiveApi.DiscardAsync();
                Database.Stuff.activeDatabase.Add(ret);
            }
            return;
        }
        if (functionId == JumpId.GuildMainPage)
        {
            await GuildManager.Stuff.OpenGuild();
            return;
        }
        var funcRow = StaticData.SystemOpenTable[functionId];
        var pageName = funcRow.Page;
        if (string.IsNullOrEmpty(pageName))
        {
            Debug.LogError("Cant find the page name from SystemOpen Table");
            return;
        }
        var pageArg = jumpInfo.Parm;
        await UiUtil.NavigateAsync(pageName, pageArg);
        //if (funcRow.mainGroup == 1)
        //{
        //    UiUtil.BackToMainGroupThenReplace(pageName, pageArg);
        //}
        //else
        //{
        //    UIEngine.Stuff.ForwardOrBackTo(pageName, pageArg);
        //}
    }
}