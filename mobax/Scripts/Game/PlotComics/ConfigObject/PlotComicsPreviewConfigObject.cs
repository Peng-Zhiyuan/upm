using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace  Plot.Runtime
{
    [CreateAssetMenu(fileName = "漫画预览配置", menuName = "剧情预览配置")]
    public class PlotComicsPreviewConfigObject : SerializedScriptableObject
    {
        [BoxGroup("预览列表")]
        [LabelText(" ")]
        [ListDrawerSettings(Expanded = true, DraggableItems = false, ShowItemCount = true, NumberOfItemsPerPage = 10)]
        [HideReferenceObjectPicker]
        public List<PlotComicsPreviewInfo> previewList ;

    }
}