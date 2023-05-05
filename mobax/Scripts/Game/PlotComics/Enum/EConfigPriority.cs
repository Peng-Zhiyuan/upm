using Sirenix.OdinInspector;

namespace Plot.Runtime
{
    public enum EConfigPriority
    {
        [LabelText("渲染优先级-->1(最高)")] SceneMap = 1, // 地图场景
        [LabelText("渲染优先级-->2")] SceneModel = 2, // 角色模型
        [LabelText("渲染优先级-->3")] SceneModelEnv = 3, // 场景模型
        [LabelText("渲染优先级-->4")] Timeline = 4,
        [LabelText("渲染优先级-->5")] SceneCamera = 5, // 场景相机
        [LabelText("渲染优先级-->6")] CameraMask = 6, // 漫画格遮罩
        [LabelText("渲染优先级-->7(最低)")] Bubble = 7, // 对话气泡
        [LabelText("渲染优先级-->8(最低)")] ReferenceImage = 8, // 参考图
    }
}