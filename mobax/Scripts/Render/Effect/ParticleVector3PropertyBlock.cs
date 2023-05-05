using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleVector3PropertyBlock : MonoBehaviour
{
    void Start()
    {
        var objects = FindObjectsOfType<ParticleSystem>();

        MaterialPropertyBlock props = new MaterialPropertyBlock();
        ParticleSystemRenderer renderer;

        foreach (ParticleSystem particle in objects) {
            float r = Random.Range(0.0f, 100f);
            float g = Random.Range(0.0f, 1.0f);
            float b = Random.Range(0.0f, 1.0f);
            props.SetColor("_Position", new Color(r, g, b));
            renderer = particle.gameObject.GetComponent<ParticleSystemRenderer>();
            renderer.SetPropertyBlock(props);
        }
    }
}
