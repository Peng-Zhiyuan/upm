using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [SkillActionElementItem("盾墙", 20)]
    public class CreatShieldWallActionElement : SkillActionElementItem
    {
        public override string Label => "盾墙";
        [FilePath(ParentFolder = "Assets/Arts/FX")]
        [ToggleGroup("Enabled"), LabelText("特效")]
        public string effectPrefab;
        [FilePath(ParentFolder = "Assets/Arts/FX")]
        [ToggleGroup("Enabled"), LabelText("撞击特效")]
        public string hitEffectPrefab;
        [FilePath(ParentFolder = "Assets/Arts/FX")]
        [ToggleGroup("Enabled"), LabelText("消失特效")]
        public string destoryEffectPrefab;
        [ToggleGroup("Enabled"), LabelText("位置偏移")]
        public Vector3 offset = Vector3.one;
        [ToggleGroup("Enabled"), LabelText("角度偏移")]
        public Vector3 angleOffset = Vector3.one;
        [ToggleGroup("Enabled"), LabelText("缩放")]
        public Vector3 scale = Vector3.one;
        [ToggleGroup("Enabled"), LabelText("是否跟着角色移动")]
        public bool isAttachLock = false;
        [ToggleGroup("Enabled"), LabelText("是否跟着角色旋转"), LabelWidth(100)]
        public bool isAttachLookAt = true;
        [ToggleGroup("Enabled"), LabelText("盾墙形状")]
        public SHIELD_WALL_TYPE wallTYpe = SHIELD_WALL_TYPE.Cube;
        [ToggleGroup("Enabled"), SuffixLabel("毫秒", true), LabelText("持续时间")]
        public int durationTime = 1;
        [ToggleGroup("Enabled"), LabelText("抵挡次数")]
        public int times = 1;
        [ToggleGroup("Enabled"), SuffixLabel("圆形使用", true), LabelText("半径")]
        public float radius = 1;
        [ToggleGroup("Enabled"), SuffixLabel("矩形使用", true), LabelText("尺寸")]
        public Vector3 size = new Vector3(0, 6, 0);

        public override Color GetColor()
        {
            return Colors.ShieldWallFrame;
        }

        public override List<string> GetPreLoadAssetLst()
        {
            List<string> addResPathLst = new List<string>();
            addResPathLst.Add(AddressablePathConst.SkillEditorPathParse(effectPrefab));
            addResPathLst.Add(AddressablePathConst.SkillEditorPathParse(hitEffectPrefab));
            addResPathLst.Add(AddressablePathConst.SkillEditorPathParse(destoryEffectPrefab));
            return addResPathLst;
        }
    }
}