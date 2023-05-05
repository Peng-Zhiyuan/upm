namespace BattleEngine.Logic
{
    /// <summary>
    /// 攻击结果
    /// </summary>
    public enum ATTACKRESULT_TYPE
    {
        None,
        /// <summary>
        /// 正常伤害
        /// </summary>
        Normal,
        /// <summary>
        /// 暴击
        /// </summary>
        Critical,
        /// <summary>
        /// 重击
        /// </summary>
        Slam,
    }
}