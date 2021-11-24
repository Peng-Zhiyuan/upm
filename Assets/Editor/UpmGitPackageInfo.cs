using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Threading.Tasks;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

public class UpmGitPackageInfo
{
    public UpmGitPackageInfo(PackageInfo nativePackageInfo)
    {
        this.nativePackageInfo = nativePackageInfo;
    }

    public PackageInfo nativePackageInfo;

    public bool? isReleaseTagExists;


    public async void GetIsReleaseTagExistsInBackground()
    {
        var releaseTagName = this.ReleaseTagName;
        var isTagExists = await UpmGitUtil.GetIsGitTagExistsAsync(releaseTagName);
        this.isReleaseTagExists = isTagExists;
    }

    public string ReleaseTagName
    {
        get
        {
            var natviePackage = this.nativePackageInfo;
            var tagName = UpmGitUtil.GetGitReleaseTagName(natviePackage);
            return tagName;
        }
    }

    public async void TryCreateReleaseTagInBackground()
    {
        var tagName = this.ReleaseTagName;
        var isSuccess = await UpmGitUtil.CreateTagAsync(tagName);
        if(isSuccess)
        {
            this.isReleaseTagExists = true;
        }
        else
        {
            Debug.LogError("create release tag error");
        }
        
    }

    public TextAsset PackageAsset
    {
        get
        {
            var packageDiPath = this.nativePackageInfo.assetPath;
            var packageJsonPath = $"{packageDiPath}/package.json";
            //Debug.Log(packageJsonPath);
            var asset = AssetDatabase.LoadAssetAtPath(packageJsonPath, typeof(TextAsset)) as TextAsset;
            return asset;
        }
    }

    public bool? isReleasedOnOpenUpm;

    public async void GetIsReleasedOnOpenUpmInBackground()
    {
        var packageName = this.nativePackageInfo.name;
        var b = await UpmGitUtil.GetIsPackageExistsOnOpenUpmAsync(packageName);
        this.isReleasedOnOpenUpm = b;
    }

}
