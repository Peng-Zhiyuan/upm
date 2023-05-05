using System.Threading.Tasks;
using System.Collections.Generic;
using CustomLitJson;
using System.Threading.Tasks;
using System;
using CustomLitJson;

public abstract class Channel
{
    
    public abstract string Alias
    {
        get;
    }

    public virtual Task<UserPlatformInfo> LoginAsync(string branch = null)
    {
        throw new Exception("[Channel] Not implement");
    }

    public async virtual Task OnLoginStartAsync()
    {
        
    }

    public virtual void OnLoginComplete()
    {

    }

    public virtual Task PurchaseAsync(int itemId)
    {
        throw new Exception("[Channel] Purchase not implement ");
    }
}
