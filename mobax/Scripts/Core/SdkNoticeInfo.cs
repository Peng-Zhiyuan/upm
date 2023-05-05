using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SdkNoticeInfo 
{
    public string id;
    public string title;
    public string content;
    public string cover_name;
    public string is_open;
    public long start_time;
    public long end_time;

    public bool HasBanner
    {
        get
        {
            if(string.IsNullOrEmpty(this.content))
            {
                return false;
            }
            return true;
        }
    }
}

public class SdkNoticeResponse
{
    public List<SdkNoticeInfo> data;
}