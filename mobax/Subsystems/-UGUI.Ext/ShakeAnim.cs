using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using DG.Tweening;
public class ShakeAnim : MonoBehaviour {

    private float shakeTime = -1;
    private Vector2 originPos = Vector2.zero;
    private float duration = 1;
    private float strong = 1;
    private int Frequency = 4;
    void Awake () {
        this.originPos = this.transform.localPosition;
    }

    public void Shake(float duration = 1, float strong = 1, int frequency = 4)
    {
        this.duration = duration;
        this.shakeTime = 0;
        this.strong = strong;
        this.Frequency = frequency;

    }

    public void StartShake(int strong = 1, int frequency = 4)
    {
        this.duration = 0;
        this.shakeTime = 0;
        this.strong = strong;
        this.Frequency = frequency;
    }

    public void EndShake()
    {
        this.shakeTime = -1;
        this.transform.localPosition = this.originPos;
    }

    void update ()
    {
        if(this.shakeTime >=0)
        {
            this.shakeTime += Time.deltaTime;
            if(this.duration > 0 && this.shakeTime > this.duration)
            {
                this.shakeTime = -1;
                this.transform.localPosition = this.originPos;
                return;
            }
            this.transform.localPosition = this.originPos + new Vector2(this.strong * Mathf.Sin(this.Frequency * Mathf.PI * this.shakeTime),this.strong*Mathf.Cos(this.Frequency * Mathf.PI * this.shakeTime));
            return;
        }
    }
}
