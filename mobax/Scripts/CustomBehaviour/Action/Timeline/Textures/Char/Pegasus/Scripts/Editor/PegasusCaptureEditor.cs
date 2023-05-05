using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Pegasus
{
    /// <summary>
    /// Editor for flythrough manager
    /// </summary>
    [CustomEditor(typeof(PegasusCapture))]

    public class PegasusCaptureEditor : Editor
    {
        private GUIStyle m_boxStyle;
        private GUIStyle m_wrapStyle;
        private PegasusCapture m_capture;
        private bool m_environment = false;

        private void OnEnable()
        {
            //Check for target
            if (target == null)
            {
                return;
            }

            //Get our manager
            m_capture = (PegasusCapture)target;

            //Set up the default camera if we can
            if (m_capture.m_mainCamera == null)
            {
                if (Camera.main != null)
                {
                    m_capture.m_mainCamera = Camera.main;
                    EditorUtility.SetDirty(m_capture);
                }
            }

            //Set up the capturer object
            if (!Application.isPlaying)
            {
                if (m_capture.m_path == null)
                {
                    m_capture.m_path = PegasusPath.CreatePegasusPath();
                }

                //And add Pegasus to the environment
                SetPegasusDefines();

                //Update the reticules text if we have it
                m_capture.UpdateReticuleText();

                //And visibility
                m_capture.UpdateReticuleVisibility();
            }
        }

        public override void OnInspectorGUI()
        {
            //Get our manager
            m_capture = (PegasusCapture)target;

            //Set up the box style
            if (m_boxStyle == null)
            {
                m_boxStyle = new GUIStyle(GUI.skin.box);
                m_boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
                m_boxStyle.fontStyle = FontStyle.Bold;
                m_boxStyle.alignment = TextAnchor.UpperLeft;
            }

            //Setup the wrap style
            if (m_wrapStyle == null)
            {
                m_wrapStyle = new GUIStyle(GUI.skin.label);
                m_wrapStyle.fontStyle = FontStyle.Normal;
                m_wrapStyle.wordWrap = true;
            }

            //Text intro
            GUILayout.BeginVertical(string.Format("Pegasus ({0}.{1})", PegasusConstants.MajorVersion, PegasusConstants.MinorVersion), m_boxStyle);
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Welcome to Pegasus Capture!\nPress Play and then your selected hot key to capture POI for your flythrough. Then press Create Pegasus to create a Pegasus fly through after you have finshed.\nNOTE: You can adjust the rotation & position damping on the Pegasus you create to make your play back more accurate.", m_wrapStyle);
            GUILayout.EndVertical();

            EditorGUI.BeginChangeCheck();

            GUILayout.Space(5);

            KeyCode keyCodeCapture = (KeyCode)EditorGUILayout.EnumPopup(GetLabel("Capture Key"), m_capture.m_keyCodeCapture);
            Camera mainCamera = (Camera)EditorGUILayout.ObjectField(GetLabel("Target Camera"), m_capture.m_mainCamera, typeof(Camera), true);
            PegasusPath path = (PegasusPath) EditorGUILayout.ObjectField(GetLabel("Pegasus Path"), m_capture.m_path, typeof(PegasusPath), false);
            bool enableOnStart = EditorGUILayout.Toggle(GetLabel("Enable On Start"), m_capture.m_enableOnStart);
            bool clearOnStart = EditorGUILayout.Toggle(GetLabel("Clear On Start"), m_capture.m_clearOnStart);
            bool showReticule = EditorGUILayout.Toggle(GetLabel("Show Reticule"), m_capture.m_showReticule);

            if (path == null || path.m_path.Count == 0)
            {
                GUI.enabled = false;
            }

            if (DisplayButton(GetLabel("Create Pegasus")))
            {
                path.CreatePegasusFromPath();
                enableOnStart = false;
            }

            GUI.enabled = true;

            GUILayout.Space(5f);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_capture, "Made changes");

                m_capture.m_mainCamera = mainCamera;
                m_capture.m_path = path;
                m_capture.m_clearOnStart = clearOnStart;
                ;

                if (m_capture.m_keyCodeCapture != keyCodeCapture)
                {
                    m_capture.m_keyCodeCapture = keyCodeCapture;
                    m_capture.UpdateReticuleText();
                }

                if (m_capture.m_showReticule != showReticule || m_capture.m_enableOnStart != enableOnStart)
                {
                    m_capture.m_enableOnStart = enableOnStart;
                    m_capture.m_showReticule = showReticule;
                    m_capture.UpdateReticuleVisibility();
                }

                EditorUtility.SetDirty(m_capture);
            }
        }

        /// <summary>
        /// Set up the pegasus defines
        /// </summary>
        public void SetPegasusDefines()
        {
            if (m_environment == true)
            {
                return;
            }

            m_environment = true;

            string currBuildSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            //Check for and inject 
            if (!currBuildSettings.Contains("PEGASUS_PRESENT"))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings + ";PEGASUS_PRESENT");
            }
        }

        /// <summary>
        /// Display a button that takes editor indentation into account
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        bool DisplayButton(GUIContent content)
        {
            TextAnchor oldalignment = GUI.skin.button.alignment;
            GUI.skin.button.alignment = TextAnchor.MiddleLeft;
            Rect btnR = EditorGUILayout.BeginHorizontal();
            btnR.xMin += (EditorGUI.indentLevel * 18f);
            btnR.height += 20f;
            btnR.width -= 4f;
            bool result = GUI.Button(btnR, content);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(22);
            GUI.skin.button.alignment = oldalignment;
            return result;
        }

        /// <summary>
        /// Get a content label - look the tooltip up if possible
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        GUIContent GetLabel(string name)
        {
            string tooltip = "";
            if (m_tooltips.TryGetValue(name, out tooltip))
            {
                return new GUIContent(name, tooltip);
            }
            else
            {

                return new GUIContent(name);
            }
        }

        /// <summary>
        /// The tooltips
        /// </summary>
        static Dictionary<string, string> m_tooltips = new Dictionary<string, string>
        {
            { "Capture Key", "Hit this key at runtime to capture the camera location and orientation, and add it to your Pegasus Path." },
            { "Target Camera", "This is the camera that will be used to get location and orientation information from for your Pegasus Path" },
            { "Pegasus Path", "This is the path object that path information can be stored in. You can delete it after you have created your Pegasus." },
            { "Enable On Start", "This will enable the capturer on start. It must be renabled after the creation of every Pegasus." },
            { "Clear On Start", "This will clear any previous locations out of your path when you start your scene." },
            { "Show Reticule", "This will show or hide your targeting reticule." },
        };

    }
}