namespace BattleEngine.Logic
{
    using System;
    using Sirenix.OdinInspector;

    [Serializable]
    [LabelText("行为禁制")]
    public enum ACTION_CONTROL_TYPE
    {
        [LabelText("（空）")]
        None = 0,
        [LabelText("弱减速")]
        control_1 = 1,
        [LabelText("强减速")]
        control_2 = 2,
        [LabelText("沉默")]
        control_3 = 3,
        [LabelText("禁锢")]
        control_4 = 4,
        [LabelText("恐惧")]
        control_5 = 5,
        [LabelText("魅惑")]
        control_6 = 6,
        [LabelText("嘲讽")]
        control_7 = 7,
        [LabelText("致盲")]
        control_8 = 8,
        [LabelText("击飞")]
        control_9 = 9,
        [LabelText("睡眠")]
        control_10 = 10,
        [LabelText("晕眩")]
        control_11 = 11,
        [LabelText("击退")]
        control_12 = 12,
        [LabelText("压制")]
        control_13 = 13,
        [LabelText("隐身")]
        control_14 = 14,
        [LabelText("霸体")]
        control_15 = 15,
        [LabelText("无敌")]
        control_16 = 16,
        [LabelText("消失")]
        control_17 = 17,
        [LabelText("冷静")]
        control_18 = 18,
        [LabelText("燃烧")]
        control_19 = 19,
        [LabelText("中毒")]
        control_20 = 20,
        [LabelText("护盾")]
        control_21 = 21,
    }

    [LabelText("行为状态")]
    public enum ACTION_CONTROL_STATE_TYPE
    {
        ManualMove = 0,
        AIMove = 1,
        ManualAttack = 2,
        AINormalAttack = 3,
        AISkillAttack = 4,
        OperateSelfTeam = 5,
        OperateEnemyTeam = 6,
    }
}