using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Plot.Runtime
{
    [Serializable]
    public class PlotComicBubbleWordArea
    {
        [TextArea(2, 3)] [HideLabel] public string word;
    }
}