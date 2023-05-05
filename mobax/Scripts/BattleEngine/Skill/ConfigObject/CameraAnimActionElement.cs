using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace BattleEngine.Logic
{
    [Serializable]
    [SkillActionElementItem("相机动画", 13)]
    public sealed class CameraAnimActionElement : SkillActionElementItem
    {
        public override string Label => "相机动画";
        [ToggleGroup("Enabled"), LabelText("坐标"), LabelWidth(120)]
        public SKILL_CAM_POSITION_TYPE type = SKILL_CAM_POSITION_TYPE.Local;
        [FilePath(ParentFolder = "Assets/Arts/Models")]
        [ToggleGroup("Enabled"), LabelText("动画")]
        public string camPathPrefab;
        [ToggleGroup("Enabled"), LabelText("动画名")]
        public string camName;
        [ToggleGroup("Enabled"), LabelText("开始偏移"), LabelWidth(120)]
        public Vector3 offset = Vector3.zero;
        [ToggleGroup("Enabled"), LabelText("动画速度"), LabelWidth(120)]
        public float timeScale = 1;

        public override Color GetColor()
        {
            return Colors.CameraAniFrame;
        }

        public override List<string> GetPreLoadAssetLst()
        {
            List<string> addResPathLst = new List<string>();
            addResPathLst.Add(AddressablePathConst.SkillEditorPathParse(camPathPrefab));
            return addResPathLst;
        }
    }
}