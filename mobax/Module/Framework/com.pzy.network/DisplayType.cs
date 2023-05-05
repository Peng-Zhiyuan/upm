using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DisplayType
{
    /// <summary>
    /// 不做任何处理
    /// </summary>
    None,

    /// <summary>
    /// 记录到全局缓存
    /// </summary>
    Cache,

    /// <summary>
    /// 显示可显示的数据变动，如果全局缓存中有内容，同时也将其显示并清空
    /// </summary>
    Show
}
