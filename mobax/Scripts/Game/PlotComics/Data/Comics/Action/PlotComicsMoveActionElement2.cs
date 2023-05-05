using System.Collections.Generic;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using UnityEngine;

namespace Plot.Runtime
{
    [PlotComicsActionElementItem("移动(绝对)", (int) EPlotActionType.MoveAbsolute)]
    public class PlotComicsMoveActionElement2 : PlotComicsActionElementItem
    {
        public virtual string Label => "移动(绝对)";

        [HideInInspector] private GameObject targetObj;

        [ToggleGroup("enabled")] [LabelWidth(100)] [LabelText("Choose Id")] [OnValueChanged("Init")]
        public int chooseId = 0;

        // [LabelWidth(100)]
        // [ValueDropdown("_animClipNames")]
        // [ToggleGroup("enabled"), LabelText("Animation Name")]
        // [OnValueChanged("SetFrame")]
        // public string animClipName = "run";

        [ToggleGroup("enabled")]
        [TitleGroup("enabled/Transform Position Change Setting", "Change Cur Pos ---> Target Value")]
        [HideLabel]
        [Tooltip("开启位移设置")]
        [LabelText("Open Setting")]
        [LabelWidth(100)]
        public bool openPos = false;

        [ToggleGroup("enabled")]
        [VerticalGroup("enabled/Transform Position Change Setting/setting", VisibleIf = "openPos")]
        [LabelText("Preview Mode")]
        [Tooltip("开启预览模式，则场景拖动会刷新视图")]
        [LabelWidth(100)]
        public bool openPreviewMode = false;

        [ToggleGroup("enabled")]
        [VerticalGroup("enabled/Transform Position Change Setting/setting")]
        [LabelText(" Target Value ")]
        [Tooltip("位移目标值")]
        [LabelWidth(100)]
        public Vector3 endPos = Vector3.zero;

        [ToggleGroup("enabled")]
        [VerticalGroup("enabled/Transform Position Change Setting/setting")]
        [LabelText("Anim Curve")]
        [LabelWidth(100)]
        [Tooltip("开启动画曲线")]
        public bool openPosCurve;

        [ToggleGroup("enabled")]
        [VerticalGroup("enabled/Transform Position Change Setting/setting")]
        [LabelText("")]
        [LabelWidth(46)]
        [ShowIf("openPosCurve")]
        public AnimationCurve posCurve;


        #region ---动画基础设置---

        public void Init()
        {
#if UNITY_EDITOR
            var cacheInfo = PlotEditorModelCacheManager.GetModelObj(this.chooseId);
            if (cacheInfo == null) return;
            this.targetObj = cacheInfo.ModelObj;
            this.InitAnimClipNames();
            this.SetAnimationController();
#endif
        }

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
        private IEnumerable<string> _animClipNames;

        public void InitAnimClipNames()
        {
#if UNITY_EDITOR
            var animClipNames = new List<string>();
            animClipNames.Insert(0, "run");
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
                        else if (state.state.motion is BlendTree)
                        {
                            ChildAnimatorState nstate = new ChildAnimatorState();
                            nstate.state = new AnimatorState();
                            BlendTree bt = state.state.motion as BlendTree;
                            nstate.state.motion = bt.children[0].motion;
                            nstate.state.name = state.state.name;
                            this.AnimStates.Add(nstate);
                        }
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
                            else if (state.state.motion is BlendTree)
                            {
                                ChildAnimatorState nstate = new ChildAnimatorState();
                                nstate.state = new AnimatorState();
                                BlendTree bt = state.state.motion as BlendTree;
                                nstate.state.motion = bt.children[0].motion;
                                nstate.state.name = state.state.name;
                                this.AnimStates.Add(nstate);
                            }
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
                        else if (state.state.motion is BlendTree)
                        {
                            ChildAnimatorState nstate = new ChildAnimatorState();
                            nstate.state = new AnimatorState();
                            BlendTree bt = state.state.motion as BlendTree;
                            nstate.state.motion = bt.children[0].motion;
                            nstate.state.name = bt.children[0].motion.name;
                            this.AnimStates.Add(nstate);
                        }
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
                            else if (state.state.motion is BlendTree)
                            {
                                ChildAnimatorState nstate = new ChildAnimatorState();
                                nstate.state = new AnimatorState();
                                BlendTree bt = state.state.motion as BlendTree;
                                nstate.state.motion = bt.children[0].motion;
                                nstate.state.name = bt.children[0].motion.name;
                                this.AnimStates.Add(nstate);
                            }
                        }
                    }
                }
            }
#endif
        }

        #endregion
    }
}