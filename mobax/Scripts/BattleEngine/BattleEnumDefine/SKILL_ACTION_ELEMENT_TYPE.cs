using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [LabelText("行为类型")]
    public enum SKILL_ACTION_ELEMENT_TYPE
    {
        NONE = 0,
        [LabelText("动画")]
        Animation = 1,
        [LabelText("技能特效")]
        Effect = 2,
        [LabelText("技能生效检测")]
        AttackBox = 3,
        [LabelText("冲峰")]
        Charge = 4,
        [LabelText("屏幕震动(注销)")]
        ScreenShake = 5,
        [LabelText("音效")]
        Audio = 6,
        [LabelText("弹道")]
        Trajectory = 7,
        [LabelText("帧终断")]
        FrameBreak = 8,
        [LabelText("事件")]
        BattleEvent = 9,
        [LabelText("QTE转向(注销)")]
        QteTurn = 10,
        [LabelText("爆炸")]
        Explosion = 11,
        [LabelText("屏幕压暗(注销)")]
        ScreenDimming = 12,
        [LabelText("相机动画")]
        CameraAni = 13,
        [LabelText("时停(注销)")]
        TimeStop = 14,
        [LabelText("瞬移")]
        Teleport = 15,
        [LabelText("受击特效")]
        HitEffect = 16,
        [LabelText("移动")]
        Move = 17,
        [LabelText("预警")]
        PreWaring = 18,
        [LabelText("DOT")]
        Dot = 19,
        [LabelText("盾墙")]
        ShieldWall = 20,
        [LabelText("召唤(注销)")]
        Summoning = 21,
        [LabelText("弹道指示抛物线")]
        ParabolaDirection = 22,
        [LabelText("击退")]
        BeatBack = 23,
        [LabelText("跳跃")]
        Jump = 24,
        [LabelText("击飞")]
        AirBorne = 25,
        [LabelText("TimeLine")]
        TimeLine = 26,
        [LabelText("虚拟相机")]
        VirtulCamera = 27,
        [LabelText("震屏")]
        CameraShake = 28,
        [LabelText("吸星大法")]
        TargetTogether = 29,
        [LabelText("镜头特效")]
        CameraEffect = 30,
        [LabelText("蒙皮Fresnel")]
        SkinMeshFresnelEffect = 31,
        [LabelText("金鱼姬弹道")]
        FlowerBulletEffect = 32,
    }

    public enum SKILL_ACTION_ELEMENT_PLATFORM
    {
        NONE = 0, //不区分
        LOGIC = 1, //逻辑层
        VIEW = 2, //表现层
        BOTH = 3, //都需要
    }
}