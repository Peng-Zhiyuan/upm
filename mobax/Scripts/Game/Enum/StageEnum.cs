

/** 关卡具体状态 */
public enum EStageState
{
    Lock = 0,
    Open, // 开启
    Result, // 通关
    Close, // 关闭
}

// 关卡开启状态
public enum EStageOpenState
{
    None,
    Open = 0, // 解锁
    Close = 1, // 关闭
}

/** 关卡难度模式 */
public enum EStageMode
{
    None,
    Easy = 0, // 简单模式
    Hard = 1, // 困难模式
}

/** 关卡线索 */
public enum EStageClueType
{
    None = -1,
    WaitCollected = 0, // 待收集
    Hidden = 1, // 隐藏中
    Collected = 2, // 已收集
}

/** 关卡评分等级 */
public enum EStageScoreLevel
{
    S = 1,
    A = 2,
    B = 3,
    C = 4,
    D = 5,
    Max = 6,
}

// 关卡地图人物状态
public enum EStageMapRoleState
{
    Idle = 0,
    Move = 1,
}

public enum EStageClueOpenType
{
    None = 0,
    Compose = 1,
    OpenEvent = 2,
}

public enum EStageExploreType
{
    Cat = 1, // 找猫
    Fish = 2, // 钓鱼
}

public enum EStageUIMapState
{
    Country = 1, // 国家
    Zone = 2, // 地区
    Map = 3, // 3D地图
}