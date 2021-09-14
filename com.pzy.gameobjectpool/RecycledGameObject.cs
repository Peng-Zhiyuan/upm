using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Sirenix.OdinInspector;

public class RecycledGameObject : SerializedMonoBehaviour
{
    [ReadOnly]
    public GameObject prefab;

    public GameObject Prefab
    {
        get
        {
            return this.prefab;
        }
        set
        {
            this.prefab = value;
        }
    }

    public GameObject GameObject
    {
        get
        {
            return this.gameObject;
        }
    }

    public Transform Transform
    {
        get
        {
            return this.transform;
        }
    }

    public bool IsVirgin
    {
        get;
        set;
    }
    
    public virtual void OnResuse()
    {

    }

    public virtual void OnRecycle()
    {

    }

    public void Recycle()
    {
        GameObjectPool.Stuff.Recycle(this);
    }
}