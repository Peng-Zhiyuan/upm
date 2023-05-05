using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 登录模块大部分已迁移到 TS 运行时
/// 但是部分信息需要以 CS 的形式保存
/// 这些字段会被 TS 代码设置
/// </summary>
public static class LoginCsharpInfo
{
    public static string gameServerUrl;
    public static string selectedRoleId;
    public static string selectedRoleName;

    public static int sid;

    public static string cookieKey;
    public static string cookieValue;
}
