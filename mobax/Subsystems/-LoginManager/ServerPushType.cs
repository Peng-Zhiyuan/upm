using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public enum ServerPushType
{
    OrderChnaged = 9,
    PlayerMail = 10,
    PlayerGift = 11,
    [LabelText("成为好友")] Friend = 12,
    [LabelText("成为粉丝")] Fans = 13,
    [LabelText("公会申请处理")] GuildApplyChanged = 20,
    [LabelText("公会被踢")] GuildKick = 21,
    [LabelText("公会职位变动")] GuildPermissionsChanged = 22,
    GlobalConfig = 90,
    [LabelText("跑马灯")]Marquee = 91,
    
}