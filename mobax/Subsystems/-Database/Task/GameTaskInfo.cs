using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 之所以叫做 GameTask，是为了和 C# 的 Task 区分
/// </summary>
public class GameTaskInfo 
{

    // 如果是链式任务，是任务链 id
    // 否则是任务 id 本身
    public int key;

    // 如果是链式任务，是上一次提交完成的任务 id
    // 否则是任务 id 本身
    public int val;

    public long expire;


    public bool HasExpire
    {
        get
        {
            if(expire == 0)
            {
                return false;
            }
            return true;
        }
    }
}
