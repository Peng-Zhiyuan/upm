using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Sirenix.OdinInspector;

public class GameObjectPoolV2
{
    [ShowInInspector, ReadOnly]
    Dictionary<GameObject, Queue<Recycleable>> dic = new Dictionary<GameObject, Queue<Recycleable>>();

    [ShowInInspector, ReadOnly]
    GameObject root;
    public void SetStorageRoot(GameObject root)
    {
        this.root = root;
    }

    public T Reuse<T>(T prefab, Transform parent, bool isActive) where T : Component
    {
        if (prefab == null)
        {
            throw new Exception("prefab is null");
        }

        var prefabGo = prefab.gameObject;
        var recyclable = TakeoutFromDic(prefabGo);
        if (recyclable == null)
        {
            var go = GameObject.Instantiate(prefabGo, parent);
            recyclable = go.GetOrAddComponent<Recycleable>();
            recyclable.prefab = prefabGo;
        }
        else
        {
            if(recyclable.transform.parent != parent)
            {
                recyclable.transform.SetParent(parent, false);
            }
        }
        recyclable.gameObject.SetActive(isActive);
        var ret = recyclable.GetComponent<T>();
        return ret;
    }

    /// <summary>
    /// 如果目标对象上有回收信息，则进行回收，否则进行销毁
    /// 不修改游戏对象的层级信息
    /// </summary>
    /// <param name="gameObject"></param>
    public void Recycle(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }
        var recycleable = gameObject.GetComponent<Recycleable>();
        if (recycleable == null)
        {
            GameObject.Destroy(gameObject);
            return;
        }
        AnimatorUtil.ResetToDefaultState(gameObject);
        gameObject.SetActive(false);
        var prefab = recycleable.prefab;
        AddToDic(prefab, recycleable);
        if(root != null)
        {
            gameObject.transform.SetParent(this.root.transform, false);
        }
    }

    void AddToDic(GameObject prefab, Recycleable instance)
    {
        Queue<Recycleable> list;
        dic.TryGetValue(prefab, out list);
        if (list == null)
        {
            list = new Queue<Recycleable>();
            dic[prefab] = list;
        }
        list.Enqueue(instance);
    }

    Recycleable TakeoutFromDic(GameObject prefab)
    {
        Queue<Recycleable> queue;
        dic.TryGetValue(prefab, out queue);
        if (queue == null)
        {
            return null;
        }
        if (queue.Count == 0)
        {
            return null;
        }
        var instance = queue.Dequeue();
        return instance;
    }


}