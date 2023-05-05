using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public static class GameObjectPoolUtil
{
    public static async Task<T> ReuseAddressableObjectAsync<T>(Bucket bucket, string prefabAddress) where T : RecycledGameObject
    {
        var prefab = await bucket.GetOrAquireAsync<GameObject>(prefabAddress, true);
        if (prefab == null)
        {
            return null;
        }
        var instanceObject = bucket.Pool.Reuse<T>(prefab);
        return instanceObject;
    }
}