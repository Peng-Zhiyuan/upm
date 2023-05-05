using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class MaterialFloatAccesser : MonoBehaviour
{
    public ParticleSystem Particle;
    public Renderer Renderer;
    public Image Image;
    public string Keyword = "";
    public float FloatVal = 0;
    float LastVal = 0;

    private void LateUpdate()
    {
        if (Image == null && Renderer == null && Particle == null)
            return;
        Material m = null;
        if (Application.isPlaying) {
            if (Image != null)
                m = Image.material;
            if (Renderer != null)
                m = Renderer.material;
            if (Particle != null)
                m = Particle.GetComponent<ParticleSystemRenderer>().material;
        } else {
            if (Image != null)
                m = Image.material;
            if (Renderer != null)
                m = Renderer.sharedMaterial;
            if (Particle != null)
                m = Particle.GetComponent<ParticleSystemRenderer>().sharedMaterial;
        }
        if (m == null)
            return;

        if (string.IsNullOrEmpty(Keyword)||
            !m.HasProperty (Keyword))
            return;
        var val = m.GetFloat(Keyword);
        if (LastVal != FloatVal) {
            LastVal = val;
            m.SetFloat(Keyword, FloatVal);
        }
    }
}
