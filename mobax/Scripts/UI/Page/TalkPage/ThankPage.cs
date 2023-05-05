using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
public class ThankPage : MonoBehaviour
{
    public Image maskImg;
    public float maskFadeTime = 0.3f;
    public float blackPauseTime = 1.0f;
    
    public Transform root;

    public float yBegin;
    public float yEnd;
    public Text tiper;
    
    IEnumerator Start()
    {
        root.gameObject.SetActive(false);
        StartCoroutine(MaskFade(0f, 1f, maskFadeTime));
        yield return new WaitForSeconds(maskFadeTime+blackPauseTime);
        root.gameObject.SetActive(true);
        StartCoroutine(MaskFade(1f, 0f, maskFadeTime));
        tiper.transform.DOLocalMoveY(yEnd, 0.3f);
    }
    IEnumerator MaskFade(float b, float e, float t)
    {
        maskImg.color = new Color(0f, 0f, 0f, b);

        var offset = 0.0f;

        if (offset > 0.0f)
        {
            yield return new WaitForSeconds(offset * t);
        }
        var rt = (1-offset)* t;
        var c = 0.0f;
        while (c<rt)
        {
            c += Time.deltaTime;
            if (c >= rt)
            {
                maskImg.color = new Color(0f, 0f, 0f, e);
            }
            else
            {
                maskImg.color = new Color(0f, 0f, 0f,    Mathf.LinearToGammaSpace(b+(c/rt)*(e-b)));
            }
            yield return null;
        }
    }
}
