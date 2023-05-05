namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;
    using Sirenix.OdinInspector;
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    [Serializable]
    [CreateAssetMenu(fileName = "技能配置", menuName = "技能|状态/技能配置")]
    [LabelText("技能配置")]
    public sealed class SkillConfigObject : SerializedScriptableObject
    {
        [OnValueChanged("DataChange")]
        [HorizontalGroup("Split", 0.5f, LabelWidth = 80)]
        [BoxGroup("Split/Left")]
        [LabelText("技能ID"), DelayedProperty]
        public uint ID;
        [BoxGroup("Split/Left")]
        [LabelText("技能名称"), DelayedProperty]
        public string Name = "技能1";
#if UNITY_EDITOR
        [BoxGroup("Split/Left")]
        [FilePath(ParentFolder = "Assets/Arts/Models"), LabelText("默认英雄")]
        public string defaultActorRes;
        [BoxGroup("Split/Left")]
        [LabelText("英雄大小")]
        public Vector3 defaultActorScale = Vector3.one;
#endif
        [BoxGroup("Split/Left")]
        public int fps = 30;
        [BoxGroup("Split/Left")]
        public int totalFrame = 0;

        [BoxGroup("Split/Left")]
        [LabelText("技能目标类型")]
        public SKILL_AFFECT_TARGET_TYPE AffectTargetType;
        [BoxGroup("Split/Left")]
        [LabelText("索敌类型")]
        public LOCK_DOWN_TARGET_TYPE lockDownTargetType = LOCK_DOWN_TARGET_TYPE.None;
        [BoxGroup("Split/Left")]
        [LabelText("索敌数量")]
        public int lockDownTargetNum = 1;
        [BoxGroup("Split/Left")]
        [LabelText("是否重置目标")]
        public bool ResetTarget = false;

        [BoxGroup("Split/Left")]
        [LabelText("启动StartUP"), LabelWidth(120)]
        public SkillFlag StartUPFlag = new SkillFlag() { flagFrame = 0, flagCause = SKILL_BREAK_CAUSE.ManualMove | SKILL_BREAK_CAUSE.ManualAttack | SKILL_BREAK_CAUSE.UltraSkill | SKILL_BREAK_CAUSE.Focus };
        [BoxGroup("Split/Left")]
        [LabelText("生效Active"), LabelWidth(120)]
        public SkillFlag ActiveFlag = new SkillFlag();
        [BoxGroup("Split/Left")]
        [LabelText("收招FollowThrough"), LabelWidth(120)]
        public SkillFlag FollowThroughFlag = new SkillFlag();
        [BoxGroup("Split/Left")]
        [LabelText("恢复Recovery"), LabelWidth(120)]
        public SkillFlag RecoveryFlag = new SkillFlag();
        [BoxGroup("Split/Left")]
        [LabelText("恢复跳转下一技能"), LabelWidth(120)]
        public bool RecoveryFlagBreak = true;

        [BoxGroup("Split/Left")]
        [LabelText("需要角色姿态")]
        public ACTOR_POSTURE_TYPE needPostureType = ACTOR_POSTURE_TYPE.NORMAL;
        [BoxGroup("Split/Left")]
        [LabelText("加速结束帧")]
        public int StartUpFrame = 0;
        [BoxGroup("Split/Left")]
        [LabelText("释放时可滑动"), LabelWidth(180)]
        public bool SlideSkill = false;
        [BoxGroup("Split/Left")]
        [LabelText("释放时滑动速度"), LabelWidth(180)]
        public float SlideSpeed = 0;
        [BoxGroup("Split/Left")]
        [LabelText("是否需要标记目标"), LabelWidth(180)]
        public bool tagTarget = false;
        [BoxGroup("Split/Left")]
        [LabelText("是否需要伤害预判"), LabelWidth(180)]
        public bool PreCheckTrigger = false;
        [BoxGroup("Split/Left")]
        [LabelText("伤害预判中心"), LabelWidth(180), ShowIf("PreCheckTrigger")]
        public Vector3 PreCheckCenter;
        [BoxGroup("Split/Left")]
        [LabelText("伤害预判半径"), LabelWidth(180), ShowIf("PreCheckTrigger")]
        public float PreCheckRadius = 0;
        [BoxGroup("Split/Left")]
        [LabelText("伤害预判失败使用技能"), LabelWidth(180), ShowIf("PreCheckTrigger")]
        public uint PreCheckFailSkill = 0;
        [BoxGroup("Split/Left")]
        [LabelText("执行技能时是否可拖动"), LabelWidth(180)]
        public bool CanbeDragged = false;
        [BoxGroup("Split/Left")]
        [LabelText("当前目标死亡是否立刻中断技能"), LabelWidth(180)]
        public bool NoTargetNeedBreak = false;

        [BoxGroup("Split/Right")]
        [LabelText("行为列表")]
        [ListDrawerSettings(Expanded = true, DraggableItems = true, ShowItemCount = true, HideAddButton = true, NumberOfItemsPerPage = 100)]
        [HideReferenceObjectPicker]
        public List<SkillActionElementItem> actionElements = new List<SkillActionElementItem>();

        [BoxGroup("Split/Right")]
        [HideLabel]
        [OnValueChanged("AddActionElement")]
        [ValueDropdown("ActionElementTypeSelect")]
        public string ActionElementTypeName = "(添加行为)";

        public IEnumerable<string> ActionElementTypeSelect()
        {
            var types = typeof(SkillActionElementItem).Assembly.GetTypes().Where(x => !x.IsAbstract).Where(x => typeof(SkillActionElementItem).IsAssignableFrom(x)).Where(x => x.GetCustomAttribute<SkillActionElementItemAttribute>() != null).OrderBy(x => x.GetCustomAttribute<SkillActionElementItemAttribute>().Order).Select(x => x.GetCustomAttribute<SkillActionElementItemAttribute>().ElementType);
            var results = types.ToList();
            results.Insert(0, "(添加行为)");
            return results;
        }

        private void AddActionElement()
        {
            if (ActionElementTypeName != "(添加行为)")
            {
                var effectType = typeof(SkillActionElementItem).Assembly.GetTypes().Where(x => !x.IsAbstract).Where(x => typeof(SkillActionElementItem).IsAssignableFrom(x)).Where(x => x.GetCustomAttribute<SkillActionElementItemAttribute>() != null).Where(x => x.GetCustomAttribute<SkillActionElementItemAttribute>().ElementType == ActionElementTypeName).First();
                var effect = Activator.CreateInstance(effectType) as SkillActionElementItem;
                effect.type = (SKILL_ACTION_ELEMENT_TYPE)effectType.GetCustomAttribute<SkillActionElementItemAttribute>().Order;
                actionElements.Add(effect);
                ActionElementTypeName = "(添加行为)";
            }
        }

        private void DataChange() { }

#if UNITY_EDITOR
        public void RenameFile()
        {
            string assetPath = string.Format("Assets/res/$Data_ahead/SkillConfigs/{0}.asset", this.name);
            var fileName = Path.GetFileName(assetPath);
            var newName = $"skill_{this.ID}";
            UnityEditor.AssetDatabase.RenameAsset(assetPath, newName);
        }

        public void RenameJsonFile()
        {
            string assetPath = string.Format("Assets/res/$Data_ahead/SkillConfigs/JSON/{0}.json", this.name);
            var fileName = Path.GetFileName(assetPath);
            var newName = $"skill_{this.ID}";
            UnityEditor.AssetDatabase.RenameAsset(assetPath, newName);
        }
#endif

#if !Server
        public string[] GetAllElementsResPath()
        {
            Dictionary<string, string> resDic = new Dictionary<string, string>();
            List<string> resPathLst = new List<string>();
            for (int i = 0; i < actionElements.Count; i++)
            {
                resPathLst = actionElements[i].GetPreLoadAssetLst();
                if (resPathLst == null)
                {
                    continue;
                }
                for (int j = 0; j < resPathLst.Count; j++)
                {
                    if (string.IsNullOrEmpty(resPathLst[j])
                        || resDic.ContainsKey(resPathLst[j]))
                    {
                        continue;
                    }
                    resDic[resPathLst[j]] = resPathLst[j];
                }
            }
            return resDic.Keys.ToArray();
        }

        public string[] GetElementsResPath(SKILL_ACTION_ELEMENT_TYPE type)
        {
            Dictionary<string, string> resDic = new Dictionary<string, string>();
            List<string> resPathLst = new List<string>();
            for (int i = 0; i < actionElements.Count; i++)
            {
                if (actionElements[i].type != type)
                {
                    continue;
                }
                resPathLst = actionElements[i].GetPreLoadAssetLst();
                if (resPathLst == null)
                {
                    continue;
                }
                for (int j = 0; j < resPathLst.Count; j++)
                {
                    if (string.IsNullOrEmpty(resPathLst[j])
                        || resDic.ContainsKey(resPathLst[j]))
                    {
                        continue;
                    }
                    resDic[resPathLst[j]] = resPathLst[j];
                }
            }
            return resDic.Keys.ToArray();
        }
#endif

        public byte[] GetSkillConfigObjectBytes()
        {
            byte[] bytes = BitConverter.GetBytes(this);
            return bytes;
        }
    }

    [LabelText("护盾类型")]
    public enum ShieldType
    {
        [LabelText("普通护盾")]
        Shield,
        [LabelText("物理护盾")]
        PhysicShield,
        [LabelText("魔法护盾")]
        MagicShield,
        [LabelText("技能护盾")]
        SkillShield,
    }

    [LabelText("标记类型")]
    public enum TagType
    {
        [LabelText("能量标记")]
        Power,
    }

    [Serializable]
    public class SkillFlag
    {
        [LabelText("flag帧")]
        public int flagFrame = -1;
        public SKILL_BREAK_CAUSE flagCause;
    }
}