using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pegasus
{
    /// <summary>
    /// Captures a pegasus path
    /// </summary>
    [System.Serializable]
    public class PegasusPath : ScriptableObject
    {
        /// <summary>
        /// The points that are stored in a pegasus path
        /// </summary>
        [System.Serializable]
        public class PegasusPoint
        {
            public Vector3 m_location;
            public Vector3 m_rotationEuler;
            public Vector3 m_dofDistance;

            public PegasusPoint(Vector3 location, Vector3 rotationEuler)
            {
                m_location = location;
                m_rotationEuler = rotationEuler;
            }
        }

        //The default speed to create at
        public float m_defaultSpeed = 8f;

        //The pegasus points stored in this path
        public List<PegasusPoint> m_path = new List<PegasusPoint>();

        /// <summary>
        /// Create a new pegasus path
        /// </summary>
        /// <returns>New pegasus path</returns>
        public static PegasusPath CreatePegasusPath()
        {
            PegasusPath pp = ScriptableObject.CreateInstance<PegasusPath>();
            #if UNITY_EDITOR
            string path = "Assets/PegasusPath.asset";
            AssetDatabase.CreateAsset(pp, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            #endif
            return pp;
        }

        public void AddPoint(GameObject go)
        {
            if (go == null)
            {
                return;
            }

            Vector3 location = go.transform.position;
            Vector3 rotation = go.transform.eulerAngles;
            PegasusPoint pp = null;
            if (m_path.Count > 0)
            {
                pp = m_path[m_path.Count - 1];
                if (pp.m_location == location && pp.m_rotationEuler != rotation)
                {
                    return;
                }
            }
            m_path.Add(new PegasusPoint(location, rotation));
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }

        public void ClearPath()
        {
            m_path.Clear();
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }

        public void CreatePegasusFromPath()
        {
            //Exit if nothing to do
            if (m_path.Count == 0)
            {
                return;
            }

            GameObject pegasusGo = new GameObject("Pegasus Manager");
            PegasusManager pegasus = pegasusGo.AddComponent<PegasusManager>();
            pegasus.SetDefaults();
            pegasus.m_heightCheckType = PegasusConstants.HeightCheckType.None;
            pegasus.m_minHeightAboveTerrain = 0.1f;
            pegasus.m_flythroughType = PegasusConstants.FlythroughType.SingleShot;

            PegasusPoint p = null;
            for (int i = 0; i < m_path.Count; i++)
            {
                p = m_path[i];
                pegasus.AddPOI(p.m_location, p.m_location + Quaternion.Euler(p.m_rotationEuler) * (Vector3.forward * 2f));
            }

            pegasus.SetSpeed(m_defaultSpeed);

            #if UNITY_EDITOR
            Selection.activeObject = pegasusGo;
            #endif  
        }
    }
}