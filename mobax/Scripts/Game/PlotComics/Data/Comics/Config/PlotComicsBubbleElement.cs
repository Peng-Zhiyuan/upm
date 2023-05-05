using System.Collections.Generic;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace Plot.Runtime
{
    [PlotComicsConfigElementItem("对话气泡", (int) EPlotComicsElementType.Bubble, (int) EConfigPriority.Bubble)]
    // 这个气泡只起到预览调试作用
    public class PlotComicsBubbleElement : PlotComicsConfigElementItem
    {
        public override string Label => "对话气泡";

        [ToggleGroup("enabled"), LabelText("Target")]
        [LabelWidth(65)]
        //TODO:这里最好不要用0.000多位小数点
        [HorizontalGroup("enabled/model", 0.720f)]
        [VerticalGroup("enabled/model/Setting")]
        [TitleGroup("enabled/model/Setting/Bubble Base Setting")]
        [VerticalGroup("enabled/model/Setting/Bubble Base Setting/Setting")]
        [HorizontalGroup("enabled/model/Setting/Bubble Base Setting/Setting/button", 0.9f)]
        [OnValueChanged("InitBubbleTexture")]
        [Sirenix.OdinInspector.FilePath(ParentFolder = "Assets/Arts/Plots/Plot2D/$plots_Images/$plots_image_bubble", Extensions = "png")]
        public string bubbleRes;

        // private string DynamicParent = PlotDefineUtil.PLOT_BUBBLE_PARENT_FOLDER;

        [ToggleGroup("enabled")]
        [Tooltip("唯一ID")]
        [LabelText("Id")]
        [LabelWidth(65)]
        [VerticalGroup("enabled/model/Setting/Bubble Base Setting/Setting")]
        public int id;

        [HorizontalGroup("enabled/model")]
        [PreviewField(ObjectFieldAlignment.Left, Height = 120)]
        [ReadOnly]
        [HideLabel]
        public Texture2D bubbleTexture;

        [VerticalGroup("enabled/model/Setting/Bubble Base Setting/Setting")]
        [ToggleGroup("enabled")]
        [HideReferenceObjectPicker]
        [LabelWidth(100)]
        [LabelText(" Transform ")]
        public PlotComicsTransBaseSetting trans = new PlotComicsTransBaseSetting();

        [VerticalGroup("enabled/model/Setting")]
        [TitleGroup("enabled/model/Setting/Bubble Words Setting")]
        [VerticalGroup("enabled/model/Setting/Bubble Words Setting/Setting")]
        [ToggleGroup("enabled")]
        [LabelText("Words Num")]
        [Tooltip("对话数量")]
        [LabelWidth(65)]
        [Range(0, 2)]
        public int wordNum = 1;

        [ToggleGroup("enabled")]
        [Tooltip("第一段对话id")]
        [VerticalGroup("enabled/model/Setting/Bubble Words Setting/Setting")]
        [LabelWidth(65)]
        [LabelText("ID-1")]
        [ShowIf("@wordNum >= 1")]
        public string wordConfigId1;

        [ToggleGroup("enabled")]
        [Tooltip("第二段对话id")]
        [VerticalGroup("enabled/model/Setting/Bubble Words Setting/Setting")]
        [LabelWidth(65)]
        [LabelText("ID-2")]
        [ShowIf("@wordNum >= 2")]
        public string wordConfigId2;

        [ToggleGroup("enabled")]
        [Tooltip("第一段对话")]
        [VerticalGroup("enabled/model/Setting/Bubble Words Setting/Setting")]
        [LabelWidth(65)]
        [HideLabel]
        [TextArea(2, 3)]
        [ShowIf("@wordNum >= 1")]
        public string word1;

        [ToggleGroup("enabled")]
        [Tooltip("第二段对话")]
        [VerticalGroup("enabled/model/Setting/Bubble Words Setting/Setting")]
        [LabelWidth(65)]
        [HideLabel]
        [TextArea(2, 3)]
        [ShowIf("@wordNum >= 2")]
        public string word2;

        public void InitBubbleTexture()
        {
#if UNITY_EDITOR
            this.bubbleTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(
                $"{PlotDefineUtil.PLOT_BUBBLE_PARENT_FOLDER}/{this.bubbleRes}");
#endif
        }

#if UNITY_EDITOR
        [HorizontalGroup("enabled/model/Setting/Bubble Base Setting/Setting/button", 0.1f)]
        [CustomValueDrawer("DoSelection")]
        [LabelWidth(80)]
        public bool selection;

        private bool DoSelection(bool value, GUIContent label)
        {
            var button = GUILayout.Button(EditorGUIUtility.IconContent("Pick"));
            if (button)
            {
                this.selectionTime = EditorApplication.timeSinceStartup;
            }

            return button;
        }
#endif
    }
}