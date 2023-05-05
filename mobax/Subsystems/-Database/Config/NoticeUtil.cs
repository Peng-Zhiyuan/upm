using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoticeUtil
{
    public static bool HasAnyNoticeNotRead
    {
        get
        {
            //var list = ConfigManager.Stuff.Get(ConfigType.Notice);
            var list = SdkNoticeManager.Stuff.GetAll();
            foreach (var one in list)
            {
                var id = one.id;
                var b = IsNotiedHasReadMark(id);
                if (!b)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public static void SetReadMarkForNotice(string configId, bool b)
    {
        var key = $"notice.{configId}";
        PlayerPrefsUtil.SetBool(key, b);
    }

    public static bool IsNotiedHasReadMark(string configId)
    {
        var key = $"notice.{configId}";
        var ret = PlayerPrefsUtil.GetBool(key, false);
        return ret;
    }
}
