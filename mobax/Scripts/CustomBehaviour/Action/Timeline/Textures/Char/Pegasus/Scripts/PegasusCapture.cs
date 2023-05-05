using UnityEngine;
using UnityEngine.UI;

namespace Pegasus
{
    /// <summary>
    /// A system to convert a runtime flythrough into a Pegasus flythrough
    /// </summary>
    public class PegasusCapture : MonoBehaviour
    {
        public KeyCode m_keyCodeCapture = KeyCode.P;
        public Camera m_mainCamera;
        public PegasusPath m_path;
        public bool m_enableOnStart = true;
        public bool m_clearOnStart = true;
        public bool m_showReticule = true;
        public GameObject m_reticuleGO;

        void Start()
        {
            //Grab main camera if necessary
            if (m_mainCamera == null)
            {
                m_mainCamera = Camera.main;
            }

            //Create new path if necessary
            if (m_path == null)
            {
                m_path = PegasusPath.CreatePegasusPath();
            }

            //Show reticule
            if (m_reticuleGO == null)
            {
                m_reticuleGO = GameObject.Find("Pegasus Capture Reticule");
            }
            if (m_reticuleGO != null)
            {
                m_reticuleGO.SetActive(m_showReticule && m_enableOnStart);
                UpdateReticuleText();
            }

            //Show previous path
            if (m_enableOnStart)
            {
                //Clear path
                if (m_clearOnStart)
                {
                    m_path.ClearPath();
                }

                //Display path
                foreach (var path in m_path.m_path)
                {
                    GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    marker.transform.position = path.m_location;
                    marker.transform.localScale = Vector3.one * 0.25f;
                }
            }
        }

	    // Update called once per frame
	    void Update()
        {
            if (Application.isPlaying)
            {
                //If not enabled then exit
                if (!m_enableOnStart)
                {
                    return;
                }

                //Exit if we are doing nothing
                if (m_path == null || m_mainCamera == null)
                {
                    return;
                }

                //Handle clicks
                if (Input.GetKeyDown(m_keyCodeCapture))
                {
                    Debug.Log("Pegasus POI Location Captured!");
                    ProcessCaptureEvent();
                }
            }
        }

        /// <summary>
        /// Save the event
        /// </summary>
        private void ProcessCaptureEvent()
        {
            m_path.AddPoint(m_mainCamera.gameObject);
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.transform.position = m_mainCamera.gameObject.transform.position;
            marker.transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Update reticule visibility
        /// </summary>
        public void UpdateReticuleVisibility()
        {
            if (m_reticuleGO == null)
            {
                m_reticuleGO = GameObject.Find("Pegasus Capture Reticule");
            }
            if (m_reticuleGO != null)
            {
                m_reticuleGO.SetActive(m_showReticule && m_enableOnStart);
            }
        }

        public void UpdateReticuleText()
        {
            if (m_reticuleGO == null)
            {
                m_reticuleGO = GameObject.Find("Pegasus Capture Reticule");
            }
            if (m_reticuleGO != null)
            {
                Text [] texts = m_reticuleGO.GetComponentsInChildren<Text>();

                foreach (var text in texts)
                {
                    text.text = string.Format(
                        "Play your game and then press {0} to create a POI at the current location.", m_keyCodeCapture.ToString());
                }

                m_reticuleGO.SetActive(m_showReticule && m_enableOnStart);
            }
        }
    }
}