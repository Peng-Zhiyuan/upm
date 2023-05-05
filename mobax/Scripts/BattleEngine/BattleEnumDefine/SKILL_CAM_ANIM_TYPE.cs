using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [LabelText("相机类型")]
    public enum SKILL_USE_CAM_TYPE
    {
        [LabelText("主相机")]
        Main = 0,
        [LabelText("技能相机")]
        Skill = 1
    }

    [System.Serializable]
    [LabelText("相机动画类型")]
    public enum SKILL_CAM_ANIM_TYPE
    {
        [LabelText("动画")]
        Anim = 0,
        [LabelText("远近上下缩放")]
        Trans = 1
    }

    [System.Serializable]
    [LabelText("相机坐标类型")]
    public enum SKILL_CAM_POSITION_TYPE
    {
        [LabelText("本地坐标")]
        Local = 0,
        [LabelText("全局坐标")]
        Global = 1
    }
}