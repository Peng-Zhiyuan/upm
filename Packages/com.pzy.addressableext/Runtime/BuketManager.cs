using Sirenix.OdinInspector;
using System.Collections.Generic;

public class BuketManager : StuffObject<BuketManager>
{
    [ShowInInspector]
    Dictionary<string, Bucket> nameToBucketDic = new Dictionary<string, Bucket>();

    public Bucket GetBucket(string name = "main")
    {
        var bucket = DictionaryUtil.TryGet(nameToBucketDic, name, null);
        if(bucket == null)
        {
            bucket = new Bucket(name);
            nameToBucketDic[name] = bucket;
        }
        return bucket;
    }

    public void ReleaseBucket(string name)
    {
        var bucket = DictionaryUtil.TryGet(nameToBucketDic, name, null);
        if (bucket != null)
        {
            bucket.ReleaseAll();
        }
    }

}
