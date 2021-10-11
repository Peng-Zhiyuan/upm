using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class BundleSettings : ScriptableObject
{
    public bool BuildLocalBundle = false;

    public bool autoProcessNewAssets = true;

    public List<string> IgnoreFileTypes = new List<string>();
}
