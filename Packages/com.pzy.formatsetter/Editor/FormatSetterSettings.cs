using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.Serialization;
using System;

[CreateAssetMenu(menuName = "Format Setter Settings")]
public class FormatSetterSettings : ScriptableObject
{
    public bool enableProcessTexture;
    public bool enableProcessAudioClip;
    public TextureImporterFormat defaultFormat;
    public List<FormatSetterFiter> filterList;
}

[Serializable]
public class FormatSetterFiter
{
    public string name;
    public string keyWord;
    public TextureImporterFormat format;
}
