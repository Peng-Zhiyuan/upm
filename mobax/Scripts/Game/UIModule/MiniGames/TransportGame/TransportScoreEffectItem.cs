using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public partial class TransportScoreEffectItem: MonoBehaviour
{
    public Action<TransportScoreEffectItem> onEffectEnd;
    
    private static readonly float EffectDuration = 1f;

    public void SetScore(int score)
    {
        var scoreTxt = transform.Find("Txt_score").GetComponent<Text>();
        if (score > 0)
        {
            scoreTxt.color = new Color((float) 0x6F / 0xFF, (float) 0xFF / 0xFF, (float) 0x76 / 0xFF);
            scoreTxt.SetLocalizer("M4_minigame2_scored1", $"{score}");
        }
        else
        {
            scoreTxt.color = new Color((float) 0xFF / 0xFF, (float) 0x60 / 0xFF, (float) 0x60 / 0xFF);
            scoreTxt.SetLocalizer("M4_minigame2_scored2", $"{score}");
        }
        
        UiUtil.SetAlpha(scoreTxt, 1);
        scoreTxt.SetLocalPositionY(0);
        scoreTxt.DOFade(.2f, EffectDuration);
        scoreTxt.rectTransform.DOLocalMoveY(scoreTxt.GetLocalPositionY() + 100, EffectDuration).OnComplete(() =>
        {
            onEffectEnd?.Invoke(this);
        });
    }
}
