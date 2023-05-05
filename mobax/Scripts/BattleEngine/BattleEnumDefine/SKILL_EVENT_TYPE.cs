using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [LabelText("事件类型类型")]
    public enum SKILL_EVENT_TYPE
    {
        [LabelText("(无)")]
        NONE = 0,
        [LabelText("(关闭UI)")]
        UI_CLOSE = 1,
        [LabelText("打开UI")]
        UI_OPEN = 2,
        [LabelText("关闭押暗效果")]
        UI_CloseDark = 3,
        [LabelText("近战技能镜头")]
        Camera_Skill1 = 4,
        [LabelText("远程技能镜头")]
        Camera_Skill2 = 5,
        [LabelText("隐藏蒙皮")]
        hide_SkinMesh = 6,
        [LabelText("显示蒙皮")]
        show_SkinMesh = 7,
        [LabelText("克罗可闹钟瞬移")]
        Clock_Move = 8,
        [LabelText("召唤猫咪")]
        SummonCat = 9,
        [LabelText("显示道具文字")]
        ShowItemWords = 10,
        [LabelText("显示节点对象")]
        ShowGameObject = 11,
        [LabelText("关闭节点对象")]
        HideGameObject = 12,
        [LabelText("霸体生效")]
        CantControl = 13,
        [LabelText("流光")]
        FreshEffect = 14,
    }
}