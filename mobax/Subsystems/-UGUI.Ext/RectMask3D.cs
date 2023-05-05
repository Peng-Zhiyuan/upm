using UnityEngine;
using UnityEngine.UI;
public class RectMask3D : RectMask2D
{
    GameObject m_Mask3D = null;
    [SerializeField] private Material m_Material;
    [SerializeField] private int m_ID = 1;
 
    public int id
    {
        get { return m_ID; }
        set
        {
            if (value != m_ID)
            {
                m_ID = value;
                Refresh();
            }
        }
    }
 
#if UNITY_EDITOR
    protected void OnValidate()
    {
        if (Application.isPlaying)
        {
            Refresh();
        }
    }
#endif
    protected override void Awake()
    {
        base.Awake();
        if (Application.isPlaying)
        {
            Refresh();
        }
    }
 
    private void Refresh()
    {
        if (m_Mask3D == null)
        {
            m_Mask3D = GameObject.CreatePrimitive(PrimitiveType.Quad);
            m_Mask3D.layer = LayerMask.NameToLayer("UI");
            m_Mask3D.name = "Mask3D";
            m_Mask3D.hideFlags = HideFlags.NotEditable;
            m_Mask3D.GetComponent<MeshRenderer>().material = m_Material;
            m_Mask3D.transform.SetParent(transform);
        }
        m_Mask3D.transform.localPosition = Vector3.zero;
        m_Mask3D.transform.localScale = this.rectTransform.sizeDelta;
 
        var material = m_Mask3D.GetComponent<Renderer>().material;
        material.SetInt("_ID", m_ID);
    }
 
}
#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(RectMask3D))]
class SuperMask2DInspector : UnityEditor.Editor
{
 
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
#endif
