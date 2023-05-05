using System.Collections.Generic;
using UnityEngine;

namespace Plot.Runtime
{
    public class PlotDefineUtil
    {
        #region ---Editor---

        // 默认帧率
        public static readonly int DEFAULT_FRAME_NUM = 30;

        // 剧情编辑器场景路径
        public static readonly string PLOT_EDITOR_SCENE_PATH = "Assets/Editor/Plot/PlotEditor/PlotEditorScene.unity";

        // 剧情数据存储父文件夹
        public static readonly string PLOT_DATA_PATH_PARENT_FOLDER = "Assets/res/$Data/PlotConfigData";
        public static readonly string PLOT_AHEAD_DATA_PATH_PARENT_FOLDER = "Assets/res/$Data_ahead/PlotConfigData";

        // 剧情数据预览存储文件夹
        public static readonly string PLOT_DATA_PREVIEW_PATH_PARENT_FOLDER = "Assets/res/$Data/PlotPreviewData";
        public static readonly string PLOT_AHEAD_DATA_PREVIEW_PATH_PARENT_FOLDER = "Assets/res/$Data_ahead/PlotPreviewData";

        // 剧情编辑器关卡地图数据读取
        public static readonly string PLOT_STAGE_MAP_DATA_PARENT_FOLDER = "Assets/res/$Data/StageData/Scene";
        public static readonly string PLOT_STAGE_AHEAD_MAP_DATA_PARENT_FOLDER = "Assets/res/$Data_ahead/StageData/Scene";

        // 剧情编辑器关卡地图资源读取
        public static readonly string PLOT_STAGE_MAP_ADDRESS_PARENT_FOLDER = "Assets/Arts/Roguelike/$Prefabs";

        // 剧情数据模型资源文件夹
        public static readonly string PLOT_MODEL_PATH_FOLDER = "Assets/Arts/Plots";

        // 剧情数据模型资源文件夹
        public static readonly string PLOT_MODEL_ENV_PATH_FOLDER = "Assets/Arts/Env";

        // 剧情场景文件夹
        public static readonly string PLOT_SCENE_PATH_FOLDER = "Assets/Arts/Plots/$Scenes";

        // 剧情编辑器遮罩文件夹目录
        public static readonly string PLOT_MASK_PARENT_FOLDER = "Assets/Arts/Plots/Plot2D/$plots_Images/$plots_image_mask";

        // 剧情编辑器分镜图片文件夹目录
        public static readonly string PLOT_FRAG_PICTURE_PARENT_FOLDER =
            "Assets/Arts/Plots/Plot2D/$plots_Images";

        // 剧情编辑器遮罩溶解资源文件夹目录
        public static readonly string PLOT_MASK_DISSOLVE_PARENT_FOLDER =
            "Assets/Arts/Plots/Plot2D/$DissolveImg";

        // 剧情编辑器边框文件夹目录
        public static readonly string PLOT_FRAME_PARENT_FOLDER = "Assets/Arts/Plots/Plot2D/$plots_Images/$plots_image_frame";

        // 剧情编辑器气泡文件夹目录
        public static readonly string PLOT_BUBBLE_PARENT_FOLDER = "Assets/Arts/Plots/Plot2D/$plots_Images/$plots_image_bubble";

        // 剧情编辑器气泡预制体文件夹目录
        public static readonly string PLOT_BUBBLE_PREFAB_PARENT_FOLDER = "Assets/Arts/Plots/Plot2D/$plots_bubbles";

        // 剧情编辑器UI texture等节点
        public static readonly string PLOT_UI_COMICS_ROOT = "UIRoot/Canvas/PlotComicsPage/$ComicsFrag";

        // 剧情编辑器Timeline资源目录
        public static readonly string PLOT_TIMELINE_PARENT_FOLDER = "Assets/Arts/Plots/$plots_timelines";
        public static readonly string PLOT_CANVAS_ROOT = "UIRoot/Canvas";

        // 默认地图格子宽高
        public static Vector3 GRID_SIZE = new Vector3(20, 0, 20);
        public static Vector3 ADD_SCENE_OFFSET = new Vector3(0, 0, 0);

        // 剧情漫画3d地图桶 -- 连续漫画播放结束之后卸载
        public static readonly string PLOT_COMICS_3D_ENV_BUCKET = "Plot_Comics_3DEnv";

        // 剧情漫画遮罩框桶 -- 连续漫画播放结束之后卸载
        public static readonly string PLOT_COMICS_MASK_FRAME_BUCKET = "Plot_Comics_MaskFrame";

        // 剧情漫画遮罩框桶 -- 一屏漫画播完即卸载
        public static readonly string PLOT_COMICS_PICTURE_BUCKET = "Plot_Comics_Picture";

        public static List<string> ActionStyles { get; } = new List<string>()
        {
            CharacterActionConst.Stand,
            CharacterActionConst.Idle,
            CharacterActionConst.Run,
            CharacterActionConst.RunWeapon,
            CharacterActionConst.Attack,
            /*CharacterActionConst.Atk1,
            CharacterActionConst.Atk2,
            CharacterActionConst.Atk3,*/
            CharacterActionConst.Walk,
            CharacterActionConst.Hurt,
            CharacterActionConst.FightIdle,
            CharacterActionConst.Win,
            CharacterActionConst.Dead,
            CharacterActionConst.Wave,
            CharacterActionConst.Stun,
            CharacterActionConst.ShowWeapon,
            CharacterActionConst.FloatUp,
            CharacterActionConst.FloatHover,
            CharacterActionConst.FloatDown,
        };

        #endregion


        #region ---Run Time---

        // 剧情RunTime时相机节点
        public static readonly string PLOT_RUNTIME_CAMERA_PATH = "PlotRoot/SceneUnit/PlotCamera";

        // 剧情RunTime时地图节点
        public static readonly string PLOT_RUNTIME_MAP_ROOT_PATH = "PlotRoot/SceneUnit/MapRoot";

        // 剧情RunTime时特效节点
        public static readonly string PLOT_RUNTIME_EFFECT_ROOT_PATH = "PlotRoot/SceneUnit/EffectRoot";

        // 剧情RunTime时弹道节点
        public static readonly string PLOT_RUNTIME_BULLET_ROOT_PATH = "PlotRoot/SceneUnit/BulletRoot";

        // 剧情RunTime时模型节点
        public static readonly string PLOT_RUNTIME_MODEL_ROOT_PATH = "PlotRoot/SceneUnit/ModelRoot";

        // 剧情RunTime时Timeline节点
        public static readonly string PLOT_RUNTIME_TIMELINE_ROOT_PATH = "PlotRoot/SceneUnit/TimelineRoot";

        // 剧情RunTime时气泡节点
        public static readonly string PLOT_RUNTIME_BUBBLE_ROOT_PATH = "BubbleRoot";

        // 剧情RunTime时frame节点
        public static readonly string PLOT_RUNTIME_FRAME_ROOT_PATH = "FrameRoot";

        // 剧情RunTime时最前层
        public static readonly string PLOT_RUNTIME_FRONT_ROOT_PATH = "FrontRoot";

        // 剧情RunTime时RawImg节点
        public static readonly string PLOT_RUNTIME_RAWIMG_ROOT_PATH = "RawImg";

        // 剧情RunTime时Picture节点
        public static readonly string PLOT_RUNTIME_PICTURE_ROOT_PATH = "PictureRoot";

        // 剧情RunTime时Picture节点
        public static readonly string PLOT_RUNTIME_MASK_PICTURE_ROOT_PATH = "MaskPictureRoot";

        #endregion
    }
}