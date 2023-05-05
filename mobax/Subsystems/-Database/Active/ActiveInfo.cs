using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;

public class ActiveInfo
{
    public string iid;

    public long ttl;

    // 动态扩展位桶
    public uint[] val;
    //public ulong val;
}