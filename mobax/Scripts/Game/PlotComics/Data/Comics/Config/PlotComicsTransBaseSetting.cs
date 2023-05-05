using Sirenix.OdinInspector;
using UnityEngine;

namespace Plot.Runtime
{
    public class PlotComicsTransBaseSetting
    {
        [LabelText(" Position ")] [Tooltip("初始位置")] [LabelWidth(65)]
        public Vector3 startPos = Vector3.zero;

        [LabelText(" Rotation ")] [Tooltip("初始旋转")] [LabelWidth(65)]
        public Vector3 startRotation = Vector3.zero;

        [LabelText(" Scale ")] [Tooltip("初始缩放")] [LabelWidth(65)]
        public Vector3 startScale = Vector3.one;

        [LabelText("Alpha")] [Tooltip("初始透明度")] [LabelWidth(65)] [Range(0, 1)]
        public float startAlpha = 1;
    }
}