using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameErrorCode 
{
    public const string FailAndShouldUseCostItemInstead = "PAYMENT_FailAndShouldUseCostItemInstead";
    public const string FailDueToAlreadySubscribed = "PAYMENT_FailDueToAlreadySubscribed";
    public const string OrderSubmitFail = "ORDER_SUBMIT_FAIL";

    // 没有阵容
    public const string Server_NoFormation = "server_refused_1000101";

    // 掉线
    public const string Server_Disconnected = "server_refused_-100";

    // 需要重新登录
    public const string Server_NeedRelogin = "server_refused_1";

    // 顶号
    public const string Server_SignInElsewhere = "server_refused_3";

    // 没有服务器
    public const string Server_NoAnyServer = "server_refused_404";

    public const string Server_TimeMax = "server_refused_110";
}
