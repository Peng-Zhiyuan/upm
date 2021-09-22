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

    bool? _isReleaseTagExists;
    public bool IsReleaseTagExists
    {
        get
        {
            if(_isReleaseTagExists == null)
            {
                var releaseTagName = this.ReleaseTagName;
                var isTagExists = UpmGitUtil.IsGitTagExists(releaseTagName);
                this._isReleaseTagExists = isTagExists;
            }
            return this._isReleaseTagExists.Value;
        }
        set
        {
            _isReleaseTagExists = value;
        }
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

    public bool CreateReleaseTag()
    {
        var tagName = this.ReleaseTagName;
        var isSuccess = UpmGitUtil.CreateTag(tagName);
        return isSuccess;
    }

    public TextAsset PackageAsset
    {
        get
        {
            var packageDiPath = this.nativePackageInfo.assetPath;
            var packageJsonPath = $"{packageDiPath}/package.json";
            Debug.Log(packageJsonPath);
            var asset = AssetDatabase.LoadAssetAtPath(packageJsonPath, typeof(TextAsset)) as TextAsset;
            return asset;
        }
    }
}
