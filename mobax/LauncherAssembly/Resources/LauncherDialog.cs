using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public partial class LauncherDialog : MonoBehaviour
{
    private void OnEnable()
    {
        ConfirmText.text = LauncherLocalizatioManager.Get("confirm");
        ConfirmOnlyText.text = LauncherLocalizatioManager.Get("confirm");
        CancelText.text = LauncherLocalizatioManager.Get("cancel");
    }
    public void Set(string content, bool onlyConfirm, string tip = "")
    {
        if(onlyConfirm)
        {
            this.Group_ConfirmCancel.gameObject.SetActive(false);
            this.Group_ConfirmOnly.gameObject.SetActive(true);
        }
        else
        {
            this.Group_ConfirmCancel.gameObject.SetActive(true);
            this.Group_ConfirmOnly.gameObject.SetActive(false);
        }

        this.Text_content.text = content;
        this.Text_tip.text = tip;
    }

    TaskCompletionSource<bool> tcs;
    public Task<bool> WaitCompleteAsync()
    {
        this.tcs = new TaskCompletionSource<bool>();
        return this.tcs.Task;
    }

    public void OnButton(string msg)
    {
        if(msg == "ok")
        {
            LauncherUiManager.Stuff.Remove<LauncherDialog>();
            this.tcs.SetResult(true);

        }
        else if(msg == "cancel")
        {
            LauncherUiManager.Stuff.Remove<LauncherDialog>();
            this.tcs.SetResult(false);
        }
    }

}
