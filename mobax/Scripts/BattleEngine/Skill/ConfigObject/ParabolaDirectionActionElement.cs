using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [SkillActionElementItem("弹道指示抛物线", 22)]
    public sealed class ParabolaDirectionActionElement : SkillActionElementItem
    {
        public override string Label => "弹道指示抛物线";
        [FilePath(ParentFolder = "Assets/Arts/FX")]
        [ToggleGroup("Enabled"), LabelText("特效")]
        public string res = "";
        [ToggleGroup("Enabled"), LabelText("起点绑点"), LabelWidth(120)]
        public string attachStartPoint = "";
        [ToggleGroup("Enabled"), LabelText("终点绑点"), LabelWidth(120)]
        public string attachEndPoint = "";
        [ToggleGroup("Enabled"), LabelText("初始点偏移"), LabelWidth(120)]
        public Vector3 startOffset = Vector3.zero;
        [ToggleGroup("Enabled"), LabelText("终点偏移"), LabelWidth(120)]
        public Vector3 endOffset = Vector3.zero;
        [ToggleGroup("Enabled"), LabelText("抛物线高度"), LabelWidth(120)]
        public float height = 1;
        [ToggleGroup("Enabled"), LabelText("采样数量"), LabelWidth(120)]
        public int samplingNum = 10;

        public override Color GetColor()
        {
            return Colors.ParabolaDirectionFrame;
        }

        public override List<string> GetPreLoadAssetLst()
        {
            List<string> addResPathLst = new List<string>();
            addResPathLst.Add(AddressablePathConst.SkillEditorPathParse(res));
            return addResPathLst;
        }
    }
}