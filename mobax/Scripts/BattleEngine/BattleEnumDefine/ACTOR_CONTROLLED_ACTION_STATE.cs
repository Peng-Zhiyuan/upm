namespace BattleEngine.Logic
{
    [System.Serializable]
    /// <summary>
    /// 受控异常状态类型
    /// </summary>
    public enum ACTOR_CONTROLLED_ACTION_STATE
    {
        None = 0,
        /// <summary>
        /// 缠绕
        /// </summary>
        Enlace,
        /// <summary>
        /// 迷惑
        /// </summary>
        Charmed,
        /// <summary>
        /// 眩晕
        /// </summary>
        Dizzy,
        /// <summary>
        /// 瘫痪
        /// </summary>
        Iull,
        /// <summary>
        /// 麻痹
        /// </summary>
        Palsy,
        /// <summary>
        /// 冰冻
        /// </summary>
        Frozen
    }
}