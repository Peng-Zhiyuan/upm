using SpineRegulate;
using UnityEngine;

/// <summary>
/// 模板数据  只需要调整半身像大小  外部调用需要调整UISpineUnit缩放位移等
/// </summary>
public class SpineRegulateTemplateScriptObject : ScriptableObject
{
    public ESpineTemplateType templateType;
    public string spineName;
    public string spineFullName; // 具体路径
    public float offsetX;
    public float offsetY;
    public float scale;
    public bool openMask;
    public Vector2Int softness;
    public Vector2 maskSize;
}