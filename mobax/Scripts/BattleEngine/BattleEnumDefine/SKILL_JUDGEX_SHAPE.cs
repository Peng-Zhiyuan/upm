using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [LabelText("检测范围形状")]
    public enum SKILL_JUDGEX_SHAPE
    {
        [LabelText("圆形")]
        Cylinder = 0,
        [LabelText("扇形")]
        Sector = 1,
        [LabelText("矩形")]
        Cube = 2,
        [LabelText("环形")]
        Annular = 3
    }
}