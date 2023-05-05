//
// Fingers Gestures
// (c) 2015 Digital Ruby, LLC
// http://www.digitalruby.com
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

// #define LOG_JOYSTICK

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRubyShared
{
    /// <summary>
    /// Joystick demo script
    /// </summary>
    public class Joystick : MonoBehaviour
    {
        /// <summary>
        /// Fingers joystick script
        /// </summary>
        [Tooltip("Fingers Joystick Script")]
        public FingersJoystickScript JoystickScript;

        /// <summary>
        /// Fingers joystick script 2
        /// </summary>
        [Tooltip("Fingers Joystick Script 2")]
        public FingersJoystickScript JoystickScript2;

        /// <summary>
        /// Object to move with the joystick
        /// </summary>
        [Tooltip("Object to move with the joystick")]
        public GameObject Mover;

        /// <summary>
        /// Object to move with the joystick 2
        /// </summary>
        [Tooltip("Object to move with the joystick 2")]
        public GameObject Mover2;

        /// <summary>
        /// First mask for joystick #1
        /// </summary>
        [Tooltip("First mask for joystick #1")]
        public Collider2D Mask1;

        /// <summary>
        /// Second mask for joystick #2
        /// </summary>
        [Tooltip("Second mask for joystick #2")]
        public Collider2D Mask2;

        /// <summary>
        /// Units per second to move the Mover object with the joystick
        /// </summary>
        [Tooltip("Units per second to move the Mover object with the joystick")]
        public float Speed = 250.0f;

        private TapGestureRecognizer tapGesture;
        private TapGestureRecognizer tapGesture2;

        public Transform Direction;

        private void TapGestureFired(GestureRecognizer tap)
        {
            if (tap.State == GestureRecognizerState.Ended)
            {
                Debug.LogFormat("Tap gesture executed at {0},{1}", tap.FocusX, tap.FocusY);
                GameObject mover = (tap == tapGesture ? Mover : Mover2);
                if (mover != null)
                {
                    mover.transform.Rotate(Vector3.forward, 10.0f);
                }
            }
        }

        private void Awake()
        {
            JoystickScript.JoystickExecuted = JoystickExecuted;
            //JoystickScript.JoystickStoped = JoystickStoped;
            //JoystickScript.JoystickStarted = JoystickStarted;
            if (JoystickScript2 != null)
            {
                JoystickScript2.JoystickExecuted = JoystickExecuted;
            }
        }

        private void OnEnable()
        {
            tapGesture = new TapGestureRecognizer();
            tapGesture.ClearTrackedTouchesOnEndOrFail = true;
            tapGesture.StateUpdated += TapGestureFired;
            tapGesture.AllowSimultaneousExecutionWithAllGestures();
            FingersScript.Instance.AddGesture(tapGesture);

            // add first mask if it exists
            if (Mask1 != null && JoystickScript != null)
            {
                FingersScript.Instance.AddMask(Mask1, JoystickScript.PanGesture);
                FingersScript.Instance.AddMask(Mask1, tapGesture);
            }

            // add second tap gesture and add second mask if it is not null
            if (Mover2 != null && Mask2 != null && JoystickScript2 != null)
            {
                tapGesture2 = new TapGestureRecognizer();
                tapGesture2.ClearTrackedTouchesOnEndOrFail = true;
                tapGesture2.StateUpdated += TapGestureFired;
                tapGesture2.AllowSimultaneousExecutionWithAllGestures();
                FingersScript.Instance.AddGesture(tapGesture2);
                FingersScript.Instance.AddMask(Mask2, JoystickScript2.PanGesture);
                FingersScript.Instance.AddMask(Mask2, tapGesture2);
            }
        }
        
        private void OnDisable()
        {
            if (FingersScript.HasInstance)
            {
                FingersScript.Instance.RemoveGesture(tapGesture);

                // remove first mask if it exists
                if (Mask1 != null && JoystickScript != null)
                {
                    FingersScript.Instance.RemoveMask(Mask1, JoystickScript.PanGesture);
                }

                if (tapGesture2 != null)
                {
                    FingersScript.Instance.RemoveGesture(tapGesture2);
                }

                // remove second mask if it exists
                if (Mover2 != null && Mask2 != null && JoystickScript2 != null)
                {
                    FingersScript.Instance.RemoveMask(Mask2, JoystickScript2.PanGesture);
                }
            }
        }

        private bool IsKeyInput = false;
        private void Update()
        {

#if LOG_JOYSTICK

            foreach (Touch t in Input.touches)
            {
                Debug.LogFormat("Touch: {0},{1} {2}", t.position.x, t.position.y, t.phase);
            }

#endif

/*#if UNITY_STANDALONE_WIN
            Vector2 amount = Vector2.zero;
            amount.x = Input.GetAxis("Horizontal");
            amount.y = Input.GetAxis("Vertical");

            if (amount == Vector2.zero)
            {
                if (IsKeyInput == true)
                {
                    IsKeyInput = false;
                   // JoystickStoped(null);
                }
                return;
            }

            if (IsKeyInput == false)
            {
              //  JoystickStarted(null);
            }

            IsKeyInput = true;
            
            amount.Normalize();
            
            var data = ClientEngine.Instance.GetInputObject();
            data.cmdType = CmdType.MOVE_DIR;
            //Direction.forward = new Vector3(amount.x,amount.y,0);
            Direction.LookAt(new Vector3(amount.x,amount.y,0)*10f + Direction.transform.position);
            var dir = amount.normalized;
            data.dir = new Vector3(dir.x, 0, dir.y);
            
            data.dir = Quaternion.Euler(0, -45, 0) * data.dir;
            ClientEngine.Instance.AddInputData(data);
#endif*/

        }


        private void JoystickExecuted(FingersJoystickScript script, Vector2 amount)
        {

#if LOG_JOYSTICK

            Debug.LogFormat("Joystick: {0:0.000000},{1:0.000000}", amount.x, amount.y);

#endif
            //Debug.LogError(tapGesture.State);

            GameObject mover = (script == JoystickScript ? Mover : Mover2);
            if (mover != null)
            {
                Vector3 pos = mover.transform.position;
                pos.x += (amount.x * Speed * Time.deltaTime);
                pos.y += (amount.y * Speed * Time.deltaTime);
                mover.transform.position = pos;
            }
        }
    }
}
