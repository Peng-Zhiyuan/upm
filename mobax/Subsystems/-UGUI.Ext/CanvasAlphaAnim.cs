using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class CanvasAlphaAnim : MonoBehaviour
{
    private float time;
    public CanvasGroup canvas;
    public float duration = 1;
    public float delay = 0;
    public float max_alpha = 1;

     public float min_alpha = 0;

    private Color c = new Color(1, 1, 1, 1);

    void OnEnable()
    {
        if(this.canvas == null) this.canvas = this.GetComponent<CanvasGroup>();
        this.time = 0;
    }
    
    void Update()
    {
        this.time += Time.deltaTime;
        this.canvas.alpha = (min_alpha + (max_alpha - min_alpha) * (Mathf.Sin(delay+2 * Mathf.PI * this.time/duration) +1)/2);
    }
}
