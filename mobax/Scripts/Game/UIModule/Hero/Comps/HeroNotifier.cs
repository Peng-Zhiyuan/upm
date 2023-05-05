using System;

public static class HeroNotifier
{
    public static event Action<HeroNotifyEnum> OnChange;

    public static void Invoke(HeroNotifyEnum notifyEnum)
    {
        OnChange?.Invoke(notifyEnum);
    }
}

[Flags]
public enum HeroNotifyEnum
{
    None = 0,
    Level = 1,  // 等级
    LevelReset,  // 等级重置
    Skill,  // 技能
    SkillReset,  // 等级重置
    Star,   // 星级
    CircuitChange, // 拼图改变（有属性改变）
    CircuitUpdate, // 拼图变动（没有属性改变）
    WeaponLevel, // 武器等级
    WeaponReset, // 武器重置
}