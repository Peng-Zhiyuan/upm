using UnityEngine;
using System.Collections;
using Table;

using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Threading.Tasks;
using BattleSystem.Core;
//namespace LuaFramework
//{

public class EmotionManager : BattleComponent<EmotionManager>
{
    private Transform _emoteRoot = null;
    // private GameObject _damagePrefab = null;
    //private GameObjectPool _pool = null;


    public override async Task OnLoadResourcesAsync()
    {



        //await AssetCacher.Stuff.LoadAndCacheAsync("Effect_BE_Alert.prefab");
        //await AssetCacher.Stuff.LoadAndCacheAsync("Effect_BE_Flee.prefab");
        //await AssetCacher.Stuff.LoadAndCacheAsync("Effect_BE_Patrol.prefab");
        //await AssetCacher.Stuff.LoadAndCacheAsync("Effect_BE_Sos.prefab");
        var bucket = BucketManager.Stuff.Main;
        await bucket.AquireIfNeedAsync("Effect_BE_Alert.prefab");
        //await bucket.AquireIfNeedAsync("Effect_BE_Flee.prefab");
        //await bucket.AquireIfNeedAsync("Effect_BE_Patrol.prefab");
        //await bucket.AquireIfNeedAsync("Effect_BE_Sos.prefab");

        /*         ResourceManager.Instance.LoadPrefab("ui_hud", "DamagePanel", delegate (UnityEngine.Object[] objs)
                 {
                     if (objs.Length == 0) return;
                     _damagePrefab = objs[0] as GameObject;

                     _pool = ObjectPoolManager.Instance.CreatePool("Damage", 10, 100, _damagePrefab);
                 });*/
    }

    public async Task<RecycledGameObject> ShowEmotePanel(Transform root, StateEmotion emotion)
    {

        var go = await GameObjectPoolUtil.ReuseAddressableObjectAsync<RecycledGameObject>(BucketManager.Stuff.Battle, $"Effect_BE_{emotion.ToString()}.prefab");
        go.transform.SetParent(root);
        go.transform.localScale = Vector3.one;
        return go;

    }
}
//}