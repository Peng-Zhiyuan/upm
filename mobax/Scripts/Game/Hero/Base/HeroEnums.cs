// 务必从1开始， 因为对应表里ID。 
// 如PuzzleAttach中传过来的就是表里的id对应

using Sirenix.OdinInspector;

public enum HeroAttr
{
    VIM = 1,
    HP,
    ATK,
    DEF,
    BREAK,
    CRIT,
    TENA,
    RATE,
    HIT,
    BLOCK,
    PHYS,
    CURE,
    HASTE,
    HPUP,
    ATKUP,
    DEFUP,
    HURT,
    PARRY,
    RAGE,
    FIREATK,
    ELECATK,
    WATERATK,
    WINDATK,
    LIGHEATK,
    DARKATK,
    FIREDEF,
    ELECDEF,
    WATERDEF,
    WINDDEF,
    LIGHEDEF,
    DARKDEF,
    EXPUP,
    GOLDUP,
    DROPUP,
    MOVESPEED,
    ATKSPEED,
    WARD,
    MaxNum,
}

/** 角色性别（对应表配置） */
public enum HeroGender
{
    None = 0,
    Male = 1,
    Female = 2,
}

public enum HeroJob
{
    Job1 = 1,
    Job2,
    Job3,
    Job4,
    Job5,
}

public enum ElementType
{
    Elem0_None = 0,
    Elem1_Fire = 1,
    Elem2_Thunder,
    Elem3_Water,
    Elem4_Wind,
    Elem5_Light,
    Elem6_Dark
}

// 台词类型
public enum EDialogType
{
    Greating = 1, // 登场台词
}

// 英雄类型
public enum EHeroType
{
    [LabelText("角色")] Hero = 0,
    [LabelText("npc")] NPC = 1,
    [LabelText("怪物")] Monster = 2,
}