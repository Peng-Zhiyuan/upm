using Sirenix.OdinInspector;

namespace Plot.Runtime
{
    [LabelText("节点类型:")]
    public enum EPlotComicsElementType
    {
        [LabelText("场景地图")] SceneMap = 1, // 地图场景
        [LabelText("场景模型")] SceneModel = 2, // 角色模型
        [LabelText("场景相机")] SceneCamera = 3, // 场景相机
        [LabelText("分镜遮罩")] CameraMask = 4, // 漫画格遮罩
        [LabelText("对话气泡")] Bubble = 5, // 对话气泡
        [LabelText("场景模型-其他(炸弹&猫等)")] SceneModelEnv = 6, // 场景模型
        [LabelText("TimeLine")] Timeline = 7,
        [LabelText("参考图")] ReferenceImage = 8,
    }
}