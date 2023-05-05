using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Table;

public class EffectManager : BattleComponent<EffectManager>
{
    private int EFFECT_MAX_COUNT = 100;      //同一时间最多特效数量
    private int m_UniqueID = 0;
    private Dictionary<int, BaseEffect> m_Effects = new Dictionary<int, BaseEffect>();

    private Dictionary<int, float> m_DelayDels = new Dictionary<int, float>();
    private List<int> _mayberemoveKeyList = new List<int>();

    public void Update(float tmp_deltaTime)
    {
        foreach (var tmp_key in new List<int>(m_Effects.Keys))
        {
            BaseEffect tmp_effect = m_Effects[tmp_key];
            if (tmp_effect != null)
            {
                if (tmp_effect.CachedTransform != null)
                {
                    tmp_effect.Update(tmp_deltaTime);
                }
                if (tmp_effect.Completed)
                {
                    RemoveEffect(tmp_key);
                }
            }
        }
    }

    public override void FixedUpdate()
    {
        float fixedDelta = Time.fixedDeltaTime;
        _mayberemoveKeyList.Clear();
        _mayberemoveKeyList.AddRange(m_Effects.Keys);
        for (int key = 0; key < _mayberemoveKeyList.Count; ++key)
        {
            BaseEffect tmp_effect = m_Effects[_mayberemoveKeyList[key]];
            if (tmp_effect != null)
            {
                if (tmp_effect.CachedTransform != null)
                {
                    tmp_effect.Update(fixedDelta);
                }

                if (tmp_effect.Completed)
                {
                    RemoveEffect(_mayberemoveKeyList[key]);
                }
            }
        }
    }

    public void LateUpdate()
    {
        _mayberemoveKeyList.Clear();
        _mayberemoveKeyList.AddRange(m_DelayDels.Keys);
        for (int key = 0; key < _mayberemoveKeyList.Count; ++key)
        {
            if (m_Effects.ContainsKey(_mayberemoveKeyList[key]))
            {
                BaseEffect tmp_effect = m_Effects[_mayberemoveKeyList[key]];
                float tmp_time = m_DelayDels[_mayberemoveKeyList[key]];
                if (tmp_effect != null && Time.time > tmp_time)
                {
                    RemoveEffect(_mayberemoveKeyList[key]);
                    m_DelayDels.Remove(_mayberemoveKeyList[key]);
                    return;
                }
            }

        }
    }

    public void RemoveEffect(int param_ID)
    {
        BaseEffect tmp_effect = null;
        if (m_Effects.TryGetValue(param_ID, out tmp_effect))
        {
            if (tmp_effect != null)
            {
                tmp_effect.Unitialize();

            }
            m_Effects.Remove(param_ID);
            tmp_effect = null;
        }
    }
    public BaseEffect GetEffect(int param_ID)
    {
        BaseEffect tmp_effect = null;
        m_Effects.TryGetValue(param_ID, out tmp_effect);
        return tmp_effect;
    }

    private int GetUniqueEffectID()
    {
        return ++m_UniqueID;
    }
    
    public int CreateMapEffect(string param_ID, Vector3 param_Position, Vector3 param_Direction, float param_Time, bool IsFoucus = false, Action<int> call = null)
    {
        if (param_Time == 0)
        {
            return -1;
        }

        int tmp_id = GetUniqueEffectID();

        GameObject tmp_effectObject = new GameObject(param_ID);
        Transform tmp_effectTransform = tmp_effectObject.transform;
        param_Direction.y = 0;
        tmp_effectObject.layer = IsFoucus?LayerMask.NameToLayer("FOCUS"):LayerMask.NameToLayer("VFX");
        
        MapEffect tmp_mapEffect = new MapEffect(tmp_id, 0, tmp_effectTransform)
        {
            Position = param_Position,
            Direction = param_Direction,
            TotalTime = param_Time
        };
        tmp_mapEffect.Play();
        m_Effects.Add(tmp_mapEffect.ID, tmp_mapEffect);

        LoadEffect(tmp_mapEffect.ID, param_ID, tmp_effectTransform, Vector3.one, call);

        return tmp_id;
    }

