using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [SkillActionElementItem("虚拟相机", 27)]
    public sealed class VirtulCameraElement : SkillActionElementItem
    {
        public override string Label => "虚拟相机";
        [ValueDropdown("GetListOfMonoBehaviours")]
        [OnValueChanged("RefreshFrame")]
        [ToggleGroup("Enabled"), LabelText("动画名称")]
        public string anim;
        public string animClipName
        {
            get { return anim; }
        }

        public override Color GetColor()
        {
            return Colors.VirtulCameraFrame;
        }

        public override void StartFrameChanged()
        {
            RefreshFrame();
        }

        private void RefreshFrame()
        {
            GameObject root = GameObject.Find("skillcam");
            if (root == null)
            {
                return;
            }
            Animator anim = root.GetComponent<Animator>();
            AnimationClip[] animeClips = anim.runtimeAnimatorController.animationClips;
            List<string> animNames = new List<string>();
            foreach (var VARIABLE in animeClips)
            {
                if (VARIABLE.name == animClipName)
                {
                    endFrame = startFrame + Mathf.FloorToInt(VARIABLE.length * 30f);
                    break;
                }
            }
        }

        private IEnumerable<string> GetListOfMonoBehaviours()
        {
#if UNITY_EDITOR
            GameObject root = GameObject.Find("skillcam");
            if (root == null)
            {
                return new string[] { };
            }
            Animator anim = root.GetComponent<Animator>();
            AnimationClip[] animeClips = anim.runtimeAnimatorController.animationClips;
            List<string> animNames = new List<string>();
            foreach (var VARIABLE in animeClips)
            {
                animNames.Add(VARIABLE.name);
            }
            return animNames.ToArray();
#else
            return new string[] { };
#endif
        }
    }
}