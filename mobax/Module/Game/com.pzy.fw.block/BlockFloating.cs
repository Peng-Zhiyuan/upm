using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Threading.Tasks;

public partial class BlockFloating : Floating
{
    public BlockLevel? Level
    {
        get
        {
            var v = this.Animator.GetInteger("level");
            if(v == 0)
            {
                return null;
            }
            else
            {
                return (BlockLevel)v;
            }
        }
        set
        {
            if(value != null)
            {
                this.Animator.SetInteger("level", (int)value);
            }
            else
            {
                this.Animator.SetInteger("level", 0);
            }
        }
    }

    public List<TaskCompletionSource<bool>> tcsList = new List<TaskCompletionSource<bool>>();

    public void OnAnimatorEnterTransactionStayState()
    {
        foreach(var tcs in tcsList)
        {
            try
            {
                tcs.SetResult(true);
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }
        tcsList.Clear();
    }

    public Task WaitTansactionStayState()
    {
        var tcs = new TaskCompletionSource<bool>();
        if(this.Animator.GetCurrentAnimatorStateInfo(0).IsName("transaction_stay"))
        {
            tcs.SetResult(true);
            return tcs.Task;
        }
        else
        {
            tcsList.Add(tcs);
            return tcs.Task;
        }
    }
}

