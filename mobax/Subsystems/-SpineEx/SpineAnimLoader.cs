using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Spine.Unity;
public class SpineAnimLoader : Single<SpineAnimLoader>
{
    private Dictionary<int, SkeletonDataAsset> skeletonDataAssetDic = new Dictionary<int, SkeletonDataAsset>();
    public async Task InitAsync()
    {
        var address = "SpinePlayerUnit.prefab";
        //await AssetCacher.Stuff.LoadAndCacheAsync(address);
        var bucket = BucketManager.Stuff.GetBucket(UIEngine.LatestNavigatePageName);
        await bucket.AquireIfNeedAsync(address);
    }
    public async Task<SpinePlayerUnit> LoadSpinePlayerAsync(Bucket bucket, int sdId, bool visible = true)
    {
        var spinePlayerUnit = await GameObjectPoolUtil.ReuseAddressableObjectAsync<SpinePlayerUnit>(bucket, "SpinePlayerUnit.prefab");
        //spinePlayerUnit.Hide();
        if (!skeletonDataAssetDic.ContainsKey(sdId))
        {
            string path = $"Unit{sdId}_SkeletonData.asset";
            //var skeletonDataAsset = await AddressableRes.AquireAsync<SkeletonDataAsset>(path);
            var skeletonDataAsset = await bucket.GetOrAquireAsync<SkeletonDataAsset>(path);

            if (skeletonDataAsset == null)
            {
                Debug.LogError($"skeletonDataAsset load fail:{path}");
                return spinePlayerUnit;
            }
            else
            {
                skeletonDataAssetDic[sdId] = skeletonDataAsset;
            }
        }
        spinePlayerUnit.SetSkeletonDataAsset(skeletonDataAssetDic[sdId], sdId);
        spinePlayerUnit.skeletonGraphic.Initialize(true);
      
        spinePlayerUnit.PlayAnim("Wait", true);
        spinePlayerUnit.skeletonGraphic.Update(0.05f);
        //spinePlayerUnit.skeletonGraphic.AnimationState.SetAnimation(0, "Wait", true);
       
        //if(visible)spinePlayerUnit.Show();
        return  spinePlayerUnit;
    }

    public async Task<SpinePlayerUnit>  LoadSpinePlayerAsync(Bucket bucket, int sdId, Transform parent, Vector3 offset = default(Vector3), Vector3 scale = default(Vector3), bool visible = true)
    {
        var spinePlayerUnit = await SpineAnimLoader.Instance.LoadSpinePlayerAsync(bucket, sdId, visible);
        spinePlayerUnit.transform.CustomSetParent(parent, offset, scale);
        return spinePlayerUnit;
    }
}
