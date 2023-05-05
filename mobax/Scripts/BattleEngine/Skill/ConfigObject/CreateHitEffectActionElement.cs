using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [SkillActionElementItem("受击特效", 16)]
    public sealed class CreateHitEffectActionElement : SkillActionElementItem
    {
        public override string Label => "受击特效";
        [FilePath(ParentFolder = "Assets/Arts/FX")]
        [ToggleGroup("Enabled"), LabelText("特效")]
        public string res;
        [ToggleGroup("Enabled"), LabelText("特效大小")]
        public Vector3 resScale = Vector3.one;
        [ToggleGroup("Enabled"), LabelText("速度改变"), Space(10)]
        [ListDrawerSettings(Expanded = false, DraggableItems = true, ShowItemCount = true, HideAddButton = false)]
        [HideReferenceObjectPicker]
        public List<AnimSpeed> speedModify = new List<AnimSpeed>();
        [ToggleGroup("Enabled"), LabelText("绑点")]
        public string attachPoint = "";
        [ToggleGroup("Enabled"), LabelText("特效偏移")]
        public Vector3 offset = Vector3.zero;
        [ToggleGroup("Enabled"), LabelText("特效旋转")]
        public Vector3 euler = Vector3.zero;

        [FilePath(ParentFolder = "Assets/res/$Sound")]
        [ToggleGroup("Enabled"), LabelText("受击音效名称"), LabelWidth(120)]
        public string audio;
        [ToggleGroup("Enabled"), LabelText("受击音效音量"), LabelWidth(120)]
        public float volume = 1;
        [ToggleGroup("Enabled"), LabelText("受击动画"), LabelWidth(120)]
        public string hitAim = "";

        public override Color GetColor()
        {
            return Colors.HurtBoxFrame;
        }

        public override List<string> GetPreLoadAssetLst()
        {
            List<string> addResPathLst = new List<string>();
            addResPathLst.Add(AddressablePathConst.SkillEditorPathParse(res));
            addResPathLst.Add(AddressablePathConst.SkillEditorPathParse(audio));
            return addResPathLst;
        }
    }
}