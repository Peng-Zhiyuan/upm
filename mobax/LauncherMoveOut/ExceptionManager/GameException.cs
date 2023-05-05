using UnityEngine;
using System.Collections;
using System;

public class GameException : Exception
{
    public ExceptionFlag flag;
    public string code;
    public string generalExplanation;

    /// <summary>
    /// 高级异常
    /// </summary>
    /// <param name="flag">为异常指定额外处理</param>
    /// <param name="developerMsg">给开发者用的消息</param>
    /// <param name="code">每一个可以反馈给用户的异常，都需要一个独立的错误码，否则将展示位异常报告界面</param>
    /// <param name="generalExplanation">一般性描述。当没有找到详细解释时使用，诸如“操作失败，请重试”。当为空时将会使用默认语句。</param>
    public GameException(ExceptionFlag flag, string developerMsg, string code = "", string generalExplanation = "") : base($"{flag}|:|{code}|:|{developerMsg}|:|{generalExplanation}")
    {
        this.flag = flag;
        this.code = code;
        this.generalExplanation = generalExplanation;
    }
}

public enum ExceptionFlag
{
    /// <summary>
    /// 不做任何特殊处理
    /// </summary>
    None,

    /// <summary>
    /// 静默
    /// </summary>
    Silent,

    /// <summary>
    /// 登录账号
    /// </summary>
    Logout,
}
