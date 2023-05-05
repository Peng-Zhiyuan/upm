namespace BattleEngine.Logic
{
    public enum ACTOR_TYPE
    {
        NONE = -1,
        ADC = 0, //物理输出核心单位
        APC = 1, //法术输出核心单位
        TANK = 2, //伤害吸收核心单位
        Enchanter = 3, //魔法师
        HEALTH = 4, //治疗
    }
}