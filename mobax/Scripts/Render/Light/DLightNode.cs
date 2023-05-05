using System;
using UnityEngine;
using System.Text;
using Sirenix.Serialization;
using Sirenix.OdinInspector;
namespace ScRender
{
    [Serializable]
    public class DLightNodeData
    {

        public float shadowYZoffset = 1f;
        public Color mainColor;
        public float mainintensity;

        
        public Vector3 A = new Vector3(0, 2, 0);
        public Vector3 B = Vector3.zero;
        public Vector3 C = new Vector3(0, 0, 1f);

        public float shadowFallOff = 0.5f;
        public Color shadowColor = new Color(0f, 0f, 0f, 0.5f);
        
        //[HideInInspector]
        public Vector3 finalDir = new Vector3(0f, 1f, 0f);
    }


    /// <summary>
    /// 灯光节点,用来构建动态灯光图
    /// 主要属性:
    /// 
    /// </summary>
    public class DLightNode : SerializedMonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
           DLightMgr.GetSingle().RegNode(this);
        }

        // Update is called once per frame
        void Update()
        {
            data.C.z = shadowHeight*data.shadowYZoffset;
            Vector3 dir = (data.C - data.A).normalized;
            var mat4= Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            //data.finalDir = mat4.MultiplyVector(dir);
        }

        private void OnDrawGizmosSelected()
        {
           
        }

        private void OnDrawGizmos()
        {
            RenderGizmos();
        }

        void RenderGizmos()
        {
            Vector3 dir = (data.C - data.A).normalized;
            Gizmos.color=Color.red;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            //data.finalDir = Gizmos.matrix.MultiplyVector(dir);
            Gizmos.DrawLine(data.A,data.B);
            Gizmos.DrawLine(data.A,data.C);
            Gizmos.DrawLine(data.B,data.C);
            Gizmos.matrix=Matrix4x4.identity;
            Gizmos.color=Color.white;
        }

        public void ResetStatic()
        {
            DLightMgr.GetSingle().RefreshForce();
        }
        
        // private void OldOnDrawGizmos()
        // {
        //     // 
        //     var norAC = Vector3.Normalize(shadowDir);
        //     Vector3 A = Vector3.zero;
        //     Vector3 B = Vector3.zero;
        //     B.y = 0;
        //     A.y = height;
        //     Vector3 norAB = (B-A).normalized;
        //     var cos = Vector3.Dot(norAC, norAB);
        //     var lenAB = A.y;
        //     var lenAC =lenAB/cos;
        //     var C = A + norAC * lenAC;
        //     
        //     Gizmos.color=Color.red;
        //     Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        //     finalDir = Gizmos.matrix.MultiplyVector(shadowDir);
        //     Gizmos.DrawLine(A,B);
        //     Gizmos.DrawLine(A,C);
        //     Gizmos.DrawLine(B,C);
        //     Gizmos.matrix=Matrix4x4.identity;
        //     Gizmos.color=Color.white;
        //
        // }

        // public void OnDrawGizmosSelected()
        // {
        //     //Gizmos.DrawCube(transform.position,Vector3.one);
        // }


        public void SaveData()
        {
            var bytes = SerializationUtility.SerializeValue(data, DataFormat.JSON);
            var id = gameObject.GetInstanceID().ToString();
            var val = Encoding.ASCII.GetString(bytes);
            //Debug.LogFormat("{0}: {1}", id, val);
            PlayerPrefs.SetString(id, val);
        }

        public void LoadData()
        {
            
            Debug.Log("============CurData");
            Debug.Log(data);
            var id = gameObject.GetInstanceID().ToString();
            var str = PlayerPrefs.GetString(id);
            //Debug.LogFormat("{0}: {1}", id, str);
            var val = Encoding.ASCII.GetBytes (str);
            if (val != null)
            {
                data = SerializationUtility.DeserializeValue<DLightNodeData>(val, DataFormat.JSON);
                Debug.Log("============NowData");
                Debug.Log(data);
            }
        
            //ResetStatic();
        }
        const float shadowHeight = 2f;
        public DLightNodeData data;
    }
}

