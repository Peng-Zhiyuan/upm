using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class BlockService : Service
{
    public override void OnCreate()
    {
        BlockManager.RequestChangeBlockStatus = OnRequestChangeBlockStatus;
        BlockManager.RequestTaskToWaitTransacationStay = WaitTransactionStayAsync;
    }

    async Task WaitTransactionStayAsync()
    {
        var f = UIEngine.Stuff.FindFloating<BlockFloating>();
        await f.WaitTansactionStayState();
    }

    void OnRequestChangeBlockStatus(BlockLevel? blockLevel)
    {
        var f = UIEngine.Stuff.FindFloating<BlockFloating>();
        if(f == null)
        {
            f = UIEngine.Stuff.ShowFloatingImediatly<BlockFloating>();
        }
        f.Level = blockLevel;
    }
}
