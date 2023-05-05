using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class AlphaAnim : MonoBehaviour
{
    private float time;
    public MaskableGraphic grahic;
    public float duration = 1;
    public float delay = 0;
    public float max_alpha = 1;

     public float min_alpha = 0;

     public bool loop = false;

    private Color c = new Color(1, 1, 1, 1);

    void OnEnable()
    {
        if(this.grahic == null) this.grahic = this.GetComponent<Image>();
        this.time = 0;
    }
    void Update()
    {
        this.time += Time.deltaTime/Time.timeScale;
        c = this.grahic.color; 
        c.a = (min_alpha + (max_alpha - min_alpha) * (Mathf.Sin(delay+2 * Mathf.PI * this.time/duration) +1)/2);
        if(loop&&c.a == min_alpha){
            c.a = max_alpha;
        }
        this.grahic.color = c;
    }
}
