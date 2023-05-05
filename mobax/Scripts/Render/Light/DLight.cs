using System;
using System.Collections;
using UnityEngine;
using System.Text;
using Sirenix.Serialization;
using Sirenix.OdinInspector;
namespace ScRender
{
    
    [Serializable]
    public class DLightData
    {

        
        public bool isStatic=true;
        public bool spriteOnly = false;
        public Color staticColor=Color.white;
        public float staticIntensity=1f;
        public float staticRange = 1f;
        
        
        public bool isMain=false;
        public DLIGHT_PRIORITY priority = DLIGHT_PRIORITY.NORMAL;
   
        
        //flash
        public bool flash = false;
        public float flashTime = 1;
        public float flashKeep = 0.3f;
        public float flashOffset = 1f;
        

        //============mainColor
        public bool mainAni = false;
        public float mainTime = 1;
        public AnimationCurve mainCurve=AnimationCurve.Linear(0f,1f,0f,1f);
        public bool mainLoop = false;
        //intensity
        public float intensityMainBegin = 1f;
        public float intensityMainEnd = 1f;
        //Color
        public Color colorMainBegin = Color.white;
        public Color colorMainEnd = Color.white;
        //range
        public float rangeBegin = 1f;
        public float rangeEnd = 1f;

     
        //============ColorSub
        public bool subAni = false;
        public float subTime = 1;
        public AnimationCurve subCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public bool subLoop = false;
        //==intensity
        public float intensitySubBegin = 1f;
        public float intensitySubEnd = 1f;

        //==Color
        public Color colorSubBegin = Color.white;
        public Color colorSubEnd = Color.white;
    }
    
    
    
    
    /// <summary>
    /// 动态灯光
    /// </summary>
    public class DLight : SerializedMonoBehaviour
    {
        /// <summary>
        /// 每次spawn出来都调用
        /// </summary>
        public void Init()
        {
            if (light == null)
            {
                light = transform.GetComponentInChildren<Light>();
                Debug.LogWarning("light is null",gameObject);
                isRelease = true;
                return;
                
            }
            
            data.isMain = light.type==LightType.Directional;
            if (data.isMain)
            {
                data.priority = DLIGHT_PRIORITY.IMPORTANT;
            }
            light.gameObject.SetActive(true);
            if (data.isStatic)
            {
                ResetStatic();
            }
        }
        /// <summary>
        /// 每次释放的时候调用
        /// </summary>
        public void Release()
        {
            isRelease = true;
            light.gameObject.SetActive(false);
        }


        public void OnEnable()
        {
           // var s = $"spriteOnly{data.spriteOnly}";
           // Debug.Log(s);
            Reset();
        }

        public void Reset()
        {
            isRelease = false;
            if (data.mainAni)
            {
                mainTick = 0f;
            }
            
            if (data.subAni)
            {
                subTick = 0f;
            }
            DLightMgr.GetSingle().LightIn(this,data.priority);
        }

        public void ResetStatic()
        {
            if (data.isStatic)
            {
                data.mainAni = false;
                data.subAni = false;
                data.flash = false;
                light.color = data.staticColor;
                light.intensity = data.staticIntensity;
                light.range = data.staticRange;
            }
            if (data.spriteOnly)
            {
                light.cullingMask = CHAR_LAYER + (int)Math.Pow(2, 24);
            }
            else
            {
                light.cullingMask = ALL_LAYER + (int)Math.Pow(2, 24);
                
            }
        }

        public IEnumerator AutoReset(float _delay)
        {
            yield return new WaitForSeconds(_delay);
            Reset();
        }

        private void Update()
        {
   
            if (isRelease)
            {
                return;
            }

            if (data.flash)
            {
                flashTick += Time.deltaTime;
                if (flashTick >= data.flashTime+data.flashKeep)
                {
                    flashTick = 0;
                    StartCoroutine(Flash());
                }
            }

            if (data.mainAni)
            {
                mainTick += Time.deltaTime;
                if (mainTick >= data.mainTime)
                {
                    mainTick = data.mainLoop ? 0 : data.mainTime;
                }
                mainIntensity = Mathf.Lerp(data.intensityMainBegin, data.intensityMainEnd, (data.mainCurve.Evaluate(mainTick / data.mainTime)));
                mainColor = Color.Lerp(data.colorMainBegin, data.colorMainEnd, (data.mainCurve.Evaluate(mainTick / data.mainTime)));
                if (!data.isMain)
                {
                    light.range = Mathf.Lerp(data.rangeBegin, data.rangeEnd, (data.mainCurve.Evaluate(mainTick / data.mainTime)));
                }
            }
            if (data.subAni)
            {
                subTick += Time.deltaTime;
                if (subTick >= data.subTime)
                {
                    subTick = data.subLoop ? 0 : data.subTime;
                }
                subIntensity = Mathf.Lerp(data.intensitySubBegin, data.intensitySubEnd, (data.subCurve.Evaluate(subTick / data.subTime)));
                subColor = Color.Lerp(data.colorSubBegin, data.colorSubEnd, (data.subCurve.Evaluate(subTick / data.subTime)));
          
            }

            if (data.flash || data.mainAni || data.subAni)
            {
                BuildFinalColor();
            }
        }

        void BuildFinalColor()
        {
            if (light == null)
            {
                return;
            }
            light.color =  mainColor+subColor;
            light.intensity = (mainIntensity + subIntensity)*curFlashOffset;
        }
        
        
        

        IEnumerator Flash()
        {
            curFlashOffset = data.flashOffset;
            yield return new WaitForSeconds(data.flashKeep);
            curFlashOffset = 1f;
        }
        
        

        private void OnDisable()
        {
            if (isRelease)
            {
                return;
            }

            if (DLightMgr.GetSingle())
            {
                DLightMgr.GetSingle().LightOut(this);
            }
        }

        private void OnDrawGizmosSelected()
        {
           
            // if (data.isStatic)
            // {
            //     //Gizmos.DrawWireSphere(transform.position,data.staticRange);
            // }
            // else
            // {
            //
            // }
            if (light == null)
            {
                return;
            }
            Gizmos.color = light.color;
            Gizmos.DrawWireSphere(transform.position, light.range);
        }
        public void SaveData()
        {
            var bytes = SerializationUtility.SerializeValue(data, DataFormat.JSON);
            var id = gameObject.GetInstanceID().ToString();
            var val = Encoding.ASCII.GetString(bytes);
            Debug.LogFormat("{0}: {1}", id, val);
            PlayerPrefs.SetString(id, val);
        }

        public void LoadData()
        {
            var id = gameObject.GetInstanceID().ToString();
            var str = PlayerPrefs.GetString(id);
            Debug.LogFormat("{0}: {1}", id, str);
            var val = Encoding.ASCII.GetBytes (str);
            if (val != null)
            {
                data = SerializationUtility.DeserializeValue<DLightData>(val, DataFormat.JSON);
                if (data != null)
                {
                    ResetStatic();
                }
                
                
            }
        }
        
        //ALL LAYER EXCLUDE UNLIT
        private const int CHAR_LAYER = 8390656;
        private const int ALL_LAYER = 8388407;
      
        public DLightData data;

        
        
        public Light light;
        private bool isRelease = false;
        //flash
        public float flashTick = 0f;
        private float curFlashOffset = 1f;
        private float mainIntensity;
        private Color mainColor;
        //============ColorSub
        public float mainTick;
        public float subTick = 0f;
        //==intensity
        private float subIntensity;
        //==Color
        private Color subColor;
        
    }
}