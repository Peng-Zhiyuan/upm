using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Linq;
using UnityEngine.ResourceManagement;

public class BuketService : Service
{
    public override void OnCreate()
    {
        var detail = false;

        BucketManager.LogDetail = detail;
        // address 找不到不需要打印
        if(!detail)
        {
            ResourceManager.ExceptionHandler = (a, b) =>
            {
                // do notion
            };
        }
    }

}