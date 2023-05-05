using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using BattleEngine.Logic;

public class CameraOcclusion : MonoBehaviour
{
    private Transform currentTarget;

    //上次碰撞的物体
    private Dictionary<GameObject, bool> _lastColliderObjects = new Dictionary<GameObject, bool>();

    private Dictionary<GameObject, bool> _colliderObjects = new Dictionary<GameObject, bool>();

    public float intervalTime = 0.2f;

    private Transform selfTrans;
    private int CameraOccLayerIndex = 0;

    private void Awake()
    {
        selfTrans = transform;
        CameraOccLayerIndex = LayerMask.GetMask("CameraOcc");
        AddListen();
        CancelInvoke();
        InvokeRepeating("CameraOcclusionUpdate", 0, 1.0f);
    }

    private void OnDestroy()
    {
        RemoveListen();
    }

    void AddListen()
    {
        GameEventCenter.AddListener(GameEvent.SelectChanged, this, ObjectCreated);
    }

    void RemoveListen()
    {
        GameEventCenter.RemoveListener(GameEvent.SelectChanged, this);
    }

    void ObjectCreated(object[] param_Objects)
    {
        if(param_Objects == null || param_Objects.Length == 0)
            return;
        
        Creature creature = (Creature)param_Objects[0];
        if(creature == null || creature.GetModelObject == null)
            return;

        currentTarget = creature.GetModelObject.transform;
    }

    void CameraOcclusionUpdate()
    {
        _lastColliderObjects.Clear();
        if (_colliderObjects.Count > 0)
        {
            foreach (var pair in _colliderObjects)
            {
                _lastColliderObjects.Add(pair.Key, true);
            }
            _colliderObjects.Clear();
        }
        if (currentTarget != null)
        {
            if (MathHelper.DoubleDistanceVect3(currentTarget.position, selfTrans.position) < 400)
                CheckCollider();
        }
        else
        {
            CheckCollider();
        }
        if (_lastColliderObjects.Count > 0)
        {
            foreach (var pair in _lastColliderObjects.ToList())
            {
                if (_colliderObjects.ContainsKey(pair.Key))
                {
                    _lastColliderObjects.Remove(pair.Key);
                }
            }
            foreach (var pair in _lastColliderObjects)
            {
                ShowChildren(pair.Key, true);
            }
        }
    }

    private RaycastHit[] hit;

    //检测遮挡
    void CheckCollider()
    {
        if (currentTarget == null)
        {
            Vector3 starPosition = selfTrans.forward * 20;
            Debug.DrawRay(starPosition, selfTrans.forward * -1, Color.red);
            hit = Physics.RaycastAll(starPosition, selfTrans.forward * -1, 100f, CameraOccLayerIndex); //起始位置、方向、距离
        }
        else
        {
            Vector3 forward = (selfTrans.position - currentTarget.position).normalized;
            float distance = Vector3.Distance(currentTarget.position, selfTrans.position);
            Debug.DrawRay(currentTarget.position, forward * distance, Color.red);
            hit = Physics.RaycastAll(currentTarget.position, forward, distance, CameraOccLayerIndex); //起始位置、方向、距离    
        }
        for (int i = 0; i < hit.Length; i++)
        {
            if (!hit[i].collider.gameObject.name.Equals("LocalPlayerCamera")
                && !hit[i].collider.gameObject.name.Equals("Editable Poly") //地面
                && !hit[i].collider.gameObject.CompareTag("Role")) //角色
            {
                if (!_colliderObjects.ContainsKey(hit[i].collider.gameObject))
                    _colliderObjects.Add(hit[i].collider.gameObject, true);
                if (!_lastColliderObjects.ContainsKey(hit[i].collider.gameObject))
                {
                    ShowChildren(hit[i].collider.gameObject, false);
                }
            }
        }
    }

    void ShowChildren(GameObject go, bool vis)
    {
        Transform[] rootTrans = go.transform.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < rootTrans.Length; i++)
        {
            if (rootTrans[i] == go.transform)
            {
                continue;
            }
            rootTrans[i].SetActive(vis);
            break;
        }
    }
}