namespace BattleEngine.Logic
{
    [System.Serializable]
    /// <summary>
    /// 生命状态
    /// </summary>
    public enum ACTOR_LIFE_STATE
    {
        /// <summary>
        /// 没有创建
        /// </summary>
        None = 0,
        /// <summary>
        /// 出身状态
        /// </summary>
        Born,
        /// <summary>
        /// 正在入场
        /// </summary>
        Entering,
        /// <summary>
        /// 活着 
        /// </summary>
        Alive,
        /// <summary>
        /// 死亡
        /// </summary>
        Dead,
        /// <summary>
        /// 停止操作
        /// </summary>
        StopLogic,
        /// <summary>
        /// 无敌模式
        /// </summary>
        God,
        /// <summary>
        ///替补状态
        /// </summary>
        Substitut,
        /// <summary>
        ///旁观
        /// </summary>
        LookAt,
        /// <summary>
        ///助战
        /// </summary>
        Assist,
        /// <summary>
        ///守护
        /// </summary>
        Guard,
    }
}