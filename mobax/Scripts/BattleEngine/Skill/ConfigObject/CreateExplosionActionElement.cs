using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [SkillActionElementItem("爆炸", 11)]
    public sealed class CreateExplosionActionElement : SkillActionElementItem
    {
        public override string Label => "爆炸";
        [FilePath(ParentFolder = "Assets/Arts/FX")]
        [ToggleGroup("Enabled"), LabelText("特效")]
        public string res;
        [ToggleGroup("Enabled"), LabelText("缩放")]
        public Vector3 scale = Vector3.one;
        [ToggleGroup("Enabled"), LabelText("速度改变"), Space(10)]
        [ListDrawerSettings(Expanded = false, DraggableItems = true, ShowItemCount = true, HideAddButton = false)]
        [HideReferenceObjectPicker]
        public List<AnimSpeed> speedModify = new List<AnimSpeed>();
        [ToggleGroup("Enabled"), LabelText("开始半径")]
        public float radiusStart = 1;
        [ToggleGroup("Enabled"), LabelText("结束半径")]
        public float radiusEnd = 1;

        public override Color GetColor()
        {
            return Colors.ExplosionFrame;
        }

        public override List<string> GetPreLoadAssetLst()
        {
            List<string> addResPathLst = new List<string>();
            addResPathLst.Add(AddressablePathConst.SkillEditorPathParse(res));
            return addResPathLst;
        }
    }
}