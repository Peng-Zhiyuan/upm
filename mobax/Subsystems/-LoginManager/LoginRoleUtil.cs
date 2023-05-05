using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginRoleUtil 
{
    /// <summary>
    /// 账号现在是否被封禁
    /// </summary>
    /// <param name="loginRoleInfo">登录用户数据</param>
    /// <param name="nowTimestampSec">当前的时间戳，秒</param>
    /// <returns></returns>
    public static bool IsForbiden(LoginRoleInfo loginRoleInfo, long nowTimestampSec)
    {
        // 小于 0 一直封
        // 大于 0 时间戳
        var ts = loginRoleInfo.disable;
        if (ts == -1)
        {
            return true;
        }
        if (ts == 0)
        {
            return false;
        }
        if (nowTimestampSec < ts)
        {
            return true;
        }
        return false;
    }
}
