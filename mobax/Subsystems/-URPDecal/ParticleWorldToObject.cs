using System.Collections;
using UnityEngine;


[ExecuteAlways]
public class ParticleWorldToObject : MonoBehaviour
{
    ParticleSystem Particle;
    public Material Mat;
    private void Awake()
    {
        if (Particle != null) return;
        Particle = gameObject.GetComponent<ParticleSystem>();
        var render = gameObject.GetComponent<ParticleSystemRenderer>();
        Mat = render.sharedMaterial;
    }
       
    private void Update()
    {
        var worldToObject = transform.worldToLocalMatrix;
        var particleScaleModule = Particle.sizeOverLifetime;
        var scale = new Vector3(1, 1, 1);
            
        if (particleScaleModule.enabled)
        {
            var p = Particle.time / Particle.main.duration;
            var scaleCurveX = particleScaleModule.x;
            var scaleCurveY = particleScaleModule.y;
            var scaleCurveZ = particleScaleModule.z;
            scale.x = Mathf.Max(0.01f,scaleCurveX.curve.Evaluate(p));
            scale.y = Mathf.Max(0.01f, scaleCurveY.curve.Evaluate(p));
            scale.z = Mathf.Max(0.01f, scaleCurveZ.curve.Evaluate(p));
        }
            
        var scaleMatirx = Matrix4x4.Scale(scale).inverse;
        Mat.SetMatrix("_WORLD2OBJECT", scaleMatirx * worldToObject);
        //Mat.EnableKeyword("_SupportParticleSystem");
    }
}