    public int CreateBodyEffect(string param_ID, Transform param_Target, float param_Time, Vector3 param_Offset,Vector3 param_Scale, Vector3 rot, bool IsFoucus = false, Action<int> call = null, float radius = 0)
    {
        if (string.IsNullOrEmpty(param_ID))
            return -1;
        
        if (param_Time == 0)
        {
            //LogMgr.LogErrorFormat("EffectManager -> CreateBodyEffect : Create Effect {0} with Time 0 ...", param_ID);
            return -1;
        }

        if (param_Target == null)
            return -1;

        int tmp_id = GetUniqueEffectID();

        GameObject tmp_effectObject = new GameObject(param_ID);
        Transform tmp_effectTransform = tmp_effectObject.transform;
        tmp_effectObject.layer = IsFoucus?LayerMask.NameToLayer("FOCUS"):LayerMask.NameToLayer("VFX");
        
        BodyEffect tmp_bodyEffect = new BodyEffect(tmp_id, 0, tmp_effectTransform)
        {
            BodyPoint = param_Target,
            TotalTime = param_Time,
            Offset = param_Offset,
            Rotation = rot
        };
        tmp_bodyEffect.Play();
        m_Effects.Add(tmp_bodyEffect.ID, tmp_bodyEffect);

        LoadEffect(tmp_bodyEffect.ID, param_ID, tmp_effectTransform, param_Scale, call, radius);

        return tmp_id;
    }
    
    public int CreateSenceEffect(string param_ID, Vector3 position, float param_Time, Vector3 param_Offset,Vector3 param_Scale, Vector3 rot, bool IsFoucus = false, Action<int> call = null, float radius = 0)
    {
        if (string.IsNullOrEmpty(param_ID))
            return -1;
        
        if (param_Time == 0)
        {
            //LogMgr.LogErrorFormat("EffectManager -> CreateBodyEffect : Create Effect {0} with Time 0 ...", param_ID);
            return -1;
        }

        int tmp_id = GetUniqueEffectID();

        GameObject tmp_effectObject = new GameObject(param_ID);
        Transform tmp_effectTransform = tmp_effectObject.transform;
        BodyEffect tmp_bodyEffect = new BodyEffect(tmp_id, 0, tmp_effectTransform)
        {
            BodyPoint = null,
            TotalTime = param_Time,
            Offset = position + param_Offset,
            Rotation = rot
        };
        tmp_bodyEffect.Play();
        m_Effects.Add(tmp_bodyEffect.ID, tmp_bodyEffect);

        LoadEffect(tmp_bodyEffect.ID, param_ID, tmp_effectTransform, param_Scale, call, radius);

        return tmp_id;
    }
    
    public int CreateCameraEffect(string param_ID, float param_Time, Vector3 param_Offset,Vector3 param_Scale, bool IsFoucus = false, Action<int> call = null)
    {
        if (param_Time == 0)
        {
            //LogMgr.LogErrorFormat("EffectManager -> CreateBodyEffect : Create Effect {0} with Time 0 ...", param_ID);
            return -1;
        }

        int tmp_id = GetUniqueEffectID();

        GameObject tmp_effectObject = new GameObject(param_ID);
        Transform tmp_effectTransform = tmp_effectObject.transform;
        tmp_effectObject.layer = IsFoucus?LayerMask.NameToLayer("FOCUS"):LayerMask.NameToLayer("VFX");
        
        CameraEffect tmp_bodyEffect = new CameraEffect(tmp_id, 0, tmp_effectTransform)
        {
            TotalTime = param_Time,
            Offset = param_Offset
        };
        tmp_bodyEffect.Play();
        m_Effects.Add(tmp_bodyEffect.ID, tmp_bodyEffect);

        LoadEffect(tmp_bodyEffect.ID, param_ID, tmp_effectTransform, param_Scale, call);

        return tmp_id;
    }
    
