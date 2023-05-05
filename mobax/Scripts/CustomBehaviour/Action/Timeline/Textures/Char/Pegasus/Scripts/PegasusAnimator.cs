using System;
using UnityEngine;

namespace Pegasus
{
    /// <summary>
    /// This class can be attached to Mecanim based objects to change the animation state based on the speed at which the object is travelling.
    /// </summary>
    public class PegasusAnimator : MonoBehaviour
    {
        //Whats being controlled, and current state
        [Header("Character And Animator")]
        [Tooltip("Drop your character here. By default it will select the character this script is attached to.")]
        public Transform m_character;
        [Tooltip("Drop your animator here. By default it will select the animator on the character this script is attached to.")]
        public Animator m_animator;
        [Tooltip("Select your initial animation state. This will be played immediately on start at runtime.")]
        public PegasusConstants.PegasusAnimationState m_animationState = PegasusConstants.PegasusAnimationState.Idle;
        private PegasusConstants.PegasusAnimationState m_lastAnimationState = PegasusConstants.PegasusAnimationState.Idle;

        //Override the animations that get played - changed only at the start of the playback
        [Header("Optional Animation Overrides")]
        [Tooltip("Add your idle animation override here. This is optional, and the default animation will be used instead if not supplied.")]
        public AnimationClip m_idleAnimation;
        [Tooltip("Add your walk animation override here. This is optional, and the default animation will be used instead if not supplied.")]
        public AnimationClip m_walkAnimation;
        [Tooltip("Add your run animation override here. This is optional, and the default animation will be used instead if not supplied.")]
        public AnimationClip m_runAnimation;

        //Typical idle, walk & run speeds - override to influence animation speeds
        [Header("Walk & Run Speeds (m/sec)")]
        [Tooltip("Walk animations will play when the character movement is greater than the idle speed and less than this speed.")]
        public float m_walkSpeed = 2f;
        [Tooltip("Walk animations will play when the character movement is greater than the idle speed and less than this speed.")]
        public float m_maxWalkSpeed = 3.5f;
        [Tooltip("Run animations will play when the character movement is greater than the walk speed.")]
        public float m_runSpeed = 7f;

        //Internal variables
        private float m_speedDamping = 0.7f;
        private float m_speed = 0f;
        private float m_lastSpeed = float.MinValue;
        private Vector3 m_lastPosition = Vector3.zero;
        private float m_animationMultiplier = 1f;
        private int m_animationStateHash = Animator.StringToHash("AnimationState");
        private int m_animationMultiplierHash = Animator.StringToHash("AnimationMultiplier");

        void Start()
        {
            if (m_character == null)
            {
                m_character = gameObject.transform;
            }

            if (m_animator == null)
            {
                m_animator = GetComponent<Animator>();
            }

            if (m_animator != null)
            {
                m_lastPosition = transform.position;

                if (m_idleAnimation != null)
                {
                    ReplaceClip(m_animator, "HumanoidIdle", m_idleAnimation);
                }
                if (m_walkAnimation != null)
                {
                    ReplaceClip(m_animator, "HumanoidWalk", m_walkAnimation);
                }
                if (m_runAnimation != null)
                {
                    ReplaceClip(m_animator, "HumanoidRun", m_runAnimation);
                }

                PlayState(m_animationState, true);
            }
        }

        // Update is called once per frame
        void Update()
        {
            //Exit if we are not connected to anything
            if (m_animator == null)
            {
                return;
            }

            float distance = Vector3.Distance(transform.position, m_lastPosition);
            m_lastPosition = transform.position;

            if (Time.deltaTime > 0f)
            {
                //Get damped speed - damping smooths out frame rate variations a little
                m_speed = Mathf.Lerp(m_speed, distance / Time.deltaTime, Time.deltaTime * (1f / m_speedDamping));

                //Determine what state we should be in, and adjust animation multiplier
                if (m_speed > m_maxWalkSpeed)
                {
                    m_animationState = PegasusConstants.PegasusAnimationState.Running;
                    m_animationMultiplier = m_speed / m_runSpeed;
                }
                else if (m_speed > 0f)
                {
                    m_animationState = PegasusConstants.PegasusAnimationState.Walking;
                    m_animationMultiplier = m_speed / m_walkSpeed;
                }
                else
                {
                    m_animationState = PegasusConstants.PegasusAnimationState.Idle;
                    m_animationMultiplier = 1f;
                }
            }
            else
            {
                m_speed = 0f;
                m_animationState = PegasusConstants.PegasusAnimationState.Idle;
                m_animationMultiplier = 1f;
            }

            if (m_animator != null && m_speed != m_lastSpeed)
            {
                m_lastSpeed = m_speed;
                m_animator.SetInteger(m_animationStateHash, (int)m_animationState);
                m_animator.SetFloat(m_animationMultiplierHash, m_animationMultiplier);
            }
        }

        /// <summary>
        /// Play an animation state
        /// </summary>
        /// <param name="newState">The new state</param>
        /// <param name="forceStateNow">Force an animation state update</param>
        private void PlayState(PegasusConstants.PegasusAnimationState newState, bool forceStateNow)
        {
            if (forceStateNow)
            {
               // ProtoStaticData.SceneTable.ElementList[i].Name
                //Set variables
                m_animationState = newState;
                m_animator.SetInteger(m_animationStateHash, (int)m_animationState);

                //Force animation
                switch (m_animationState)
                {
                    case PegasusConstants.PegasusAnimationState.Idle:
                        m_animator.Play("Base Layer.Idle");
                        break;
                    case PegasusConstants.PegasusAnimationState.Walking:
                        m_animator.Play("Base Layer.Walk");
                        break;
                    case PegasusConstants.PegasusAnimationState.Running:
                        m_animator.Play("Base Layer.Run");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else if (m_animationState != newState)
            {
                //Set variables
                m_animationState = newState;
                m_animator.SetInteger(m_animationStateHash, (int)m_animationState);
            }

            //And update the animation speed multiplier
            m_animator.SetFloat(m_animationMultiplierHash, m_animationMultiplier);
        }

        /// <summary>
        /// Replace a clip with another one
        /// </summary>
        /// <param name="animator">Animator its being replaced on</param>
        /// <param name="clipName">Clip name</param>
        /// <param name="overrideClip">New clip</param>
        private void ReplaceClip(Animator animator, string clipName, AnimationClip overrideClip)
        {
            AnimatorOverrideController overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
            if (overrideController == null)
            {
                overrideController = new AnimatorOverrideController();
                overrideController.name = "PegasusRuntimeController";
                overrideController.runtimeAnimatorController = animator.runtimeAnimatorController;
            }
            overrideController[clipName] = overrideClip;
            if (ReferenceEquals(animator.runtimeAnimatorController, overrideController) == false)
            {
                animator.runtimeAnimatorController = overrideController;
            }
        }

        //                    #if UNITY_EDITOR
        //                    var settings = AnimationUtility.GetAnimationClipSettings(m_idleAnimation);
        //                    settings.loopTime = m_loopIdleAnimation;
        //                    settings.loopBlend = m_loopIdleAnimation;
        //                    AnimationUtility.SetAnimationClipSettings(m_idleAnimation, settings);
        //                    #endif
    }
}