using UnityEngine;

/// <summary>
/// 指定调整 外部不需要调整UISpineUnit任何操作  全靠内部自己实现
/// </summary>
public class SpineRegulateAppointScriptObject : ScriptableObject
{
    public string pageName;
    public string spineName;
    public string spineFullName;  // 具体路径
    public float offsetX;
    public float offsetY;
    public float scale;
    public bool openMask;
    public Vector2Int softness;
    public Vector2 maskSize;
}