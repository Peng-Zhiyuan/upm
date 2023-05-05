using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class TaskRunner 
{
    List<Task> taskList = new List<Task>();

    public void Add(Task task)
    {
        RunTaskInBackground(task);
    }

    public int total;
    public int completeCount;
    public Exception exception;
    public bool isError;
    async void RunTaskInBackground(Task task)
    {
        total++;
        try
        {
            await task;
        }
        catch(Exception e)
        {
            this.exception = e;
            this.isError = true;
            this.SetResultIfNeed();
            return;
        }
        this.completeCount++;
        this.ReportProcessIfNeed();
        this.SetResultIfNeed();
    }


    void ReportProcessIfNeed()
    {
        if(total != 0)
        {
            this.percentHandler?.Invoke(completeCount / (float)total);
        }
        else
        {
            this.percentHandler?.Invoke(1f);
        }
    }

    TaskCompletionSource<bool> tcs; 
    Action<float> percentHandler;
    public Task WhenAll(Action<float> percentHandler)
    {
        this.percentHandler = percentHandler;
        if (this.tcs != null)
        {
            return this.tcs.Task;
        }
        tcs = new TaskCompletionSource<bool>();
        this.SetResultIfNeed();
        return tcs.Task;
    }

    void SetResultIfNeed()
    {
        if(this.total == this.completeCount)
        {
            this.tcs?.TrySetResult(true);
            this.tcs = null;
        }
        else if(this.isError)
        {
            this.tcs?.TrySetException(this.exception);
            this.tcs = null;
        }
    }

}
