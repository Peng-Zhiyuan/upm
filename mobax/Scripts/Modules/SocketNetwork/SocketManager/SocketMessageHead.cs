using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public struct SocketMessageHead
{
    // 0x78
    public byte magic;

    /// <summary>
    /// 错误码
    /// </summary>
    public Int16 code;

    /// <summary>
    /// path路径字数
    /// </summary>
    public UInt16 path;

    /// <summary>
    /// 数据尺寸
    /// </summary>
    public UInt32 body;

    public override string ToString()
    {
        return $"[code: {code}, path: {path}, body: {body}]";
    }

    public static int Size
    {
        get
        {
            return 9;
        }
    }

}