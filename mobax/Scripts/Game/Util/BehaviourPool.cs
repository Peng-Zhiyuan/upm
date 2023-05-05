using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class BehaviourPool<T> where T : MonoBehaviour
{
    private T _prefab;
    private Transform _parent;
    private List<T> _usingList;
    private Queue<T> _pool;
    private int _usedCount;

    public List<T> List => _usingList;
    
    public void SetPrefab(T prefab)
    {
        _prefab = prefab;
        
        // 如果本来就放在舞台上的， 那就要隐藏掉并放进池里
        var scene = prefab.gameObject.scene;
        if (!string.IsNullOrEmpty(scene.name))
        {
            _PutIntoPool(prefab);
        }
        
        // 如果有脏东西，都清掉吧
        _usedCount = 0;
        RecycleLeft();
    }
    
    public void SetParent(Transform p)
    {
        _parent = p;
    }

    public T Get(Transform parent = null)
    {
        T item;
        _usingList ??= new List<T>();
        parent ??= _parent;
        if (_usedCount < _usingList.Count)
        {
            item = _usingList[_usedCount];
            item.transform.SetParent(parent);
        }
        else
        {
            if (null != _pool && _pool.Count > 0)
            {
                item = _pool.Dequeue();
                item.transform.SetParent(parent);
            }
            else
            {
                item = Object.Instantiate(_prefab, parent);
            }
            _usingList.Add(item);
        }
        ++_usedCount;
        item.gameObject.SetActive(true);
        
        return item;
    }

    public void Add(T item)
    {
        _usingList ??= new List<T>();
        if (_usedCount < _usingList.Count)
        {
            var tmpItem = _usingList[_usedCount];
            _usingList[_usedCount] = item;
            _usingList.Add(tmpItem);
        }
        else
        {
            _usingList.Add(item);
        }

        ++_usedCount;
    }

    // 只是做一个标记清0， 而舞台上的item也是依旧能用于建设。
    // 剩余的只要调用一个RecycleLeft， 就可以都只把剩下的放进池里
    public void MarkClear()
    {
        _usedCount = 0;
    }

    /// <summary>
    /// 剩余的都放进池里。
    /// 也就是舞台上
    /// </summary>
    public void RecycleLeft()
    {
        if (null == _usingList) return;
        
        for (var i = _usedCount; i < _usingList.Count; ++i)
        {
            _PutIntoPool(_usingList[i]);
        }
        
        _usingList.RemoveRange(_usedCount, _usingList.Count - _usedCount);
    }

    public void Recycle(T item)
    {
        if (!_usingList.Contains(item))
        {
            throw new Exception("Item not in the list. please check");
        }

        _PutIntoPool(item);
        _usingList.Remove(item);
        --_usedCount;
    }
    
    private void _PutIntoPool(T item)
    {
        _pool ??= new Queue<T>();
        
        _pool.Enqueue(item);
        item.gameObject.SetActive(false);
    }
}