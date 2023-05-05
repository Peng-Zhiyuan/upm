using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

namespace BattleEngine.Logic
{
    [System.Serializable]
    [SkillActionElementItem("动画", 1)]
    public sealed class PlayAnimActionElement : SkillActionElementItem
    {
        public override string Label => "动画";
        [ValueDropdown("GetListOfMonoBehaviours")]
        [ToggleGroup("Enabled"), LabelText("动画名称")]
        public string anim;
        public string animClipName
        {
            get { return anim; }
        }
        [ToggleGroup("Enabled"), LabelText("速度改变"), Space(10)]
        [ListDrawerSettings(Expanded = false, DraggableItems = true, ShowItemCount = true, HideAddButton = false)]
        [HideReferenceObjectPicker]
        public List<AnimSpeed> speedModify = new List<AnimSpeed>();
        [ToggleGroup("Enabled"), LabelText("结束立刻切回待机"), LabelWidth(120)]
        public bool isExitToIdel = false;

        public override Color GetColor()
        {
            return Colors.AnimationFrame;
        }

        private IEnumerable<string> GetListOfMonoBehaviours()
        {
#if UNITY_EDITOR
            GameObject root = GameObject.Find("HeroNode");
            Animator anim = root.GetComponentInChildren<Animator>();
            List<ChildAnimatorState> states = new List<ChildAnimatorState>();
            AnimationClip[] animeClips = anim.runtimeAnimatorController.animationClips;
            List<string> animNames = new List<string>();
            AnimatorController ac = null;
            if (anim.runtimeAnimatorController is AnimatorController)
            {
                ac = (AnimatorController)anim.runtimeAnimatorController;
                AnimatorControllerParameter[] parameters = ac.parameters;
                for (int i = 0; i < parameters.Length; i++)
                {
                    animNames.Add(parameters[i].name);
                }
            }
            else
            {
                AnimatorOverrideController aco = (AnimatorOverrideController)anim.runtimeAnimatorController;
                ac = (AnimatorController)aco.runtimeAnimatorController;
                AnimatorControllerParameter[] parameters = ac.parameters;
                for (int i = 0; i < parameters.Length; i++)
                {
                    animNames.Add(parameters[i].name);
                }
            }
            return animNames.ToArray();
#else
            return new string[] { };
#endif
        }
    }

    [Serializable]
    public struct AnimSpeed
    {
        [LabelWidth(100)]
        public int Frame;
        [LabelWidth(100)]
        public float value;
    }
}