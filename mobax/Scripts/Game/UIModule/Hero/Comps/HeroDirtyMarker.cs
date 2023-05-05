using System;

public static class HeroDirtyMarker
{
    private static HeroDirtyEnum _marker;

    public static void Mark(HeroDirtyEnum d)
    {
        _marker = _marker | d;
    }
    
    public static void Clear(HeroDirtyEnum d)
    {
        _marker = _marker & ~d;
    }
    
    public static bool IsDirty(HeroDirtyEnum d)
    {
        return (_marker & d) == d;
    }
}

[Flags]
public enum HeroDirtyEnum
{
    None = 0,
    Level = 1 << 0,  // 等级
    Skill = 1 << 1,  // 技能
    Star = 1 << 2,   // 星级
    Puzzle = 1 << 3, // 拼图
}