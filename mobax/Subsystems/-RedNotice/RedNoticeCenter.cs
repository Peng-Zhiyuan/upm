using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedNoticeCenter : Single<RedNoticeCenter>
{
    private Dictionary<string,RedNoticeUnit> noticeDic = new Dictionary<string,RedNoticeUnit>();
    public void RegisterNotice(string name,RedNoticeUnit unit)
    {
        //Debug.LogError("RegisterNotice:"+name);
        noticeDic[name] = unit;
    }

    public void UnRegisterNotice(string name)
    {
        //Debug.LogError("RegisterNotice:"+name);
        noticeDic.Remove(name);
    }

    public RedNoticeUnit GetNotice(string noticeName)
    {
        return noticeDic[noticeName];
    }

     public void RefreshNotice(string noticeName,bool isShow,string info = "")
    {
        if(noticeDic.ContainsKey(noticeName))
        {
            //Debug.LogError("RefreshNotice:"+noticeName);
            RedNoticeUnit noticeUnit = noticeDic[noticeName];
            if(noticeUnit)
            {
                noticeUnit.Refresh(isShow,info);
            }
        }
      
    }
}
