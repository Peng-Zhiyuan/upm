using Sirenix.OdinInspector;
using UnityEngine;

namespace Plot.Runtime
{
    [PlotComicsConfigElementItem("场景地图", (int) EPlotComicsElementType.SceneMap, (int) EConfigPriority.SceneMap)]
    public class PlotComicsSceneMapElement : PlotComicsConfigElementItem
    {
        #region ---抽象---

        public override string Label => "场景地图";

        #endregion

        [ToggleGroup("enabled"), LabelText("模型资源:")]
        [LabelWidth(80)]
        [VerticalGroup("enabled/map")]
        [ShowIf("IsPveType")]
        [Sirenix.OdinInspector.FilePath(ParentFolder = "Assets/res/$Data/StageData/Scene")]
        public string mapRes;

        [ToggleGroup("enabled"), LabelText("场景类型:")] [LabelWidth(80)] [VerticalGroup("enabled/map")]
        public EPlotMapType mapType = EPlotMapType.PveScene;

        [ToggleGroup("enabled"), LabelText("场景资源:")]
        [LabelWidth(80)]
        [VerticalGroup("enabled/map")]
        [ShowIf("IsAppointType")]
        [InfoBox("这个模式仅为预览截图模式！！！！游戏运行不兼容该模式", InfoMessageType.Warning)]
        [Sirenix.OdinInspector.FilePath(ParentFolder = "Assets/Arts/Plots/$Scenes", Extensions = "unity")]
        public string sceneRes;

        [ShowIf("IsPveType")] [VerticalGroup("enabled/map")] [LabelText(" 显示区域起点:")] [LabelWidth(80)]
        public Vector3 satrtAreaVec;

        [ShowIf("IsPveType")] [VerticalGroup("enabled/map")] [LabelText(" 显示区域终点:")] [LabelWidth(80)]
        public Vector3 endAreaVec;

        [ToggleGroup("enabled"), LabelText("显示绘制框:")]
        [LabelWidth(80)]
        public bool showGizmos = false;
        
        private bool IsAppointType()
        {
            return this.mapType.Equals(EPlotMapType.AppointScene);
        }

        private bool IsPveType()
        {
            return this.mapType.Equals(EPlotMapType.PveScene);
        }
    }
}