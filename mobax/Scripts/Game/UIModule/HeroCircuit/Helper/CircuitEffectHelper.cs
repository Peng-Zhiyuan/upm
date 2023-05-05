using System.Threading.Tasks;
using UnityEngine;

public static class CircuitEffectHelper
{
    public static Transform EffectNode { get; set; }

    public static async void PlaySettle(int shape, Vector3 pos)
    {
        var circuitShapeCfg = StaticData.PuzzleShapeTable.TryGet(shape);
        var bucket = BucketManager.Stuff.Main;
        var effect = await bucket.GetOrAquireAsync<GameObject>($"fx_ui_herocircuit_anim_{circuitShapeCfg.Qlv}_{circuitShapeCfg.Shapetype}.prefab");
        if (null == EffectNode) return;
        
        var effectInstance = bucket.Pool.Reuse<RecycledGameObject>(effect);
        effectInstance.transform.SetParent(EffectNode);
        effectInstance.transform.SetLocalScale(Vector3.one);
        effectInstance.transform.localRotation = Quaternion.Euler(0, 0, -circuitShapeCfg.Angle);
        effectInstance.SetPosition(pos);
        // 例子特效也播放起来
        _PlaySparkle(pos);
        // 到时间就销毁
        await Task.Delay(1000);
        
        effectInstance.Recycle();
    }

    private static async void _PlaySparkle(Vector3 pos)
    {
        var bucket = BucketManager.Stuff.Main;
        var effect = await bucket.GetOrAquireAsync<GameObject>($"fx_ui_herocircuit.prefab");
        if (null == EffectNode) return;
        
        var effectInstance = bucket.Pool.Reuse<RecycledGameObject>(effect);
        effectInstance.transform.SetParent(EffectNode);
        effectInstance.SetPosition(pos);
        // 到时间就销毁
        await Task.Delay(1000);
        
        effectInstance.Recycle();
    }
}