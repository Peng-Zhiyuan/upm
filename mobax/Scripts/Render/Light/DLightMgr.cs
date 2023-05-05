using System;
using System.Collections.Generic;
using UnityEngine;
using LegacySystem.Collections;
using System.Text;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
namespace ScRender
{
    public class DLightIns
    {
        public DLight light = null;
        public DLIGHT_PRIORITY priority = DLIGHT_PRIORITY.NORMAL;
        public int tick = 0;

        public static int sort(DLightIns _l, DLightIns _r)
        {
            return _l.tick - _r.tick;
        }
    }
    
    
    public class DLightMgr : MonoBehaviour
    {
        private static DLightMgr _single=null;
        public static DLightMgr GetSingle()
        {
            if (_single == null)
            {
                _single = GameObject.FindObjectOfType<DLightMgr>();
                if (_single == null)
                {
                    Debug.LogError("Can not found LightMgr");
                }
            }
            return _single;
        }

        public void Clear()
        {
            for (int i = 0; i < insList.Count; i++)
            {
                insList[i].light.Release();
            }
            insList.Clear();
            nodeList.Clear();
            lastNodeId = -1;
        }


        public void LightIn(DLight _light,DLIGHT_PRIORITY _priority)
        {
            for (var i = 0; i < insList.Count; ++i)
            {
                var ins = insList[i];
                if (ins.light == _light)
                {
                    return;
                }
            }

            if (insList.Count >= maxNum)
            {
                insList.Sort(DLightIns.sort);
                ReleaseIns(insList[0]);
                insList.Remove(insList[0]);
            }
            DLightIns dins = new DLightIns()
            {
                light = _light,
                priority = _priority,
            };
            InitIns(dins);
            insList.Add(dins);
            lightActive = insList.Count;
        }

        public void LightOut(DLight _light)
        {
            for (var i = 0; i < insList.Count; ++i)
            {
                var ins = insList[i];
                if (ins.light == _light)
                {
                    ReleaseIns(ins);
                    insList.Remove(ins);
                    lightActive = insList.Count;
                    return;
                }
            }
      
        }
        
        
        void InitIns(DLightIns _ins)
        {
            _ins.light.Init();
            _ins.tick = Time.frameCount+_ins.priority==DLIGHT_PRIORITY.IMPORTANT?10000000:0;
        }

        void ReleaseIns(DLightIns _ins)
        {
            _ins.light.Release();
        }


        void OnGUI()
        {
            GUI.Label(new Rect(0f,0f,Screen.width,30f),debugStr);
        }

        private void Update()
        {
            //build shadow light
            UpdateMainLight();
        }
        
        
        
        
        [Button("Save")]
        void SaveData ()
        {
            for (int i = 0; i <nodeList.Count; i++)
            {
                nodeList[i].SaveData();
            }

            for (int i = 0; i < insList.Count; ++i)
            {
                insList[i].light.SaveData();
            }
            Debug.Log("灯光保存成功");
        }

        [Button("Load")]
        void LoadData ()
        {
#if UNITY_EDITOR
            if (nodeList.Count == 0)
            {
                var coll=GameObject.FindObjectsOfType<DLightNode>();
                nodeList.AddRange(coll);
            }
            if (insList.Count == 0)
            {
                var coll=GameObject.FindObjectsOfType<DLight>();
                for (int i = 0; i < coll.Length; ++i)
                {
                    coll[i].LoadData();
                }
            }
#endif
            for (int i = 0; i <nodeList.Count; i++)
            {
                nodeList[i].LoadData();
            }
            Debug.Log("灯光读取成功");
        }

        public void RegNode(DLightNode _node)
        {
            nodeList.Add(_node);
        }

        private void OnDrawGizmos()
        {
            var vPos = GetViewPos();
            Gizmos.DrawSphere(vPos,0.1f);
        }

        Vector3 GetViewPos()
        {
            //TempGetCastPoint
            if (!Application.isPlaying)
            {
                return Vector3.zero;
            }


            //if (GameStateManager.Instance.GetCurrentStateID() == (int) eGameState.PlayingState)
          /*  if (BattleManager.Instance.GetCurrentState() == eBattleState.Play)
            {
                if (SceneObjectManager.Instance.LocalPlayerCamera != null)
                {
                    return SceneObjectManager.Instance.LocalPlayerCamera.GetPosition();
                }
            }*/

            if (Camera.main == null)
            {
                return Vector3.zero;
            }

            
            var ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));

