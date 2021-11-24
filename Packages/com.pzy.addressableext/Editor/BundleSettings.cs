using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu]
public class BundleSettings : SerializedScriptableObject
{
    public bool BuildLocalBundle = false;

    public bool autoProcessNewAssets = true;

    public List<string> IgnoreFileTypes = new List<string>();

    
    /// <summary>
    /// 指定的后缀会被强制划分到指定 group，而无视文件夹标记
    /// </summary>
    public Dictionary<string, string> forceExtensionToGroup = new Dictionary<string, string>();

    /// <summary>
    /// 指定的后缀，如果没有被指定划分到某个 group， 则会被划分到这里指定的 group
    /// </summary>
    public Dictionary<string, Suggest> sergestExtensionToGroup = new Dictionary<string, Suggest>();
}

public class Suggest
{
    public string forceToGroup;
    public List<string> exludeFromGroupList;
}