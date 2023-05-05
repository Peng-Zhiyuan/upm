using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomLitJson;
using UnityEngine;

public static class CircuitTransactionHelper
{
    private const int SaveThreshold = 5;
    private static readonly Dictionary<string, CircuitTransaction> TransactionsMap = new Dictionary<string, CircuitTransaction>();

    public static async Task<bool> PutOn(HeroCircuitInfo circuitInfo, int x, int y, int shape, string heroInstanceId, bool submit = true)
    {
        var transaction = _GetPutOnTransaction(circuitInfo, x, y, shape, heroInstanceId);
        return await _InternalOperation(transaction, submit);
    }

    public static async Task<bool> Off(HeroCircuitInfo circuitInfo, bool submit = true)
    {
        var transaction = _GetTakeOffTransaction(circuitInfo);
        return await _InternalOperation(transaction, submit);
    }

    /// <summary>
    /// 检查是否需要提交当前的
    /// </summary>
    public async static Task<bool> Submit()
    {
        if (TransactionsMap.Count <= 0) return true;

        List<string> offList = null;
        Dictionary<string, JsonData> opMap = null;
        int heroId = 0;
        foreach (var transaction in TransactionsMap.Values)
        {
            heroId = transaction.heroId;
            if (transaction.operation == CircuitTransactionOperation.TakeOff)
            {
                offList ??= new List<string>();
                offList.Add(transaction.circuitInstanceId);
            }
            else
            {
                var op = new JsonData
                {
                    ["x"] = transaction.moveToX,
                    ["y"] = transaction.moveToY,
                    ["shape"] = transaction.shape,
                };
                opMap ??= new Dictionary<string, JsonData>();
                opMap[transaction.circuitInstanceId] = op;
            }
        }

        var success = await HeroCircuitApi.Set(heroId, opMap, offList);
        if (success)
        {
            // 成功了就清掉操作内容， 失败了的话， 就不处理了， 等下一个操作来临
            foreach (var transaction in TransactionsMap.Values)
            {
                PoolManager.Put(transaction);
            }
            TransactionsMap.Clear();
        }
        else
        {
            // Todo， 目前错误了也是直接将事务组重置
            // 最好是将所有未完成的事务操作回退掉
            TransactionsMap.Clear();
        }

        return success;
    }

    private static async Task<bool> _InternalOperation(CircuitTransaction transaction, bool submit)
    {
        if (TransactionsMap.TryGetValue(transaction.circuitInstanceId, out var prevTransaction))
        {
            PoolManager.Put(prevTransaction);
        }
        TransactionsMap[transaction.circuitInstanceId] = transaction;
        
        // 如果个数积累到一定程度，就需要提交
        if (submit || TransactionsMap.Count >= SaveThreshold)
        {
            return await Submit();
        }
        
        return true;
    }

    private static CircuitTransaction _GetTakeOffTransaction(HeroCircuitInfo circuitInfo)
    {
        var transaction = PoolManager.Take<CircuitTransaction>();
        var heroInstanceId = circuitInfo.ItemInfo.UsedHero;
        transaction.operation = CircuitTransactionOperation.TakeOff;
        transaction.circuitInstanceId = circuitInfo.InstanceId;
        transaction.heroId = string.IsNullOrEmpty(heroInstanceId) ? 0 : HeroHelper.InstanceIdToRowId(heroInstanceId);
        return transaction;
    }

    private static CircuitTransaction _GetPutOnTransaction(HeroCircuitInfo circuitInfo, int x, int y, int shape, string usedHero)
    {
        var transaction = PoolManager.Take<CircuitTransaction>();
        transaction.operation = CircuitTransactionOperation.PutOn;
        transaction.circuitInstanceId = circuitInfo.InstanceId;
        transaction.heroId = string.IsNullOrEmpty(usedHero) ? 0 : HeroHelper.InstanceIdToRowId(usedHero);
        transaction.shape = shape;
        transaction.moveToX = x;
        transaction.moveToY = y;
        return transaction;
    }
}

[Flags]
public enum CircuitTransactionOperation
{
    PutOn = 1 << 0,
    Move = 1 << 1,
    Turn = 1 << 2,
    TakeOff = 1 << 3,
}

public class CircuitTransaction
{
    public int heroId;
    public string circuitInstanceId;
    public int moveToX;
    public int moveToY;
    public int shape;
    public CircuitTransactionOperation operation;
}
