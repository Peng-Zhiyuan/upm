using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class DialogService : Service
{
    public override void OnCreate()
    {
        DialogManager.OnProcessDialogTaskAsync = OnProcessDialogTask;
        DialogManager.OnGetUid = OnGetUrl;
    }

    static string OnGetUrl()
    {
        var myUid = Database.Stuff.roleDatabase.Me._id;
        return myUid;
    }

    static async Task<DialogResult> OnProcessDialogTask(DialogTask info)
    {
        var style = info.style;

        // 只能在运行时显示
        if (!Application.isPlaying)
        {
            var result = new DialogResult();
            result.choice = true;
            return result;
        }

        // 如果没有资源，那就不显示了，默认选择了 true
        var avaliable = BucketManager.Stuff.Main.IsAddressAquired($"{nameof(DialogFloating)}.prefab");
        if (!avaliable)
        {
            var result = new DialogResult();
            result.choice = true;
            return result;
        }



        if (style == DialogStyle.ComfirmCancel)
        {
            var result = await ProcessConfirmCancelStyleAsync(info);
            return result;

        }
        else if (style == DialogStyle.ConfirmOnly)
        {
            var result = await ProcessConfirmOnlyStyleAsync(info);
            return result;
        }
        else if (style == DialogStyle.ExceptionReport)
        {
            var result = await ProcessExceptionStyle(info);
            return result;
        }
        else if (style == DialogStyle.NoButton)
        {
            var result = await ProcessNoButtonStyleAsync(info);
            return result;
        }
        else if(style == DialogStyle.Ad)
        {
            var result = await ProcessAdStyleAsync(info);
            return result;
        }
        else
        {
            throw new Exception($"[DialogService] unsupported dialog type '{style}'");
        }


    }

    static Task<DialogResult> ProcessAdStyleAsync(DialogTask info)
    {
        var tcs = new TaskCompletionSource<DialogResult>();

        var param = info.param;
        var contentType = (AdPage.ContentType)param;
        UIEngine.Stuff.ForwardOrBackTo<AdPage>(contentType);
        UIEngine.Stuff.HookBack(nameof(AdPage), () =>
        {
            var isSilent = AdPage.out_isSilentChecked;
            var choice = AdPage.out_choice;
            var result = new DialogResult();
            result.isSilentChcked = isSilent;
            result.choice = choice;
            tcs.SetResult(result);
        });

        return tcs.Task;
    }

    static Task<DialogResult> ProcessConfirmCancelStyleAsync(DialogTask info)
    {
        var tcs = new TaskCompletionSource<DialogResult>();
        var floating = UIEngine.Stuff.ShowFloatingImediatly<DialogFloating>(null, UILayer.GlobalDialogLayer);
        floating.HideAllButtonGroup();
        floating.IsConfirmCancelGroupVisible = true;

        floating.Title = info.title;
        floating.Content = info.content;
        floating.onClick = (intent) =>
        {
            UIEngine.Stuff.RemoveFloating<DialogFloating>();
            if (intent == true)
            {
                var result = new DialogResult();
                result.choice = true;
                tcs.SetResult(result);
            }
            else
            {
                var result = new DialogResult();
                result.choice = false;
                tcs.SetResult(result);
            }
        };

        return tcs.Task;
    }

    static Task<DialogResult> ProcessConfirmOnlyStyleAsync(DialogTask info)
    {
        var tcs = new TaskCompletionSource<DialogResult>();
        var floating = UIEngine.Stuff.ShowFloatingImediatly<DialogFloating>();
        floating.HideAllButtonGroup();
        floating.IsConfirmOnlyGroupVisible = true;

        floating.Title = info.title;
        floating.Content = info.content;
        floating.onClick = (intent) =>
        {
            UIEngine.Stuff.RemoveFloating<DialogFloating>();
            if (intent == true)
            {
                var result = new DialogResult();
                result.choice = true;
                tcs.SetResult(result);
            }
            else
            {
                var result = new DialogResult();
                result.choice = false;
                tcs.SetResult(result);
            }
        };

        return tcs.Task;
    }

    static Task<DialogResult> ProcessNoButtonStyleAsync(DialogTask info)
    {
        var tcs = new TaskCompletionSource<DialogResult>();
        var floating = UIEngine.Stuff.ShowFloatingImediatly<DialogFloating>();
        floating.HideAllButtonGroup();
        floating.IsNoButtonGroupVisible = true;

        floating.Title = info.title;
        floating.Content = info.content;
        floating.onClick = (intent) =>
        {
            if (tcs == null) return;
            UIEngine.Stuff.RemoveFloating<DialogFloating>();
            if (intent == true)
            {
                var result = new DialogResult();
                result.choice = true;
                tcs.SetResult(result);
            }
            else
            {
                var result = new DialogResult();
                result.choice = false;
                tcs.SetResult(result);
            }
            tcs = null;
        };

        return tcs.Task;
    }

    static Task<DialogResult> ProcessExceptionStyle(DialogTask info)
    {
        var tcs = new TaskCompletionSource<DialogResult>();
        var floating = UIEngine.Stuff.ShowFloatingImediatly<ExceptionFloating>();
        var exceptionData = info.exceptionReportData;
        floating.TypeName = exceptionData.type;
        floating.msg = exceptionData.description;
        floating.trace = exceptionData.trace;

        floating.onClick = () =>
        {
            var result = new DialogResult();
            result.choice = true;
            tcs.SetResult(result);
        };
        return tcs.Task;
    }
}
