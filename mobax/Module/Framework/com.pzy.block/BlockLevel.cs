using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockLevel 
{
    /// <summary>
    /// 不可见阻塞
    /// </summary>
    Invisible = 1,

    /// <summary>
    /// 可见阻塞
    /// </summary>
    Visible = 2,

    /// <summary>
    /// 转场阻塞
    /// </summary>
    Transaction = 3,
}
