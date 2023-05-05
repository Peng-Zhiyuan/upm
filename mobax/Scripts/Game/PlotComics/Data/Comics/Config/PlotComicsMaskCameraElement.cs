using System.Linq;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Plot.Runtime
{
    [PlotComicsConfigElementItem("分镜遮罩", (int) EPlotComicsElementType.CameraMask, (int) EConfigPriority.CameraMask)]
    public class PlotComicsMaskCameraElement : PlotComicsConfigElementItem
    {
        public override string Label => "分镜遮罩";

        [ToggleGroup("enabled"), LabelText("Mask Res")]
        [Tooltip("遮罩资源")]
        [LabelWidth(65)]
        //TODO:这里最好不要用0.000多位小数点
        [HorizontalGroup("enabled/model", 0.664f)]
        [VerticalGroup("enabled/model/Setting")]
        [TitleGroup("enabled/model/Setting/Mask Base Setting")]
        [VerticalGroup("enabled/model/Setting/Mask Base Setting/Setting")]
        [Sirenix.OdinInspector.FilePath(ParentFolder = "Assets/Arts/Plots/Plot2D/$plots_Images/$plots_image_mask", Extensions = "png")]
        [OnValueChanged("InitMaskTexture")]
        public string maskRes;

        // public string DynamicParent = "Assets/Arts/Plots/Plot2D/$plots_Images/$plots_image_mask";

        [VerticalGroup("enabled/model/Setting/Mask Base Setting/Setting")]
        [ToggleGroup("enabled")]
        [HideReferenceObjectPicker]
        [LabelWidth(100)]
        [LabelText(" Transform ")]
        public PlotComicsTransBaseSetting trans = new PlotComicsTransBaseSetting();

        [ToggleGroup("enabled"), LabelText("Frame Res")]
        [Tooltip("边框资源")]
        [VerticalGroup("enabled/model/Setting/Mask Base Setting/Setting")]
        [LabelWidth(65)]
        [ReadOnly]
        public string frameRes;

        [ToggleGroup("enabled"), LabelText("Remark")]
        [Tooltip("备注")]
        [VerticalGroup("enabled/model/Setting/Mask Base Setting/Setting")]
        [LabelWidth(65)]
        [SerializeField]
        [ReadOnly]
        private string desc = "1个分镜只用1张遮罩！！！";

        [ToggleGroup("enabled")]
        [VerticalGroup("enabled/model/Setting")]
        [TitleGroup("enabled/model/Setting/Frag Picture Setting")]
        [VerticalGroup("enabled/model/Setting/Frag Picture Setting/Setting")]
        [LabelText("Add Picture")]
        [LabelWidth(65)]
        public bool openPicture = false;

        [VerticalGroup("enabled/model/Setting/Frag Picture Setting/Setting")]
        [Sirenix.OdinInspector.FilePath(ParentFolder = "Assets/Arts/Plots/Plot2D/$plots_Images", Extensions = "png")]
        [OnValueChanged("InitPictureTexture")]
        [ShowIf("openPicture")]
        [LabelWidth(65)]
        [LabelText("Picture Res")]
        public string pictureRes;

        // private string DynamicParent1 = PlotDefineUtil.PLOT_FRAG_PICTURE_PARENT_FOLDER;

        [VerticalGroup("enabled/model/Setting/Frag Picture Setting/Setting")]
        [Tooltip("图片初始位置")]
        [LabelText(" Position ")]
        [LabelWidth(65)]
        [ShowIf("openPicture")]
        public Vector3 pictureStartPos = Vector3.zero;
        
        [VerticalGroup("enabled/model/Setting/Frag Picture Setting/Setting")]
        [Tooltip("开启全屏适配")]
        [LabelText("开启全屏适配")]
        [LabelWidth(65)]
        [ShowIf("openPicture")]
        public bool openFit = false;

        #region ---TEMP RESET---

        [ToggleGroup("enabled")]
        [VerticalGroup("enabled/model/Setting")]
        [TitleGroup("enabled/model/Setting/Frag Temp Picture Setting", VisibleIf = "@false")]
        [VerticalGroup("enabled/model/Setting/Frag Temp Picture Setting/Setting")]
        [LabelText("Add Picture")]
        [LabelWidth(65)]
        public bool openTempPicture = false;

        [VerticalGroup("enabled/model/Setting/Frag Temp Picture Setting/Setting")]
        [Sirenix.OdinInspector.FilePath(ParentFolder = "Assets/Arts/Plots/Plot2D/$Images/TempFrame")]
        [ShowIf("openTempPicture")]
        [LabelWidth(65)]
        [LabelText("Picture Res")]
        public string tempPictureRes;

        [VerticalGroup("enabled/model/Setting/Frag Temp Picture Setting/Setting")]
        [Tooltip("图片初始位置")]
        [LabelText(" Position ")]
        [LabelWidth(65)]
        [ShowIf("openTempPicture")]
        public Vector3 tempPictureStartPos = Vector3.zero;

        #endregion

        [HorizontalGroup("enabled/model")]
        [PreviewField(ObjectFieldAlignment.Left, Height = 140)]
        [ReadOnly]
        [VerticalGroup("enabled/model/rightInfo")]
        [HideLabel]
        public Texture2D maskTexture;

        [HorizontalGroup("enabled/model")]
        [PreviewField(ObjectFieldAlignment.Left, Height = 140)]
        [ReadOnly]
        [VerticalGroup("enabled/model/rightInfo")]
        [HideLabel]
        [ShowIf("openPicture")]
        public Texture2D fragPictureTexture;

        public void InitMaskTexture()
        {
            // 初始化frameName
            var split = this.maskRes.Split(new char[] {'_'});
            var index = split.Last();
            this.frameRes = $"frame_style_{index}";

#if UNITY_EDITOR
            this.maskTexture =
                AssetDatabase.LoadAssetAtPath<Texture2D>($"{PlotDefineUtil.PLOT_MASK_PARENT_FOLDER}/{this.maskRes}");
#endif
        }

        private void InitPictureTexture()
        {
#if UNITY_EDITOR
            this.fragPictureTexture =
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    $"{PlotDefineUtil.PLOT_FRAG_PICTURE_PARENT_FOLDER}/{this.pictureRes}");
#endif
        }
    }
}