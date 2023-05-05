using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Sirenix.OdinInspector;

public class DailyDatabase
{
    [ShowInInspector]
    Dictionary<int, Dictionary<int, DailyInfo>> dateIndexToKeyToInfoDic = new Dictionary<int, Dictionary<int, DailyInfo>>();
    [ShowInInspector]
    int minDateIndex = -1;
    [ShowInInspector]
    int maxDateIndex = -1;
    

    public async Task ResetByFetchTodayAsync()
    {
        this.Clean();
        var startDate = this._30DayBeforeDate;
        var startDateIndex = DateUtil.DateToDateIndex(startDate);
        var infoList = await DailyApi.RequestAsync(startDateIndex);
        this.AddList(infoList);
    }

    public void AddList(List<DailyInfo> infoList)
    {
        if(infoList == null)
        {
            return;
        }
        foreach(var info in infoList)
        {
            this.Add(info);
        }
    }

    public void Add(DailyInfo info)
    {
        var dateIndex = info.time;
        var key = info.id;
        var keyToInfoDic = this.dateIndexToKeyToInfoDic.GetOrCreateDic(dateIndex);
        keyToInfoDic[key] = info;

        if(maxDateIndex == -1)
        {
            maxDateIndex = dateIndex;
        }
        if(minDateIndex == -1)
        {
            minDateIndex = dateIndex;
        }

        if(dateIndex > maxDateIndex)
        {
            this.maxDateIndex = dateIndex;
        }
        else if(dateIndex < minDateIndex)
        {
            this.minDateIndex = dateIndex;
        }
    }

    private DateTime _30DayBeforeDate
    {
        get
        {
            var nowDate = Clock.Now;
            var _30DayBefore = nowDate - TimeSpan.FromDays(30);
            return _30DayBefore;
        }
    }

    public DateTime MaxDateTime
    {
        get
        {
            var maxDateIndex = this.maxDateIndex;
            var ret = DateUtil.DateIndexToDateTime(maxDateIndex);
            return ret;
        }
    }

    public DateTime MinDateTime
    {
        get
        {
            var minDateIndex = this.minDateIndex;
            var ret = DateUtil.DateIndexToDateTime(minDateIndex);
            return ret;
        }
    }

    void Modify(int dateIndex, int key, int value)
    {
        var keyToValueDic = this.dateIndexToKeyToInfoDic.GetOrCreateDic(dateIndex);
        var info = keyToValueDic.GetOrCreate(key, () =>
        {
            var info = new DailyInfo();
            info.id = key;
            return info;
        });
        info.val = value;
    }

    public int Get(int key, int dayOffset = 0)
    {
        var date = Clock.Now;
        if(dayOffset != 0)
        {
            if(dayOffset > 0)
            {
                throw new Exception("[DailyDatabase] dayOffset must be 0 or negative");
            }
            date = date.AddDays(dayOffset);
        }
        var ret = Get(date, key);
        return ret;
    }

    public int Get(DateTime date, int key)
    {
        var dateIndex = DateUtil.DateToDateIndex(date);
        var keyToInfoDic = this.dateIndexToKeyToInfoDic.TryGet(dateIndex, null);
        if(keyToInfoDic == null)
        {
            return 0;
        }
        var info = keyToInfoDic.TryGet(key, null);
        if(info == null)
        {
            return 0;
        }
        var value = info.val;
        return value;
    }

    public bool Contains(int key, int dayOffset = 0)
    {
        var date = Clock.Now;
        if (dayOffset != 0)
        {
            if (dayOffset > 0)
            {
                throw new Exception("[DailyDatabase] dayOffset must be 0 or negative");
            }
            date = date.AddDays(dayOffset);
        }
        var ret = Contains(date, key);
        return ret;
    }

    public bool Contains(DateTime date, int key)
    {
        var dateIndex = DateUtil.DateToDateIndex(date);
        var keyToInfoDic = this.dateIndexToKeyToInfoDic.TryGet(dateIndex, null);
        if(keyToInfoDic == null)
        {
            return false;
        }
        var b = keyToInfoDic.ContainsKey(key);
        return b;
    }

    public int Count(DateTime startDate, DateTime endDate, int key)
    {
        var startIndex = DateUtil.DateToDateIndex(startDate);
        var endIndex = DateUtil.DateToDateIndex(endDate);
        startIndex = Math.Max(startIndex, this.minDateIndex);
        endIndex = Math.Min(endIndex, this.maxDateIndex);

        var sum = 0;
        foreach(var kv in this.dateIndexToKeyToInfoDic)
        {
            var dateIndex = kv.Key;
            var keyToInfoDic = kv.Value;
            if (dateIndex >= startIndex && dateIndex <= endIndex)
            {
                var info = keyToInfoDic.TryGetValue(key);
                var count = 0;
                if(info != null)
                {
                    count = info.val;
                }
                sum += count;
            }
        }
        return sum;
    }


    public void Clean()
    {
        this.dateIndexToKeyToInfoDic.Clear();
        this.minDateIndex = -1;
        this.maxDateIndex = -1;
    }

    public void ApplyTransaction(DatabaseTransaction transaction, long timestampMs)
    {
        var t = transaction.t;
        if(t == TransactionType.New)
        {
            var newObjArray = transaction.r;
            var newObj = newObjArray[0];
            var value = newObj["val"].ToInt();
            var id = newObj["id"].ToInt();

            var date = Clock.ToDateTime(timestampMs);
            var dateIndex = DateUtil.DateToDateIndex(date);
            Modify(dateIndex, id, value);
        }
        else
        {
            var rowId = transaction.id;
            var newValue = transaction.r.ToInt();
            var date = Clock.ToDateTime(timestampMs);
            var dateIndex = DateUtil.DateToDateIndex(date);
            Modify(dateIndex, rowId, newValue);
        }


    }
}
