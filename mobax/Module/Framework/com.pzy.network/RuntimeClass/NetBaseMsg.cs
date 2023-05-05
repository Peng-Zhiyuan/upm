using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomLitJson;

public class NetBaseMsg
{
    public int code;
    //public string err;
    //public int sync_id;//消息的唯一标示ID，用来给服务器标志包的唯一性，出现包重发多次时标记用
    public long time;//服务器时间，用来给客户端同步时间
    //public string cache;//资源类的改变列表，用来同步金钱，钻石等关键资源变化。
    public List<JsonData> cache;

    public GameServerCookie cookie;

    public bool IsSuccess
    {
        get
        {
            return code == 0;
        }
    }
}

