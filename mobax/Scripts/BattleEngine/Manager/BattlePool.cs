namespace BattleEngine.View
{
    using System.Collections.Generic;
    using UnityEngine;
    using Logic;

    public sealed class BattlePool<T, P> where T : UnitEntityBase<P>
    {
        List<T> pool = new List<T>();
        public static BattlePool<T, P> Instance { get; private set; }

        public void Init()
        {
            Instance = this;
        }

        public T Create(GameObject prefab, Transform parent = null)
        {
            T t;
            if (Count > 0)
            {
                t = pool[pool.Count - 1];
                pool.RemoveAt(pool.Count - 1);
            }
            else
            {
                GameObject go;
                if (parent != null)
                {
                    go = Object.Instantiate(prefab, parent);
                }
                else
                {
                    go = Object.Instantiate(prefab);
                }
                t = go.GetComponent<T>() ?? go.AddComponent<T>();
            }
            if (parent)
            {
                t.SetParent(parent);
            }
            TransformUtil.InitTransformInfo(t.transform);
            t.SetActive(true);
            return t;
        }

        public void Recycle(T t)
        {
            t.SetActive(false);
            pool.Add(t);
        }

        public int Count
        {
            get { return pool.Count; }
        }
    }
}