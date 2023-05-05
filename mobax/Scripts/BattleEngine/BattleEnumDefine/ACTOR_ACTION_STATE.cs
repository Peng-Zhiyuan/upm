namespace BattleEngine.Logic
{
    [System.Serializable]
    /// <summary>
    /// 战斗单位行为状态
    /// </summary>
    public enum ACTOR_ACTION_STATE
    {
        None = 0,
        Born //出生
        ,
        Idle //idle
        ,
        Move //移动
        ,
        ATK //攻击
        ,
        MoveATK //攻击时移动
        ,
        Cure //治疗
        ,
        Defence //buff防御状态，不可移动，不可攻击，受伤抵抗加强， 方向性防御加强
        ,
        Hurt //受伤硬值状态
        ,
        Knocked //击倒状态
        ,
        Floating //浮空状态
        ,
        Fall //倒地状态
        ,
        GetUp //起身状态，可以时击倒起身，也可以时浮空起身
        ,
        Grab //抓住
        ,
        Controlled //受控异常状态，包含眩晕(dizzy)、麻痹(iull)、瘫痪(palsy)
        ,
        Weak //破防后的虚弱状态
        ,
        Invulnerable //无敌状态
        ,
        Fly //飞天状态
        ,
        Dead //死亡状态
        ,
        Victory //胜利状态
        ,
        Static //静止状态
        ,
        Destory //死亡
        ,
        ChangeSence //转场
    }
}