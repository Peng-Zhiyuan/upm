using UnityEngine;

namespace Pegasus
{
    /// <summary>
    /// Keybaord short cuts / defaults. These will be assigned to new Pegasus as they are created.
    /// </summary>
    public class PegasusDefaults : ScriptableObject
    {
        [Header("POI Selection Ctrl")]
        public KeyCode m_keyPrevPoi = KeyCode.Home;
        public KeyCode m_keyNextPoi = KeyCode.End;

        [Header("Positioning Ctrl-POI, ShiftCtrl-LookAt")]
        public KeyCode m_keyUp = KeyCode.PageUp;
        public KeyCode m_keyDown = KeyCode.PageDown;
        public KeyCode m_keyLeft = KeyCode.LeftArrow;
        public KeyCode m_keyRight = KeyCode.RightArrow;
        public KeyCode m_keyForward = KeyCode.UpArrow;
        public KeyCode m_keyBackward = KeyCode.DownArrow;

        [Header("Gizmo Sizes")]
        [Range(1f, 6f)]
        public float m_poiGizmoSize = 5f;
        [Range(1f, 6f)]
        public float m_lookatGizmoSize = 2f;

        [Header("AutoBank Settings")]
        public bool m_autoBankOnByDefault = false;
        [Range(0f, 90f)]
        public float m_maxAutoBankAngle = 45f;
        [Range(0f, 90f)]
        public float m_maxAutoBankSpeed = PegasusConstants.SpeedFast;

        [Header("Speed Settings")]
        public PegasusConstants.SpeedType m_flyThroughSpeed = PegasusConstants.SpeedType.Medium;
    }
}


