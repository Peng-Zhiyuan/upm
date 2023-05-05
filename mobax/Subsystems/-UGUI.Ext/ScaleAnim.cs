using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleAnim : MonoBehaviour
{

    //public float maxScale = 1.2f;
    public float strength = 0.1f;
    public float  duration = 1;
    public float delay = 0;
    private float time = 0;
    private Vector3 origin_scale = Vector3.one;
    private void Awake() {
       origin_scale = this.transform.localScale;
    }

    private void Update()
    {
        this.time += Time.deltaTime / Time.timeScale;
        this.transform.localScale = origin_scale * (1+ strength * Mathf.Sin(delay+2 * Mathf.PI * this.time/duration));
    }
}
