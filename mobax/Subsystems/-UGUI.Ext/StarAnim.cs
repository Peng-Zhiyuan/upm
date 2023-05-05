using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class StarAnim : MonoBehaviour
{
    private float time;
    public Image image;
    public float duration = 1;
    public float spaceTime = 2;
    public float max_alpha = 1;

    public float min_alpha = 0;

    public float delay = 0;


    public float max_scale = 1;
    public float min_scale = 0;
    private Color c = new Color(1, 1, 1, 1);
    public float origin_scale = 1;
    private float delayTime = 0;
    private bool playing = true;
    void Awake()
    {
        origin_scale = this.transform.localScale.x;
        this.time = 0;
        delayTime = delay;
    }
    void OnEnable() {
        delayTime = delay;
        this.image.color = new Color(1, 1, 1, 0); 
        playing = true; 
    }
    void Update()
    {
        if(delayTime > 0) 
        {
            delayTime -= Time.deltaTime;
            return;
        }
        this.time += Time.deltaTime;
        if(playing)
        {
            if(this.time > duration)//T = 2 * duration
            {
               playing = false;
               this.image.color = new Color(1, 1, 1, 0); 
               this.time = 0;
               return;
            }
            float v = (Mathf.Sin(Mathf.PI * this.time/duration) +1)/2;
            c.a = min_alpha + (max_alpha - min_alpha) * v;
            float scale = (min_scale + (max_scale - min_scale) * v)* origin_scale;
            this.transform.localScale = Vector3.one * scale;
            this.image.color = c;
        }
        else  
        {
            if(this.time >= spaceTime)
            {
                playing = true;
                this.image.color = new Color(1, 1, 1, 0); 
                this.time = 0;
            }
            
        }
       
    }
}
