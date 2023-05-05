namespace BattleEngine.Logic
{
    [System.Serializable]
    public enum ACTOR_EVENT_TYPE : int
    {
        None = 0,
        Born, //出生
        Idle, //
        Move, //移动
        ATK, //普攻
        SPSkl, //释放SP技能
        QTESkl, //释放QTE技能
        HitTarget, //命中目标
        Hurt, //受伤
        KnockBack, //被击退
        Float, //被浮空
        Controlled, //受控
        Cure, //治疗
        Dead, //死亡
    }
}