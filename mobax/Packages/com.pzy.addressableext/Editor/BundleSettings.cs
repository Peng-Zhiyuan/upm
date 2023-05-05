using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;



[CreateAssetMenu]
public class BundleSettings : SerializedScriptableObject
{
    //public bool BuildLocalBundle = false;

    public bool autoProcessNewAssets = true;

    public List<string> IgnoreFileTypes = new List<string>();

    public List<string> package = new List<string>();

    public List<string> separatePackGroup = new List<string>();

    static List<string> defaultIgnoreExtension = new List<string>() { ".cs", ".meta", ".asmdef" };

    public bool IsIgnoreExtension(string extension)
    {
        if(string.IsNullOrEmpty(extension))
        {
            return true;
        }

        if(defaultIgnoreExtension.Contains(extension))
        {
            return true;
        }

        if (IgnoreFileTypes.Contains(extension))
        {
            return true;
        }
        return false;
    }

    public bool IsPackageIncludingPath(string path)
    {
        if(!path.StartsWith("Packages/"))
        {
            return false;
        }
        var trimedHeadPackage = path.Substring("Packages/".Length);
        foreach(var one in this.package)
        {
            if(trimedHeadPackage.StartsWith(one))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsSeparatePackGroup(string groupName)
    {
        if(this.separatePackGroup.Contains(groupName))
        {
            return true;
        }
        return false;
    }
}

