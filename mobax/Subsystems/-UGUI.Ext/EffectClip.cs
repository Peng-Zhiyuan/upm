using System.Collections.Generic;
using UnityEngine;
public class EffectClip : MonoBehaviour
{
    // 遮挡容器，即ScrollView
    [SerializeField] RectTransform m_rectTrans;

    // 存放需要修改Shader的Material
    List<Material> m_materialList = new List<Material>();

    // UI的根,Canvas
    Transform m_canvas;
    float m_halfWidth, m_halfHeight, m_canvasScale;

    private void Awake() {
        //Const.effectClips.Add(this);
    }
    public void SetRender (RectTransform rtf = null)
    {
        if(rtf != null){
            m_rectTrans = rtf;
        }
        m_canvas = GameObject.Find("UIEngine.Canvas").transform;

        // 获取所有需要修改shader的material，并替换shader
        var particleSystems = GetComponentsInChildren<ParticleSystem>();
        for(int i = 0, j = particleSystems.Length; i < j ; i++)
        {
            var ps = particleSystems[i];
            var mat = ps.GetComponent<Renderer>().material;
            m_materialList.Add(mat);
            // mat.shader = Shader.Find(mat.shader.name + " 1");
        }

        var renders = GetComponentsInChildren<MeshRenderer>();
        for(int i = 0, j = renders.Length; i < j; i++)
        {
            var ps = renders[i];
            var mat = ps.material;
            m_materialList.Add(mat);
            //mat.shader = Shader.Find(mat.shader.name + " 1");
        }

        // 获取UI的scale，容器的宽高的一半的值
        m_canvasScale = m_canvas.localScale.x;
        m_halfWidth = m_rectTrans.sizeDelta.x * 0.5f * m_canvasScale;
        m_halfHeight = m_rectTrans.sizeDelta.y * 0.5f * m_canvasScale;

        // 给shader的容器坐标变量_Area赋值
        Vector4 area = CalculateArea(m_rectTrans.position);
        for(int i = 0, len = m_materialList.Count; i < len; i++)
        {
            m_materialList[i].SetVector("_Area", area);
        }
    }

    // 计算容器在世界坐标的Vector4，xz为左右边界的值，yw为下上边界值
    Vector4 CalculateArea(Vector3 position)
    {
        return new Vector4()
        {
            x = position.x - m_halfWidth,
            y = position.y - m_halfHeight,
            z = position.x + m_halfWidth,
            w = position.y + m_halfHeight
        };
    }
}
