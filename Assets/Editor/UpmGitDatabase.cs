using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Threading.Tasks;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

public static class UpmGitDatabase 
{
    public static List<UpmGitPackageInfo> packageInfoList = new List<UpmGitPackageInfo>();
    public static UpmGitDatabaseStatus status = UpmGitDatabaseStatus.NeedUpdateInfo;

    public static async void UpdatePackageInfoInBackground()
    {
        status = UpmGitDatabaseStatus.UpdatingPackageInfo;

        // 获取内置包信息
        var list = await UpmGitUtil.GetEmbededPackageInfoListAsync();
        var ret = new List<UpmGitPackageInfo>();
        foreach(var one in list)
        {
            var package = new UpmGitPackageInfo(one);
            ret.Add(package);
        }
        packageInfoList = ret;
        var count = list.Count;

        Debug.Log($"[UpmGitDatabase] package info list updated, {count} embeded package(s) found");

        status = UpmGitDatabaseStatus.AllRight;
    }

}

public enum UpmGitDatabaseStatus
{
    NeedUpdateInfo,
    AllRight,
    UpdatingPackageInfo
}