    public int CreateUICameraEffect(string param_ID, float param_Time, Vector3 param_Offset,Vector3 param_Scale, Vector3 param_Rot, bool IsFoucus = false, Action<int> call = null)
    {
        if (param_Time == 0)
        {
            //LogMgr.LogErrorFormat("EffectManager -> CreateBodyEffect : Create Effect {0} with Time 0 ...", param_ID);
            return -1;
        }

        int tmp_id = GetUniqueEffectID();

        GameObject tmp_effectObject = new GameObject(param_ID);
        Transform tmp_effectTransform = tmp_effectObject.transform;
        tmp_effectObject.layer = IsFoucus?LayerMask.NameToLayer("FOCUS"):LayerMask.NameToLayer("VFX");
        
        CameraEffect tmp_bodyEffect = new CameraEffect(tmp_id, 0, tmp_effectTransform)
        {
            TotalTime = param_Time,
            Offset = param_Offset,
            Rotation = param_Rot
        };
        tmp_bodyEffect.Play();
        m_Effects.Add(tmp_bodyEffect.ID, tmp_bodyEffect);

        LoadEffect(tmp_bodyEffect.ID, param_ID, tmp_effectTransform, param_Scale, call);

        return tmp_id;
    }
    
    
    public int CreateFlyEffect(string param_ID, Vector3 param_Start, Vector3 param_Direction, float param_Speed, string param_TargetID, float param_DurTime = 3f, bool IsFoucus = false, Action<int> param_Callback = null, bool trace = true)
    {
        /*var tmp_effectConfigItem = GetEffectConfig(param_ID);
        if (tmp_effectConfigItem == null)
            return -1;*/

        int tmp_id = GetUniqueEffectID();

        Creature tmp_Creature = SceneObjectManager.Instance.Find(param_TargetID, true);
        if (tmp_Creature == null || tmp_Creature.mData.IsDead)
            return -1;
    
        Transform tmp_targetTransform = tmp_Creature != null && trace == true ? GetCenter(tmp_Creature) : null;

        GameObject tmp_effectObject = new GameObject("effect" + tmp_id);
        Transform tmp_effectTransform = tmp_effectObject.transform;
        tmp_effectObject.layer = IsFoucus?LayerMask.NameToLayer("FOCUS"):LayerMask.NameToLayer("VFX");

        FlyEffect tmp_flyEffect = new FlyEffect(tmp_id, 0, tmp_effectTransform)
        {
            Start = param_Start,
            Direction = param_Direction,
            Speed = param_Speed,
            Target = tmp_targetTransform,
            TotalTime = param_DurTime,
        };
        tmp_flyEffect.Play();
        m_Effects.Add(tmp_flyEffect.ID, tmp_flyEffect);


        LoadEffect(tmp_flyEffect.ID, param_ID, tmp_effectTransform, Vector3.one, param_Callback, tmp_Creature.Radius);

        return tmp_id;
    }
    
