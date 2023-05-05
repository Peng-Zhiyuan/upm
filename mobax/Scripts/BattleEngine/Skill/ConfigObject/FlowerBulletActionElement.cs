using Sirenix.OdinInspector;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace BattleEngine.Logic
{
    [Serializable]
    [SkillActionElementItem("金鱼姬弹道", 32)]
    public sealed class FlowerBulletActionElement : SkillActionElementItem
    {
        public override string Label => "金鱼姬弹道";
        [FilePath(ParentFolder = "Assets/Arts/FX")]
        [ToggleGroup("Enabled"), LabelText("子弹路径"), LabelWidth(100)]
        public string effectPath = "";
        [ToggleGroup("Enabled"), LabelText("初始相对目标"), LabelWidth(100)]
        public List<string> attachPath = new List<string>();
        [ToggleGroup("Enabled"), LabelText("飞行帧数"), LabelWidth(80)]
        public int FlyFrame;
        [ToggleGroup("Enabled"), LabelText("攻击帧数"), LabelWidth(80)]
        public List<int> AttackFrameLst = new List<int>();
        [ToggleGroup("Enabled"), LabelText("切换帧数"), LabelWidth(80)]
        public List<int> ChangePosFrameLst = new List<int>();
        [ToggleGroup("Enabled"), LabelText("消失帧数"), LabelWidth(80)]
        public int HideFrame;

        public override Color GetColor()
        {
            return Colors.AudioFrame;
        }

        public override List<string> GetPreLoadAssetLst()
        {
            List<string> addResPathLst = new List<string>();
            addResPathLst.Add(AddressablePathConst.SkillEditorPathParse(effectPath));
            return addResPathLst;
        }
    }
}