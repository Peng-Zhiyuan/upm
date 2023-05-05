using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Sirenix.OdinInspector;

public class BlockManager : StuffObject<BlockManager>
{
    [ShowInInspector, ReadOnly]
    Dictionary<string, BlockLevel> keyToIsBlockDic = new Dictionary<string, BlockLevel>();

    public static Action<BlockLevel?> RequestChangeBlockStatus;
    public static Func<Task> RequestTaskToWaitTransacationStay;

    public int KeyCount
    {
        get
        {
            return keyToIsBlockDic.Count;
        }
    }

    public async void TransactionIn(string key)
    {
        await TransactionInAsync(key);
    }

    public async Task TransactionInAsync(string key)
    {
        this.AddBlock(key, BlockLevel.Transaction);
        await this.WaitTransactionStayAsync();
    }

    public void TransactionOut(string key)
    {
        this.RemoveBlock(key);
    }

    public async Task WaitTransactionStayAsync()
    {
        await RequestTaskToWaitTransacationStay?.Invoke();
    }

    /// <summary>
    /// 阻塞屏幕输入
    /// </summary>
    /// <param name="key">任意字符串，取消时需要用相同字符串指定</param>
    public void AddBlock(string key, BlockLevel level = BlockLevel.Visible)
    {
        this.keyToIsBlockDic[key] = level;
        this.RefreshFinal();
        //Debug.Log($"[BlockManager] {key} -> {level}, (final: {this.FinalBlockStatus})");
    }

    /// <summary>
    /// 取消阻塞
    /// </summary>
    /// <param name="key">阻塞时输入的键</param>
    public void RemoveBlock(string key)
    {
        this.keyToIsBlockDic.Remove(key);
        this.RefreshFinal();
        //Debug.Log($"[BlockManager] remove {key}, (final: {this.FinalBlockStatus})");
    }

    [ShowInInspector, ReadOnly]
    public BlockLevel? maxBlockLevel;

    void RefreshFinal()
    {
        BlockLevel? level = null;
        foreach(var kv in this.keyToIsBlockDic)
        {
            var value = kv.Value;
            if (level == null)
            {
                level = kv.Value;
            }
            else
            {
                if((int)value > (int)level.Value)
                {
                    level = value;
                }
            }
        }

        if(maxBlockLevel != null)
        {
            if(level > maxBlockLevel)
            {
                level = maxBlockLevel;
            }
        }

        this.FinalBlockStatus = level;
    }

    [ShowInInspector, ReadOnly]
    BlockLevel? _finalBlockStatus;
    BlockLevel? FinalBlockStatus
    {
        get
        {
            return _finalBlockStatus;
        }
        set
        {
            if(_finalBlockStatus == value)
            {
                return;
            }
            _finalBlockStatus = value;
            RequestChangeBlockStatus?.Invoke(_finalBlockStatus);
        }
    }
}