    public int CreateCurveEffect(string param_ID, Vector3 param_Start, Vector3 param_Direction, float param_Speed, string param_TargetID, string targetBone, bool param_SecondFly = false, float param_DurTime = 3f, bool IsFoucus = false, Action<int> param_Callback = null, float height = 3f)
    {
        /*var tmp_effectConfigItem = GetEffectConfig(param_ID);
        if (tmp_effectConfigItem == null)
            return -1;*/

        int tmp_id = GetUniqueEffectID();

        Creature tmp_Creature = SceneObjectManager.Instance.Find(param_TargetID, true);
        if (tmp_Creature == null)
            return -1;
        
        Transform tmp_targetTransform = tmp_Creature != null? tmp_Creature.GetBone(targetBone) : null;

        GameObject tmp_effectObject = new GameObject("effect" + tmp_id);
        tmp_effectObject.layer = IsFoucus?LayerMask.NameToLayer("FOCUS"):LayerMask.NameToLayer("VFX");
        Transform tmp_effectTransform = tmp_effectObject.transform;
        
        float distance = Vector3.Distance(param_Start, tmp_Creature.GetPosition());
        float time = distance / param_Speed;

        CurveEffect tmp_flyEffect = new CurveEffect(tmp_id, 0, tmp_effectTransform, param_SecondFly)
        {
            Start = param_Start,
            Direction = param_Direction,
            Speed = param_Speed,
            Target = tmp_targetTransform,
            TotalTime = time,
            HeightParam = height
        };
        tmp_flyEffect.Play();
        m_Effects.Add(tmp_flyEffect.ID, tmp_flyEffect);


        LoadEffect(tmp_flyEffect.ID, param_ID, tmp_effectTransform, Vector3.one, param_Callback, tmp_Creature.Radius);

        return tmp_id;
    }
    
    public int CreateSLineEffect(string param_ID, Vector3 param_Start, Vector3 param_Direction, float param_Speed, string param_TargetID, string targetBone, float param_DurTime = 3f, bool IsFoucus = false, Action<int> param_Callback = null, float height = 3f, float angleSpeed = 0)
    {
        /*var tmp_effectConfigItem = GetEffectConfig(param_ID);
        if (tmp_effectConfigItem == null)
            return -1;*/

        int tmp_id = GetUniqueEffectID();

        Creature tmp_Creature = SceneObjectManager.Instance.Find(param_TargetID, true);
        if (tmp_Creature == null)
            return -1;
        
        Transform tmp_targetTransform = tmp_Creature != null? tmp_Creature.GetBone(targetBone) : null;

        GameObject tmp_effectObject = new GameObject("effect" + tmp_id);
        tmp_effectObject.layer = IsFoucus?LayerMask.NameToLayer("FOCUS"):LayerMask.NameToLayer("VFX");
        Transform tmp_effectTransform = tmp_effectObject.transform;
        
        float distance = Vector3.Distance(param_Start, tmp_Creature.GetPosition());
        float time = distance / param_Speed;

        SLineEffect tmp_flyEffect = new SLineEffect(tmp_id, 0, tmp_effectTransform)
        {
            Start = param_Start,
            Direction = param_Direction,
            Speed = param_Speed,
            Target = tmp_targetTransform,
            TotalTime = time,
            HeightParam = height,
            AngleParam = angleSpeed
        };
        tmp_flyEffect.Play();
        m_Effects.Add(tmp_flyEffect.ID, tmp_flyEffect);


        LoadEffect(tmp_flyEffect.ID, param_ID, tmp_effectTransform, Vector3.one, param_Callback, tmp_Creature.Radius);

        return tmp_id;
    }
    
    private Transform GetCenter(Creature role)
    {
        return role.GetBone("body_hit");
    }
    private Transform GetBone(string bone, Transform trans)
    {
        List<Transform> bones = new List<Transform>();
        trans.GetComponentsInChildren(bones);

        foreach (var tmp_bone in bones)
        {
            var boneName = tmp_bone.name.ToLower();
            if (boneName.Equals(bone))
            {
                return tmp_bone;
            }
        }

        return null;
    }

    public Bucket Bucket
    {
        get
        {
            return BucketManager.Stuff.Battle;
        }
    }

