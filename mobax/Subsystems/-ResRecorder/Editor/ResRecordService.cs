using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResRecordService : Service
{
    public override void OnCreate()
    {
        Bucket.ResLoaded += OnAnyBucketResLoaded;
    }

    void OnAnyBucketResLoaded(Object asset)
    {
        ResRecorder.Stuff.Record(asset);
    }
}
