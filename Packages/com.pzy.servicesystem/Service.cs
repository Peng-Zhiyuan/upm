using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class Service
{
    public bool enabled = true;

    public virtual void OnCreate()
    {
        
    }
    public virtual void Handle(ServiceMessage msg)
    {

    }

    public virtual Task HandleAsync(ServiceMessage asyncMessage)
    {
        var tcs = new TaskCompletionSource<bool>();
        tcs.SetResult(true);
        return tcs.Task;
    }


}
