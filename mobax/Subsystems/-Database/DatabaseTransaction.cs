using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomLitJson;

public class DatabaseTransaction 
{
    //eg: {"_id":"102x2xekp","id":15001,"t":5,"k":"","v":1,"b":15,"r":{"_id":"102x2xekp","id":15001,"uid":"102x2","val":1,"bag":15,"attach":[1,0,0,1,1]}}

    // 此记录的 id
    public string _id;

    // 数据行 id（如果有）
    public int id;

    // 1: add
    // 2: sub
    // 3: set
    // 4: del
    // 5: new
    // 6: auto-decomposition：已被自动分解
    // 7: overflow: 数量溢出，已使用其他方式（比如邮件）发送
    public TransactionType t;

    // 某些数据库操作补充说明用的路径
    public string k;

    // 变动数
    // 通常情况下是数值
    // 在 item 更新时，若 k 是 *，此字段是一个对象，包含所有更新的字段，此时 r 与 v 一致
    public JsonData v;

    // 背包类型
    public int b;

    // 最终结果
    // 可能是数值或者对象
    public JsonData r;
}