    public async void LoadEffect(int uniqueId, string effectName, Transform param_Transform, Vector3 param_scale, Action<int> param_Callback, float radius = 0)
    {
        var address = NameFixUtil.ChangeExtension(effectName, ".prefab");
        if (string.IsNullOrEmpty(address))
        {
            Debug.LogError("LoadEffect " + address + " is null");
            return;
        }
      
        var prefab = await Bucket.GetOrAquireAsync<GameObject>(address, true);
        if (prefab == null)
        {
            Debug.LogError("缺少特效资源：" + address);
            return;
        }
            
        BaseEffect effect = null;

        if (m_Effects.TryGetValue(uniqueId, out effect))
        {
            if (!effect.Completed)
            {
                GameObject tmp_instance = GameObject.Instantiate(prefab) as GameObject;
                tmp_instance.transform.SetParent(param_Transform);
                tmp_instance.transform.localPosition = Vector3.zero;
                tmp_instance.transform.localScale = Vector3.one;
                tmp_instance.transform.localRotation = Quaternion.identity;
                tmp_instance.transform.localScale = param_scale;
                effect.SetEffectGameObject(tmp_instance);
                if (param_Callback != null)
                    param_Callback(uniqueId);

                var comp = tmp_instance.transform.GetComponent<ParticleEffectUnit>();
                if (comp != null)
                {
                    //comp.Init(radius);
                }

                return;
            }
        }
    }
    
    //public async static void LoadEffect(string effectName, Action<object> param_Callback)
    //{
    //    var address = effectName + ".prefab";

    //    var prefab = await Bucket.GetOrAquireAsync<GameObject>(address);

    //    if (param_Callback != null)
    //        param_Callback(prefab);
    //}
    
    /// <summary>
    /// 克罗可闹钟瞬移
    /// </summary>
    /// <param name="data"></param>
    public async void ShowClockEffect(Creature role, string clock, float Offset, float HOffset, Vector3 OffsetPos, float DurTime)
    {
        if(role == null)
            return;

        var address = NameFixUtil.ChangeExtension(clock, ".prefab");
        
        var prefab = await BucketManager.Stuff.Battle.GetOrAquireAsync<GameObject>(address);
        
        GameObject tmp_instance = GameObject.Instantiate(prefab) as GameObject;
        tmp_instance.transform.SetParent(role.HeadBone);
        
        tmp_instance.transform.localScale = Vector3.one;
        tmp_instance.transform.localRotation = Quaternion.identity;

        Vector2 xz1 = UnityEngine.Random.insideUnitCircle.normalized * Offset;
        var rotdir = new Vector3(xz1.x, 0, xz1.y);
            rotdir= Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), Vector3.up) * rotdir;
            var headpos = role.HeadBone.position;
            List<Vector3>poses = new List<Vector3>();
        for (int i = 0; i < 3; i++)
        {
            float hoffset = UnityEngine.Random.Range(-HOffset, HOffset);
            if (Vector3.Distance(CameraManager.Instance.MainCamera.transform.position,
                    OffsetPos + new Vector3(rotdir.x, 0, rotdir.z)) >
                Vector3.Distance(CameraManager.Instance.MainCamera.transform.position, headpos))
            {
                hoffset = UnityEngine.Random.Range(HOffset * 0.5f, HOffset);
            }
            
            var pos = new Vector3(rotdir.x, hoffset, rotdir.z);
            poses.Add(OffsetPos + pos);
            rotdir= Quaternion.AngleAxis(120, Vector3.up) * rotdir;
        }
        for (int i = 0; i < 3; i++)
        {
            var index = UnityEngine.Random.Range(0, poses.Count);
            var pos = poses[index];
            poses.RemoveAt(index);
           
            BattleTimer.Instance.DelayCall(0.8f*i, delegate(object[] objects)
            {
                tmp_instance.SetActive(false);
                tmp_instance.SetActive(true);
                float offset = Offset;
            
                tmp_instance.transform.localPosition = pos;//+ new Vector3(UnityEngine.Random.Range(-offset, offset), UnityEngine.Random.Range(-offset, offset), UnityEngine.Random.Range(-offset, offset));
                tmp_instance.transform.Find("anim").LookAt(role.HeadBone.position);
            });
        }

        BattleTimer.Instance.DelayCall(DurTime,
            delegate(object[] objects) { GameObject.Destroy(tmp_instance); });
        
    }
}