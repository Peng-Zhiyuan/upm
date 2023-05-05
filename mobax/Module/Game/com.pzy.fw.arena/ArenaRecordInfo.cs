using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaRecordInfo
{
    public string _id;
    
    // 攻击方 uid
    public string attack;

    // 自己的阵营
    public ArenaRecordCamp camp;

    // 积分变动值
    public int score;
    
    // 所在赛季
    public int circle;

    // 我的对手的信息
    public ArenaPlayer target;

    // 最后修改时间
    public long update;

    public int postScore;
    public bool isWin;
}

public class ArenaPlayer
{
    public string name;
    public int score;
}

public enum ArenaRecordCamp
{
    Unknown = 0,
    Attacker = 1,
    Defender = 2,
}