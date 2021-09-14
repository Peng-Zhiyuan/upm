using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;


public class GameObjectPool : StuffObject<GameObjectPool>
{
    public Dictionary<GameObject, Queue<RecycledGameObject>> dic = new Dictionary<GameObject, Queue<RecycledGameObject>>();

    private GameObject _root;
    public GameObject Root
    {
        get
        {
            if(_root == null)
            {
                _root = new GameObject();
                _root.name = "GameObjectPoolRoot";
                GameObject.DontDestroyOnLoad(_root);
            }
            return _root;
        }
    }

    public void RemoveAll()
    {
        foreach(var kv in this.dic)
        {
            var prefab = kv.Key;
            var goQueue = kv.Value;
            while(goQueue.Count > 0)
            {
                var recycleable = goQueue.Dequeue();
                var go = recycleable.GameObject;
                GameObject.Destroy(go);
            }
        }
        dic.Clear();
    }


    public T Reuse<T>(GameObject prefab, bool isActive = true, Type defaultType = null) where T : RecycledGameObject
    {
        if(prefab == null)
        {
            return default(T);
        }
        //Debug.Log("prefab: " + prefab);
        var recycleableGo = TakeoutFromDic(prefab);
        if(recycleableGo == null)
        {
            var go = GameObject.Instantiate(prefab);
            recycleableGo = GetNativeOrHotComponent<T>(go, defaultType);
            recycleableGo.Prefab = prefab;
            recycleableGo.IsVirgin = true;
        }
        else{
            recycleableGo.IsVirgin = false;
        }
        recycleableGo.OnResuse();
        recycleableGo.GameObject.SetActive(isActive);

        return (T)recycleableGo;
    }


    public T GetNativeOrHotComponent<T>(GameObject go, Type defaultType = null) where T : RecycledGameObject
    {
        var comp = go.GetComponent<RecycledGameObject>();
        if(comp != null)
        {
            var obj = comp as object;
            return (T)obj;
        }
        if(defaultType != null)
        {
            var addComp = go.AddComponent(defaultType);
            var obj = addComp as object;
            return (T)obj;
        }
       

        throw new Exception("threre is no RecycledGameObject");

    }


    private void AddToDic(GameObject prefab, RecycledGameObject instance)
    {
        Queue<RecycledGameObject> list;
        dic.TryGetValue(prefab, out list);
        if(list == null)
        {
            list = new Queue<RecycledGameObject>();
            dic[prefab] = list;
        }
        list.Enqueue(instance);
    }

    private RecycledGameObject TakeoutFromDic(GameObject prefab)
    {
        Queue<RecycledGameObject> queue;
        dic.TryGetValue(prefab, out queue);
        if(queue == null)
        {
            return null;
        }
        if(queue.Count == 0)
        {
            return null;
        }
        var instance = queue.Dequeue();
        return instance;
    }

    public void Recycle(RecycledGameObject instance)
    {
        if(instance == null)
        {
            return;
        }
        instance.GameObject.SetActive(false);
        instance.OnRecycle();
        var prefab = instance.Prefab;
        AddToDic(prefab, instance);
        instance.Transform.SetParent(Root.transform, false);
    }
}