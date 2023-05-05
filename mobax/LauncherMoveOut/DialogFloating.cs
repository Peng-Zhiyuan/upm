using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public partial class DialogFloating : Floating
{
    public string Title
    {
        set
        {
            this.Text_title.text = value;
        }
    }

    public string Content
    {
        set
        {
            this.Text_content.text = value;
        }
    }

    public bool IsConfirmCancelGroupVisible
    {
        set
        {
            this.Group_ConfirmCancel.gameObject.SetActive(value);
        }
    }

    public bool IsConfirmOnlyGroupVisible
    {
        set
        {
            this.Group_ConfirmOnly.gameObject.SetActive(value);
        }
    }



    public bool IsNoButtonGroupVisible
    {
        set
        {
            this.Group_NoButton.gameObject.SetActive(value);
        }
    }

    public void HideAllButtonGroup()
    {
        this.IsConfirmCancelGroupVisible = false;
        this.IsConfirmOnlyGroupVisible = false;
        this.IsNoButtonGroupVisible = false;
    }


    public Action<bool> onClick;
    public void OnButton(string msg)
    {
        if(msg == "ok")
        {
            var temp = onClick;
            onClick = null;
            temp?.Invoke(true);

        }
        else if(msg == "cancel")
        {
            var temp = onClick;
            onClick = null;
            temp?.Invoke(false);
        }
    }

    protected override Task LogicBackAsync()
    {
        this.OnButton("cancel");
        var tcs = new TaskCompletionSource<bool>();
        tcs.SetResult(true);
        return tcs.Task;
    }
}
