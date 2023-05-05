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
    public MaxSize maxSize;
    public List<FormatSetterFiter> filterList;
}

[Serializable]
public class FormatSetterFiter
{
    public string keyWord;
    public TextureImporterFormat format;
    public MaxSize maxSize = MaxSize.NotSet;
}

public enum MaxSize
{
    NotSet = 0,
    n256 = 256,
    n512 = 512,
    n1024 = 1024,
    n2048 = 2048,
}
