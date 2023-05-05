using System.Collections.Generic;

namespace SpineRegulate
{
    /// <summary>
    /// 新项目需要设置这些配置文件
    /// </summary>
    public class SpineRegulateDefined
    {
        // spine 位置调整工具预览场景
        public static readonly string SPINE_REGULATE_PREVIEW_SCENE = "Assets/$Module/Framework/com.xinwusai.spineRegulate/Editor/SpineRegulateScene.unity";

        // spine 位置调整工具检测目录-- 用来遍历类型脚本
        public static readonly string SPINE_REGULATE_CHECK_FOLDER = "Assets/res/$UI";
        public static readonly string SPINE_REGULATE_CHECK_FOLDER1 = "Assets/res/$UI_ahead";

        // spine 位置调整工具数据存储文件夹
        public static readonly string SPINE_REGULATE_DATA_PARENT_FOLDER = "Assets/res/$Data/SpineRegulateData";

        // spine 位置调整工具数据UI数据存储文件夹
        public static readonly string SPINE_REGULATE_DATA_UIPOS_PARENT_FOLDER =
            "Assets/res/$Data/SpineRegulateData/UIPos";
        public static readonly string SPINE_REGULATE_AHEAD_DATA_UIPOS_PARENT_FOLDER =
            "Assets/res/$Data_ahead/SpineRegulateData/UIPos";
        // spine 位置调整工具数据存储文件夹 -- 模板数据文件夹
        public static readonly string SPINE_REGULATE_TEMPLATE_DATA_PARENT_FOLDER =
            "Assets/res/$Data/SpineRegulateData/Template";

        // spine 位置调整工具数据存储文件夹 -- 全身像模板数据文件夹
        public static readonly string SPINE_TEMPLATE_MODEL = "Model";

        // spine 位置调整工具数据存储文件夹 -- 半身像模板数据文件夹
        public static readonly string SPINE_TEMPLATE_HALF_MODEL = "HalfModel";
        
        // spine 位置调整工具数据存储文件夹 -- 特写镜头模板数据文件夹
        public static readonly string SPINE_TEMPLATE_FEATURE_CAMERA = "FeatureCamera";

        // spine 位置调整工具spine资源文件夹
        public static readonly string SPINE_PREVIEW_PARENT_FOLDER = "Assets/Arts/Spine";
        
        // spine 位置调整工具spine模板资源
       
        public static readonly string SPINE_TEMPLATE_PREFAB = "Assets/res/$UI_ahead/GameControls/com.xinwusai.spine-ui/UISpineUnit.prefab";
    }
}