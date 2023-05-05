using System;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;
using Task = System.Threading.Tasks.Task;

public static class DrawEliminationEffect
{
    public static Transform EffectNode { set; get; }
    
    public static async void PlayEffect(string effectName, Vector3 pos)
    {
        var effectInstance = await _GetInstance(effectName);
        effectInstance.transform.position = pos;
        // 到时间就销毁
        await Task.Delay(1000);

        if (null == effectInstance) return;
        effectInstance.Recycle();
    }

    public static async void FlyEffect(string effectName, Vector3 pos1, Vector3 pos2, Action completeHandler)
    {
        var effectInstance = await _GetInstance(effectName);
        effectInstance.transform.position = pos1;
        var passPos = pos1;
        passPos.x = pos1.x * .2f + pos2.x * .8f;
        passPos.y = pos2.y + 400 * Random.Range(.8f, .12f);
        
        var path = new Vector3[3];
        path[0] = pos1;
        path[1] = passPos;
        path[2] = pos2;

        var distance = Vector2.Distance(pos1, pos2);
        Debug.Log($"距离：{distance}");
        effectInstance.transform.DOPath(path, .1f + .1f * distance / 100, PathType.CatmullRom).OnComplete(() =>
        {
            if (null == effectInstance) return;
            effectInstance.Recycle();
            completeHandler?.Invoke();
        });
    }
    
    public static async void FlyStraightEffect(string effectName, Vector3 pos1, Vector3 pos2, Action completeHandler)
    {
        var effectInstance = await _GetInstance(effectName);
        effectInstance.transform.position = pos1;
        

        var distance = Vector2.Distance(pos1, pos2);
        Debug.Log($"距离：{distance}");
        
        effectInstance.transform.DOMove(pos2, .1f + .1f * distance / 100).OnComplete(() =>
        {
            if (null == effectInstance) return;
            effectInstance.Recycle();
            completeHandler?.Invoke();
        });
    }

    private static async Task<RecycledGameObject> _GetInstance(string effectName)
    {
        var bucket = BucketManager.Stuff.Main;
        var effect = await bucket.GetOrAquireAsync<GameObject>($"{effectName}.prefab");
        var effectInstance = bucket.Pool.Reuse<RecycledGameObject>(effect);
        effectInstance.transform.SetParent(EffectNode);

        return effectInstance;
    }
}

