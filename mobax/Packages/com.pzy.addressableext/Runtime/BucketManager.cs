using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BucketManager : StuffObject<BucketManager>
{
    public static bool LogDetail;

    public static Bucket _editModeBucket;
    public static Bucket EditModeBucket
    {
        get
        {
            if(!Application.isEditor)
            {
                throw new Exception("[BuketManager] EditModeBucket only accessable in edit mode");
            }
            if(_editModeBucket == null)
            {
                _editModeBucket = new Bucket("editMode");
            }
            return _editModeBucket;
        }
    }

    [ShowInInspector]
    Dictionary<string, Bucket> nameToBucketDic = new Dictionary<string, Bucket>();

    public Bucket GetBucket(string name = "Main")
    {
        if (!Application.isPlaying)
        {
            throw new Exception("[BuketManager] GetBucket only accessable in playing mode, use BuketManager.EditModeBucket instead");
        }
       
        var bucket = DictionaryUtil.TryGet(nameToBucketDic, name, null);
        if (bucket == null)
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

    public Bucket Main
    {
        get
        {
            var bucket = this.GetBucket("Main");
            return bucket;
        }
    }

    public Bucket Conf
    {
        get
        {
            var bucket = this.GetBucket("Conf");
            return bucket;
        }
    }

    public Bucket Font
    {
        get
        {
            var bucket = this.GetBucket("Font");
            return bucket;
        }
    }

    public Bucket Battle
    {
        get
        {
            var bucket = this.GetBucket("Battle");
            return bucket;
        }
    }
    
    // 剧情桶
    public Bucket Plot
    {
        get
        {
            var bucket = this.GetBucket("Plot");
            return bucket;
        }
    }
    
    // 工具类专用桶
    public Bucket Tool
    {
        get
        {
            var bucket = this.GetBucket("Tool");
            return bucket;
        }
    }
}