            Vector3 A = ray.origin;
            Vector3 B = A;
            B.y = 0;
            Vector3 AB = (B - A).normalized;
            Vector3 AC = ray.direction;
            float cos = Vector3.Dot(AB, AC);
            float lenAC = A.y/cos;
            //Debug.Log("lenAc"+);
            Vector3 finalPos= A + ray.direction * lenAC;
            return finalPos;
        }

        private string debugStr;

        void UpdateMainLight()
        {
            if (nodeList.Count == 0)
            {
                return;
            }


            //Vector3 cPos = GetViewPos();
            lightSortPos=GetViewPos();
            nodeList.Sort(_sortLight);
            
            
            var ditry = false;
            if ((lightRecord[0] != nodeList[0])&&(lightRecord[0] != nodeList[1]))
            {
                ditry = true;
            }
            if ((lightRecord[1] != nodeList[0])&&(lightRecord[1] != nodeList[1]))
            {
                ditry = true;
            }


            if (ditry)
            {
                lightRecord[0] = nodeList[0];
                lightRecord[1] = nodeList[1];
              
            }
            var offset = GetOffset(lightRecord[0].transform.position,lightRecord[1].transform.position,
                lightSortPos,true);
            
            

            var color = Color.Lerp(lightRecord[0].data.mainColor, lightRecord[1].data.mainColor, offset);
            var intensity = Mathf.Lerp(lightRecord[0].data.mainintensity, lightRecord[1].data.mainintensity, offset);
            Vector3 dir = lightRecord[0].data.finalDir;// Vector3.Lerp(lightRecord[0].data.finalDir, lightRecord[1].data.finalDir, offset);
   
            ModifyMainLight(color,intensity, dir);
            //Debug.LogError("lightRecord[0].data.finalDir:" + lightRecord[0].data.finalDir.ToDetailString());
        }

        float GetOffset(Vector3 A,Vector3 B,Vector3 C,bool showGizmos=false)
        {
            A.y =B.y=C.y= 0f;
            var AC = C - A;
            var AB = B - A;
            Vector3 norAB = AB.normalized;
            Vector3 D=Vector3.Project(AC, norAB)+A;
            Vector3 AD = D - A;
            Vector3 norAD = AD.normalized;
            float off = 0f;
            //计算方向
            float cosDAB = Vector3.Dot(norAD, norAB);
            if (cosDAB > 0)
            {
                //正向才计算
                off = AD.magnitude / AB.magnitude;
            }
            var offset = Mathf.Clamp(off,0f,1f);
            if (showGizmos)
            {
                Debug.DrawLine(A,B);
                Debug.DrawLine(C,D);
            }
            // debugStr = "==========" + lightRecord[0].gameObject.name + " " + lightRecord[1].gameObject.name + " off:" +
            //            off + " offset:" +  offset;
            return offset;
        }

        public ShadowArgInfo Fetch(RoleRender role)
        {
            
            
            var trans = role.transform;
            if (nodeList.Count == 0)
            {
                return new ShadowArgInfo()
                {
                    dir = defaultShadowDir,
                    color  =Color.black,
                    fallOffset  =1.0f,
                };
            }

            if (nodeList.Count == 1)
            {
                return new ShadowArgInfo()
                {
                    dir = nodeList[0].data.finalDir,
                    color  =nodeList[0].data.shadowColor,
                    fallOffset  =nodeList[0].data.shadowFallOff,
                };
            }
            shadowSortPos = trans.position;
            shadowSortPos.y = 0;
            nodeList.Sort(_sort);

            if (!shadowRecord.ContainsKey(role))
            {
                shadowRecord.Add(role, new DLightNode[2]);
            }

            var ditry = false;
            if ((shadowRecord[role][0] != nodeList[0])&&(shadowRecord[role][0] != nodeList[1]))
            {
                ditry = true;
            }
            if ((shadowRecord[role][1] != nodeList[0])&&(shadowRecord[role][1] != nodeList[1]))
            {
                ditry = true;
            }


            if (ditry)
            {
                shadowRecord[role][0] = nodeList[0];
                shadowRecord[role][1] = nodeList[1];
            }
            
            
            var offset = GetOffset(shadowRecord[role][0].transform.position,shadowRecord[role][1].transform.position,
                shadowSortPos,false);

           //  Vector3 pos0 = shadowRecord[role][0].transform.position;
           //  pos0.y = 0;
           //  Vector3 pos1 = shadowRecord[role][1].transform.position;
           //  pos1.y = 0;
           //
           //  var p0 = shadowSortPos - pos0;
           //  var p1 = pos1 - pos0;
           //  Vector3 pos1Normal = p1.normalized;
           //  pos0=Vector3.Project(p0, pos1Normal);
           //  var oo = pos0.sqrMagnitude / p1.sqrMagnitude;
           //  var offset = Mathf.Clamp(pos0.sqrMagnitude / p1.sqrMagnitude,0f,1f);
           //
           //  // var d0 = Vector3.Distance(pos0, sortPos);
           //  // var d1 = Vector3.Distance(pos1, sortPos);
           //  //var offset = d0 / (d0 + d1);
           // // Debug.Log(offset+" "+pos0);
           //
           // //Debug.DrawLine(shadowRecord[role][0].transform.position,shadowRecord[role][1].transform.position);
           // //Debug.Log(" oo:"+oo+" offset:"+offset);



           var result = new ShadowArgInfo();
           result.dir=Vector3.Lerp( Vector3.Normalize(shadowRecord[role][0].data.finalDir), Vector3.Normalize(shadowRecord[role][1].data.finalDir), offset);
           result.color= Color.Lerp(shadowRecord[role][0].data.shadowColor, shadowRecord[role][1].data.shadowColor, offset);
           result.fallOffset = Mathf.Lerp(shadowRecord[role][0].data.shadowFallOff, shadowRecord[role][1].data.shadowFallOff, offset);
           return result;
        }

        public struct ShadowArgInfo
        {
            public Vector3 dir;
            public Color color;
            public float fallOffset;
        }
        
        
        
        

        static int _sort(DLightNode _l, DLightNode _r)
        {
            Vector3 lpos = _l.transform.position;
            lpos.y = 0;
            Vector3 rpos = _r.transform.position;
            rpos.y = 0;
            
            var l=Vector3.Distance(lpos,shadowSortPos);
            var r=Vector3.Distance(rpos, shadowSortPos);
            return l - r>0?1:-1;
        }
        
        static int _sortLight(DLightNode _l, DLightNode _r)
        {
            Vector3 lpos = _l.transform.position;
            lpos.y = 0;
            Vector3 rpos = _r.transform.position;
            rpos.y = 0;
            
            var l=Vector3.Distance(lpos,lightSortPos);
            var r=Vector3.Distance(rpos, lightSortPos);
            return l - r>0?1:-1;
        }

        public void RefreshForce()
        {
            lastNodeId = -1;
            UpdateMainLight();
        }
        void ModifyMainLight(Color _color,float _intensity, Vector3 dir)
        {
            if (_forceCloseMainLight)
            {
                if (mainLight != null)
                {
                    mainLight.enabled = false;
                }
                return;
            }
            //Debug.Log("setMainColor"+_intensity);
            if (mainLight == null)
            {
                mainLight = new Light()
                {
                    type = LightType.Directional,
                };
                mainLight.transform.position = new Vector3(0, 0, 10);
                mainLight.transform.rotation=Quaternion.Euler(new Vector3(20, -45, 0));
            }
           
           // mainLight.color = _color;
            mainLight.intensity = _intensity;
            mainLight.transform.rotation = Quaternion.Euler(dir);
        }


        public bool _forceCloseMainLight = false;
        public Light mainLight;
        private const int maxNum = 32;
        private List<DLightIns> insList = new List<DLightIns>();
        public int lightActive;
        private static Vector3 shadowSortPos;
        private static Vector3 lightSortPos;
        private List<DLightNode> nodeList = new List<DLightNode>();
        public Vector3 defaultShadowDir = new Vector3(0f, -1f, 0f);
        private Dictionary<RoleRender, DLightNode[]> shadowRecord = new Dictionary<RoleRender, DLightNode[]>();
        private DLightNode[] lightRecord = new DLightNode[2];

        private int lastNodeId = -1;
        private bool needForceRefresh = false;
        
    }
}