using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Task = System.Threading.Tasks.Task;

public static class LauncherGameEffect
{
    public static Transform EffectNode { set; get; }
    
    private static Dictionary<string, Transform> _effectMap;
    private static Dictionary<string, Queue<Transform>> _poolMap;

    public static void Register(string effectName, Transform effectPrefab)
    {
        _effectMap ??= new Dictionary<string, Transform>();
        _effectMap[effectName] = effectPrefab;

        if (null == _poolMap)
        {
            _poolMap = new Dictionary<string, Queue<Transform>>();
        }
        else
        {
            _poolMap.Clear();
        }
    }
    
    public static async void PlayEffect(string effectName, Vector3 pos)
    {
        var effectInstance = _Take(effectName);
        effectInstance.position = pos;
        // 到时间就销毁
        await Task.Delay(1000);
        
        if (null == effectInstance) return;
        _Recycle(effectName, effectInstance);
    }

    public static async void FlyEffect(string effectName, Vector3 pos1, Vector3 pos2, Action completeHandler)
    {
        var effectInstance = _Take(effectName);
        effectInstance.position = pos1;
        var passPos = pos1;
        passPos.x = pos1.x * .2f + pos2.x * .8f;
        passPos.y = pos2.y + 400 * Random.Range(.8f, .12f);
        
        var path = new Vector3[3];
        path[0] = pos1;
        path[1] = passPos;
        path[2] = pos2;

        var distance = Vector2.Distance(pos1, pos2);
        Debug.Log($"距离：{distance}");
        effectInstance.DOPath(path, .1f + .1f * distance / 100, PathType.CatmullRom).OnComplete(() =>
        {
            if (null == effectInstance) return;
            _Recycle(effectName, effectInstance);
        });
    }
    
    public static async void FlyStraightEffect(string effectName, Vector3 pos1, Vector3 pos2, Action completeHandler)
    {
        var effectInstance = _Take(effectName);
        effectInstance.position = pos1;
        

        var distance = Vector2.Distance(pos1, pos2);
        Debug.Log($"距离：{distance}");
        
        effectInstance.DOMove(pos2, .1f + .05f * distance / 100).OnComplete(() =>
        {
            if (null == effectInstance) return;
            _Recycle(effectName, effectInstance);
            completeHandler?.Invoke();
        });
    }

    private static Transform _Take(string effectName)
    {
        if (!_poolMap.TryGetValue(effectName, out var pool))
        {
            pool = _poolMap[effectName] = new Queue<Transform>();
        }
        var instance = pool.Count > 0 ? pool.Dequeue() : Object.Instantiate(_effectMap[effectName], EffectNode);
        instance.SetActive(true);

        return instance;
    }

    private static void _Recycle(string effectName, Transform effect)
    {
        if (!_poolMap.TryGetValue(effectName, out var pool))
        {
            pool = _poolMap[effectName] = new Queue<Transform>();
        }
        
        pool.Enqueue(effect);
        effect.SetActive(false);
    }
}

