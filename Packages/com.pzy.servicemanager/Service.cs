using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class Service
{
    public virtual void OnCreate()
    {
        
    }
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
