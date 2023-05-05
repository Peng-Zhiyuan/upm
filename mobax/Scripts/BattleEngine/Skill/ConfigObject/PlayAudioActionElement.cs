using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [SkillActionElementItem("音效", 6)]
    public sealed class PlayAudioActionElement : SkillActionElementItem
    {
        public override string Label => "音效";
        [FilePath(ParentFolder = "Assets/res/$Sound")]
        [ToggleGroup("Enabled"), LabelText("音效名称")]
        public string audio;
        [ToggleGroup("Enabled"), LabelText("音量")]
        public float volume = 1;
        [ToggleGroup("Enabled"), LabelText("是否循环"), LabelWidth(80)]
        public bool isLoop = false;

        public override Color GetColor()
        {
            return Colors.AudioFrame;
        }

        public override List<string> GetPreLoadAssetLst()
        {
            List<string> addResPathLst = new List<string>();
            addResPathLst.Add(AddressablePathConst.SkillEditorPathParse(audio));
            return addResPathLst;
        }
    }
}