using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Sirenix.OdinInspector;

public class Floating : UIEngineElement
{
    public UILayer defaultLayer = UILayer.NormalFloatingLayer;

    [BoxGroup("GlobalBack")]
    public bool globalBackTarget = false;

    public virtual async Task OnShowPreperAsync()
    {
        
    }

    public virtual void OnShow(object param){}


    public virtual void OnHide(){}

    public async void Remove()
    {
        await UIEngine.Stuff.RemoveFloatingAsync(this.name);
    }

    protected override Task LogicBackAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        tcs.SetResult(true);
        return tcs.Task;
    }
}