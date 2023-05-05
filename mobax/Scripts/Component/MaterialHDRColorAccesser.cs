using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class MaterialHDRColorAccesser : MonoBehaviour
{
    public Renderer Renderer;
    public Image Image;
    public string Keyword = "";
    [ColorUsage(true, true)]
    public Color ColrVal = Color.black;
    Color LastVal = Color.black;

    private void LateUpdate()
    {
        if (Image == null && Renderer == null)
            return;
        var m = Image != null ? Image.material: Renderer.material;
        if (string.IsNullOrEmpty(Keyword)||
            !m.HasProperty (Keyword))
            return;
        var val = m.GetColor(Keyword);
        if (LastVal != ColrVal) {
            LastVal = val;
            m.SetColor(Keyword, ColrVal);
        }
    }
}
