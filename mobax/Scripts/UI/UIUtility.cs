using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
public static class UIUtility
{
    
    public static IEnumerator MaskFade(Image maskImg,float b, float e, float t)
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
