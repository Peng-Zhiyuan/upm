using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class MatsPool : Single<MatsPool>
{
    //Dictionary<string, List<Material>> matDic = new Dictionary<string, List<Material>>();
    public Material TakeMaterial(string matName)
    {
        var matPath = $"{matName}.mat";
        var bucket = BucketManager.Stuff.Battle;
        var mat = bucket.Get<Material>(matPath);

        var newMatInstance = new Material(mat);
        return newMatInstance;
        /*if (matDic.ContainsKey(matName) && matDic[matName].Count > 0)
        {
            var mat = matDic[matName][0];
            matDic[matName].RemoveAt(0);
            return mat;
        }
        else 
        {
            var matPath = $"{matName}.mat";
            var bucket = BucketManager.Stuff.Battle;
            var mat = bucket.Get<Material>(matPath);

            var newMatInstance = new Material(mat);
            return newMatInstance;
        }*/
    }

    public async Task CacheMaterialAsync(string matName)
    {
        var matPath = $"{matName}.mat";
        var bucket = BucketManager.Stuff.Battle;
        await bucket.AquireIfNeedAsync(matPath);
    }

/*    public void PutMaterial(string matName, Material mat)
    {
        if (!matDic.ContainsKey(matName)) matDic[matName] = new List<Material>();
        matDic[matName].Add(mat);
    }*/
}
