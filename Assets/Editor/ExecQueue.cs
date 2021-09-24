using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public static class ExecQueue
{
    static Queue<ExecJob> queue = new Queue<ExecJob>();

    public static Task<ExecResult> ExecInQueue(string exec, string param)
    {
        var tcs = new TaskCompletionSource<ExecResult>();
        Enqueue(exec, param, (result) =>
        {
            tcs.SetResult(result);
        });
        return tcs.Task;
    }

    public static void Enqueue(string exec, string param, Action<ExecResult> onResult)
    {
        var job = new ExecJob();
        job.exec = exec;
        job.param = param;
        job.onResult = onResult;

        queue.Enqueue(job);

        StartProcessIfNeed();
    }


    static bool isProceesing;
    static async void StartProcessIfNeed()
    {
        if(isProceesing)
        {
            return;
        }
        start:
        var count = queue.Count;
        if(count == 0)
        {
            isProceesing = false;
            return;
        }
        isProceesing = true;
        var job = queue.Dequeue();
        var exec = job.exec;
        var param = job.param;
        var result = await Exec.RunGetOutput(exec, param);
        job.onResult.Invoke(result);
        goto start;

    }


}

public class ExecJob
{
    public string exec;
    public string param;
    public Action<ExecResult> onResult;
}