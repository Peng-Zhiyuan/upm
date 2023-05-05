using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [LabelText("角色姿态类型")]
    public enum ACTOR_POSTURE_TYPE
    {
        [LabelText("常规姿态")]
        NORMAL = 0,
        [LabelText("防御姿态")]
        DEFENSE = 1,
        [LabelText("暴怒姿态")]
        FURY = 2
    }
}