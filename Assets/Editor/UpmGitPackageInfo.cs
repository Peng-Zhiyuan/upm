using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Threading.Tasks;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using CustomLitJson;
using System.IO;
using UnityEditor.PackageManager;

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

    public async void IncreaseVersionInBackground(Action onComplete)
    {
        var packageName = this.nativePackageInfo.name;
        var packageAsset = PackageAsset;
        var json = packageAsset.text;
        var jo = JsonMapper.Instance.ToObject(json);
        var currentVersion = this.nativePackageInfo.version;

        var parts = currentVersion.Split('.');
        var lastIndex = parts.Length - 1;
        var lastPart = parts[lastIndex];
        var lastNumber = int.Parse(lastPart);
        lastNumber++;
        parts[lastIndex] = lastNumber.ToString();
        var newVersion = string.Join(".", parts);

        jo["version"] = newVersion;
        var packageDiPath = this.nativePackageInfo.assetPath;
        var packageJsonPath = $"{packageDiPath}/package.json";
        var newJson = JsonMapper.Instance.ToJson(jo);
        File.WriteAllText(packageJsonPath, newJson);
        AssetDatabase.Refresh();

        Debug.Log("seach id: " + packageName);
        var info = await UpmGitUtil.GetEmbededPackageInfo(packageName);
        Debug.Log("info updated");
        this.nativePackageInfo = info;
        onComplete?.Invoke();
    }


    public bool? isReleasedOnOpenUpm;

    public async void GetIsReleasedOnOpenUpmInBackground()
    {
        var packageName = this.nativePackageInfo.name;
        var b = await UpmGitUtil.GetIsPackageExistsOnOpenUpmAsync(packageName);
        this.isReleasedOnOpenUpm = b;
    }

}
