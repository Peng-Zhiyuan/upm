using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetPage<T>
{
    public List<T> rows;

    // 页面序号，从 1 开始
    public int page;

    // 页面大小
    public int size;

    // 总页数
    public int total;

    // 总记录数
    public int record;

    // timestamp sec
    public int update;
}
