using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [SkillActionElementItem("Timeline", 26)]
    public sealed class FeatureTimeLineElement : SkillActionElementItem
    {
        public override string Label => "特写TimeLine";
        [FilePath(ParentFolder = "Assets/Arts/SkillTimeline")]
        [ToggleGroup("Enabled"), LabelText("TimeLinePath")]
        [LabelWidth(100)]
        public string timeLinePath;

        public override Color GetColor()
        {
            return Colors.TimeLineFrame;
        }

        public override List<string> GetPreLoadAssetLst()
        {
            List<string> addResPathLst = new List<string>();
            addResPathLst.Add(AddressablePathConst.SkillEditorPathParse(timeLinePath));
            return addResPathLst;
        }
    }
}