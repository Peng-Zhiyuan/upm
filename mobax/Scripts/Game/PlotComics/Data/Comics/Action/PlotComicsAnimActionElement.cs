using System.Collections.Generic;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using UnityEngine;

namespace Plot.Runtime
{
    [PlotComicsActionElementItem("模型动作", (int) EPlotActionType.Animation)]
    public class PlotComicsAnimActionElement : PlotComicsActionElementItem
    {
        public virtual string Label => "模型动作";

#if UNITY_EDITOR
        public AnimationClip[] GetAnimClips()
        {
            return this.AnimClips;
        }

        public List<ChildAnimatorState> GetAnimStates()
        {
            return this.AnimStates;
        }

        [HideInInspector] public AnimationClip[] AnimClips { get; private set; }
        [HideInInspector] public List<ChildAnimatorState> AnimStates { get; private set; }
        [HideInInspector] public List<ChildAnimatorStateMachine> AnimChildMachines { get; private set; }
#endif

        [HideInInspector] private GameObject targetObj;

        [ValueDropdown("GetTargetObjNames")]
        [ToggleGroup("enabled"), LabelText("目标组件:")]
        [OnValueChanged("SetTargetObj")]
        [LabelWidth(80)]
        public string targetObjName = "(场景模型)-未选中";

        [LabelWidth(80)]
        // [ValueDropdown("_animClipNames")]
        [ToggleGroup("enabled"), LabelText("唯一ID:")]
        // [OnValueChanged("SetFrame")]
        public int chooseId;

        [LabelWidth(80)]
        [ValueDropdown("_animClipNames")]
        [ToggleGroup("enabled"), LabelText("动画名称:")]
        [OnValueChanged("SetFrame")]
        public string animClipName = "(场景模型)-未选中";

        private IEnumerable<string> _animClipNames;

        #region ---目标组件获取 & 设置---

        /// <summary>
        /// 获取场景内所有的目标组件
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GetTargetObjNames()
        {
            GameObject modelRoot = GameObject.Find("ModelRoot");
            List<string> targetNames = new List<string>();
            targetNames.Insert(0, "(场景模型)-未选中");
            if (modelRoot.transform.childCount <= 0) return targetNames;

            for (int i = 0; i < modelRoot.transform.childCount; i++)
            {
                var go = modelRoot.transform.GetChild(i);
                targetNames.Add(go.name);
            }

            return targetNames;
        }

        public void Init()
        {
            this.SetTargetObj();
        }

        private void SetTargetObj()
        {
            this.targetObj = this.targetObjName.Equals("(场景模型)-未选中")
                ? null
                : GameObject.Find($"ModelRoot/{this.targetObjName}");
            this.InitAnimClipNames();
            this.SetAnimationController();
        }

        #endregion

        #region ---目标动画获取 & 设置---

        private void SetFrame()
        {
            this.startFrame = 0;

#if UNITY_EDITOR
            var stateData = this.AnimStates.Find(val => val.state.motion.name.Equals(this.animClipName));
            var clipData = stateData.state.motion as AnimationClip;

            if (clipData == null)
            {
                this.endFrame = 0;
            }
            else
            {
                this.endFrame = (int) (clipData.length * PlotDefineUtil.DEFAULT_FRAME_NUM);
            }
#endif
        }

        public void InitAnimClipNames()
        {
#if UNITY_EDITOR
            var animClipNames = new List<string>();
            animClipNames.Insert(0, "(动画名称)-未选中");
            if (this.targetObj == null)
            {
                this._animClipNames = animClipNames;
                return;
            }
            
            var animator = this.targetObj.GetComponentInChildren<Animator>();
            
            List<ChildAnimatorState> states = new List<ChildAnimatorState>();
            AnimationClip[] animClips = animator.runtimeAnimatorController.animationClips;

            AnimatorController ac = null;
            if (animator.runtimeAnimatorController is AnimatorController)
            {
                ac = (AnimatorController) animator.runtimeAnimatorController;
                
                foreach (var animClip in animClips)
                {
                    animClipNames.Add(animClip.name);
                }
                
                // AnimatorControllerParameter[] parameters = ac.parameters;
                // if (parameters == null || parameters.Length <= 0)
                // {
                //     foreach (var animClip in animClips)
                //     {
                //         animClipNames.Add(animClip.name);
                //     }
                // }
                // else
                // {
                //     for (int i = 0; i < parameters.Length; i++)
                //     {
                //         animClipNames.Add(parameters[i].name);
                //     }
                // }
            }
            else
            {
                AnimatorOverrideController aco = (AnimatorOverrideController) animator.runtimeAnimatorController;
                ac = (AnimatorController) aco.runtimeAnimatorController;

                foreach (var animClip in animClips)
                {
                    animClipNames.Add(animClip.name);
                }
                // AnimatorControllerParameter[] parameters = ac.parameters;
                // // 如果parameters = 空 直接取animationClips
                // if (parameters == null || parameters.Length <= 0)
                // {
                //     foreach (var animClip in animClips)
                //     {
                //         animClipNames.Add(animClip.name);
                //     }
                // }
                // else
                // {
                //     for (int i = 0; i < parameters.Length; i++)
                //     {
                //         animClipNames.Add(parameters[i].name);
                //     }
                // }
            }

            this._animClipNames = animClipNames;
#else
            // animClipNames = PlotEditorDefine.ActionStyles;
            // animClipNames.Insert(0, "(动画名称)-未选中");
            // this._animClipNames = animClipNames;
#endif
        }

