namespace BattleSystem.ProjectCore
{
    /// <summary>
    /// 提供面向玩家理解的游戏模式分类
    /// </summary>
    public enum BattleModeType
    {
        Roguelike,
        PveMode,
        PveExplore, // 关卡探索-简称找猫猫
        Arena, // 竞技场
        Dreamscape, // 记忆回廊
        Gold, //金币模式 不显示胜利失败

        Defence, //守卫模式
        Guard, //守护模式（斯拉李）

        Role, //角色模式  不显示胜利失败
        Item, //道具本  不显示胜利失败

        SkillView, //技能展示
        Fixed, //新守卫模式，三个出怪点

        TowerNormal, //爬塔普通
        TowerFixed, //爬塔固定防守模式A
    }
}