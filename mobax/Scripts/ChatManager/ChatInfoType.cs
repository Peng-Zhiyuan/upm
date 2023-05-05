using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ChatInfoType
{
    /// <summary>
    /// 已损坏
    /// </summary>
    Crushed,

    /// <summary>
    ///  文本消息
    /// </summary>
    Text,

    /// <summary>
    /// 表情
    /// </summary>
    Expresion,

    /// <summary>
    /// 邀请
    /// </summary>
    Invite,


    /// <summary>
    /// 不可见消息：
    /// 不会计入未读消息
    /// 不会保存历史记录
    /// </summary>
    Invisible,
}