        private void SetAnimationController(int layerIndex = 0)
        {
#if UNITY_EDITOR
            if (this.targetObj == null) return;
            var animator = this.targetObj.GetComponentInChildren<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                this.AnimClips = animator.runtimeAnimatorController.animationClips;
                AnimatorController ac = null;
                if (animator.runtimeAnimatorController is AnimatorController)
                {
                    ac = (AnimatorController) animator.runtimeAnimatorController;
                    AnimatorControllerLayer[] layers = ac.layers;
                    //Debug.LogError(layers.Length);
                    this.AnimStates = new List<ChildAnimatorState>();
                    ChildAnimatorState[] tstates = layers[layerIndex].stateMachine.states;
                    foreach (ChildAnimatorState state in tstates)
                    {
                        if (state.state.motion is AnimationClip)
                        {
                            this.AnimStates.Add(state);
                        }
                        // else if (state.state.motion is BlendTree)
                        // {
                        //     ChildAnimatorState nstate = new ChildAnimatorState();
                        //     nstate.state = new AnimatorState();
                        //     BlendTree bt = state.state.motion as BlendTree;
                        //     nstate.state.motion = bt.children[0].motion;
                        //     nstate.state.name = state.state.name;
                        //     this.AnimStates.Add(nstate);
                        // }
                    }

                    ChildAnimatorStateMachine[] stateMachines = layers[layerIndex].stateMachine.stateMachines;
                    foreach (ChildAnimatorStateMachine machine in stateMachines)
                    {
                        tstates = machine.stateMachine.states;
                        foreach (ChildAnimatorState state in tstates)
                        {
                            if (state.state.motion is AnimationClip)
                            {
                                this.AnimStates.Add(state);
                            }
                            // else if (state.state.motion is BlendTree)
                            // {
                            //     ChildAnimatorState nstate = new ChildAnimatorState();
                            //     nstate.state = new AnimatorState();
                            //     BlendTree bt = state.state.motion as BlendTree;
                            //     nstate.state.motion = bt.children[0].motion;
                            //     nstate.state.name = state.state.name;
                            //     this.AnimStates.Add(nstate);
                            // }
                        }
                    }
                }
                else
                {
                    AnimatorOverrideController aco = (AnimatorOverrideController) animator.runtimeAnimatorController;
                    ac = (AnimatorController) aco.runtimeAnimatorController;
                    AnimatorControllerLayer[] layers = ac.layers;
                    //Debug.LogError(layers.Length);
                    this.AnimStates = new List<ChildAnimatorState>();
                    ChildAnimatorState[] tstates = layers[layerIndex].stateMachine.states;
                    foreach (ChildAnimatorState state in tstates)
                    {
                        if (state.state.motion is AnimationClip)
                        {
                            this.AnimStates.Add(state);
                        }
                        // else if (state.state.motion is BlendTree)
                        // {
                        //     ChildAnimatorState nstate = new ChildAnimatorState();
                        //     nstate.state = new AnimatorState();
                        //     BlendTree bt = state.state.motion as BlendTree;
                        //     nstate.state.motion = bt.children[0].motion;
                        //     nstate.state.name = bt.children[0].motion.name;
                        //     this.AnimStates.Add(nstate);
                        // }
                    }

                    ChildAnimatorStateMachine[] stateMachines = layers[layerIndex].stateMachine.stateMachines;
                    foreach (ChildAnimatorStateMachine machine in stateMachines)
                    {
                        tstates = machine.stateMachine.states;
                        foreach (ChildAnimatorState state in tstates)
                        {
                            if (state.state.motion is AnimationClip)
                            {
                                this.AnimStates.Add(state);
                            }
                            // else if (state.state.motion is BlendTree)
                            // {
                            //     ChildAnimatorState nstate = new ChildAnimatorState();
                            //     nstate.state = new AnimatorState();
                            //     BlendTree bt = state.state.motion as BlendTree;
                            //     nstate.state.motion = bt.children[0].motion;
                            //     nstate.state.name = bt.children[0].motion.name;
                            //     this.AnimStates.Add(nstate);
                            // }
                        }
                    }
                }
            }
#endif
        }

        #endregion
    }
}