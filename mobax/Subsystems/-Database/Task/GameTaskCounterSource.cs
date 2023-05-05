using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameTaskCounterType 
{
    // 不需要，可以直接完成
    None = 0,

    // 玩家基础(包括角色信息，道具，日常，成就...)
    Normal = 1,

    // 本周数据, 从历史 daily 中累加得到计数
    Weekly = 99,

    // 历史数据，从历史 daily 中累加得到计数，需要指定统计的日期
    DailyHistroy = 100,

    // 关卡完成次数，从 arg[0] 中指定关卡 id
    Stage = 101,
}
