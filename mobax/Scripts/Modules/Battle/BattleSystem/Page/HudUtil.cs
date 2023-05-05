using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public static class HudUtil
{
    /*DOTween.To(()=>变量,x=> 变量=x , 变量目标值, 过渡时间).OnComplete(()=>{ 
    });;*/
    public static void DoHpFade(float percent, Image fadeImage, float time, string unique = "")
    {
        DOTween.Kill($"Hp_fade_{unique}");
        
        var tween = DOTween.To(() => fadeImage.fillAmount, x => { fadeImage.fillAmount = x; },percent, time).OnComplete(() => { /*cadata.colorAdjustments.openColorAdjustments = false;*/ });
        tween.SetId($"Hp_fade_{unique}");
    }
    
    /*DOTween.To(()=>变量,x=> 变量=x , 变量目标值, 过渡时间).OnComplete(()=>{ 
    });;*/
    public static void DoHpFade(Creature data, Image fadeImage, float time, string unique = "")
    {
        DOTween.Kill($"Hp_fade_{data.mData.Id}{unique}");
        
        var tween = DOTween.To(() => fadeImage.fillAmount, x => { fadeImage.fillAmount = x; },data.mData.CurrentHealth.Percent(), time).OnComplete(() => { /*cadata.colorAdjustments.openColorAdjustments = false;*/ });
        tween.SetId($"Hp_fade_{data.mData.Id}{unique}");
    }

    public static void DoFade(string tweenId, Image fadeImage, float oldV, float newV, float durT, Action callback)
    {
        DOTween.Kill(tweenId);
        if(Mathf.Abs(newV - oldV) < 0.01f)
            return;
        
        fadeImage.fillAmount = oldV;
        var tween = DOTween.To(() => fadeImage.fillAmount, x => { fadeImage.fillAmount = x; },newV, durT).OnComplete(() => { if(callback != null) callback.Invoke();});
        tween.SetId(tweenId);
    }
}