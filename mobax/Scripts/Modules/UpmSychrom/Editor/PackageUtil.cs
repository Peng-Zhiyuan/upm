using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor;
using System;
using System.Threading.Tasks;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

public static class PackageUtil 
{
    public async static Task<List<PackageInfo>> GetEmbededPackageInfoListAsync()
    {
        var request = Client.List(true, false);
        await PackageOperationWaiter.Await(request);
        var list = request.Result;
        var retList = new List<PackageInfo>();
        foreach (var one in list)
        {
            var name = one.name;
            var type = one.source;
            if (type == PackageSource.Embedded)
            {
                retList.Add(one);
            }
        }
        return retList;
    }
}
