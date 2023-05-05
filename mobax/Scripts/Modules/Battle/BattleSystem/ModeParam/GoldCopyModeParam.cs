using System.Collections.Generic;

/// <summary>
/// 金钱本参数
/// </summary>
public sealed class GoldCopyModeParam
{
    public bool IsDouble; // 是否双倍
    public List<int> MonsterGroup = new List<int>(); // 怪物数组
    public EResourceCopyDifficult Difficult; // 难易度
}