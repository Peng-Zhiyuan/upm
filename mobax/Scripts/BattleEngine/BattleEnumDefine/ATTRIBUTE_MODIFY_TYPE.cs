using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [LabelText("修饰类型")]
    public enum ATTRIBUTE_MODIFY_TYPE
    {
        Add = 0,
        PercentAdd = 1,
        Set = 2
    }
}