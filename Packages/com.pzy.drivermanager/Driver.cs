using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class Driver
{
    public virtual void Handle(string msg)
    {

    }

    public virtual Task HandleAsync(string asyncMessage)
    {
        var tcs = new TaskCompletionSource<bool>();
        tcs.SetResult(true);
        return tcs.Task;
    }


}
