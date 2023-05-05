using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomLitJson;

public static class VipUtil 
{
    public static bool IsVipEffect(int vipItemId)
    {
        var itemInfo = Database.Stuff.itemDatabase.GetFirstItemInfoOfRowId(vipItemId);
        if(itemInfo == null)
        {
            return false;
        }
        var expire = itemInfo.TryGetAttachField<int>("Expire", 0);
        var now = Clock.TimestampSec;
        if(expire > now)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool HasSubmitToday(int vipItemId)
    {
        var itemInfo = Database.Stuff.itemDatabase.GetFirstItemInfoOfRowId(vipItemId);
        if (itemInfo == null)
        {
            return false;
        }
        var submitTsSec =  itemInfo.TryGetAttachField<long>("Submit", 0);
        var submitDate = Clock.ToDateTime(submitTsSec);
        var today = Clock.Today;
        if (submitDate >= today)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
