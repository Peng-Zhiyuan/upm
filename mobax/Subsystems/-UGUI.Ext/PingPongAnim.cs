using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingPongAnim : MonoBehaviour
{
   
    //public float maxScale = 1.2f;
    public float strength = 1f;
    public float  duration = 1;
    public float delay = 0;
    private float time = 0;
    public Vector3 direction = Vector3.up;
    private Vector3 origin_pos = Vector3.zero;
     void Awake() 
     {
       origin_pos =   this.transform.localPosition ;
     }

    private void Update()
    {
        this.time += Time.deltaTime / Time.timeScale;;
        this.transform.localPosition = origin_pos + direction *  strength * Mathf.Sin(delay+2 * Mathf.PI * this.time/duration);

    }
}
