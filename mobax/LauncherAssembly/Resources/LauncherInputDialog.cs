using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public partial class LauncherInputDialog : MonoBehaviour
{

    TaskCompletionSource<(bool, string)> tcs;
    public Task<(bool b, string text)> WaitCompleteAsync()
    {
        this.tcs = new TaskCompletionSource<(bool, string)>();
        return this.tcs.Task;
    }

    public void OnButton(string msg)
    {
        if (msg == "ok")
        {
            LauncherUiManager.Stuff.Remove<LauncherInputDialog>();
            this.tcs.SetResult((true, this.Input.text));

        }
        else if (msg == "cancel")
        {
            LauncherUiManager.Stuff.Remove<LauncherInputDialog>();
            this.tcs.SetResult((false, this.Input.text));
        }
    }

}
