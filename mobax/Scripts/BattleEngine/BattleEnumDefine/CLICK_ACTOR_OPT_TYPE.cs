namespace BattleEngine.Logic
{
    public enum HOVER_GAMEOBJECT_OPT_TYPE
    {
        //划过地面
        None = 0,
        /// <summary>
        /// 划过队友无操作
        /// </summary>
        Teammate = 1,
        /// <summary>
        /// 划过队友,可治疗
        /// </summary>
        TeammateHealth = 2,
        /// <summary>
        ///划过敌人无操作
        /// </summary>
        Enemy = 3,
        /// <summary>
        ///划过敌人,可攻击
        /// </summary>
        EnemyAttack = 4,
    }

    public enum CLICK_GAMEOBJECT_TYPE
    {
        //地面
        None = 0,
        //队友
        Teammate = 1,
        //敌人
        Enemy = 2,
    }
}