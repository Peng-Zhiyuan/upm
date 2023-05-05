namespace BattleEngine.Logic
{
    public enum CAMP
    {
        None = 0,
        Hero = 1,
        Enemy = 2,
        Neutral = 3,
    }

    public enum UnitType
    {
        None = 0,
        Hero = 1,
        Monster = 2,
        Pet = 3,
        Spirit = 4
    }

    //敌人类型
    public enum EnemyType
    {
        None = 0,
        SmallMonster = 1, //杂兵
        Monster = 2, //普通怪
        EliteMonster = 3, //精英怪
        DropBoss = 4, //不是Boss, 但是掉落Boss的东西
        Boss = 5,
    }

    //关卡类型
    public enum LevelType
    {
        None = 0,
        Common = 1, //普通关
        Boss = 2, //boss关
        TimeLimit = 3, //时间关卡
        Protect = 4, //保护关卡
        Legion = 5, //刷兵关卡
    }

    /// <summary>
    /// 刷怪类型
    /// </summary>
    public enum SwapType
    {
        Orgin = 0,
        Area = 1, //区域刷怪
        Around = 2, //周围
    }

    //掉落物品类型
    public enum DropItemType
    {
        coin = 0,
        gem = 1,
        rankGem = 2,
        equip = 3,
        mp = 1000,
    }

    public enum CharactorState
    {
        Idle = 0,
        Move = 1,
        Attack = 2,
        Hurt = 3,
        Controlled = 4,
        Knocked = 5,
        Die
    }

    public enum RenderingMode
    {
        Opaque,
        Cutout,
        Fade,
        Transparent,
    }

    public enum WeaponState
    {
        None = 0,
        Fire = 1,
        Ice = 2
    }

    public enum DropType
    {
        Coin = 0,
        Exp = 1,
    }

    public enum KnockType
    {
        Low = 0,
        High = 1,
    }

    public enum DieEfType
    {
        Anim = 0, //死亡动画
        Ragdoll = 1, //布娃娃
        Sliced = 2, //断肢
    }
}