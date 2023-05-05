using System;
using BattleEngine.Logic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// 星星上的点点
/// </summary>
public partial class HeroStarSpot : MonoBehaviour
{
    public Image progress;
    public Image Image_selected;
    public MutexComponent core;
    public Action<HeroStarSpot> onClick;
    public Action<HeroStarSpot> onSelected;

    public bool Selected
    {
        set
        {
            Image_selected.gameObject.SetActive(value);
            if (value)
            {
                onSelected?.Invoke(this);
            }
        }
    }
    
    public void SetReached(bool reached)
    {
        progress.fillAmount = reached ? 1 : 0;
        if (null != core)
        {
            core.Selected = reached;
        }
    }

    public void EaseReached()
    {
        progress.DOFillAmount(1, .2f).OnComplete(() =>
        {
            if (null != core)
            {
                core.Selected = true;
                core.transform.DOScale(Vector3.one * 1.6f, .1f).OnComplete(() =>
                {
                    core.transform.DOScale(Vector3.one, .1f);
                });
            }
        });
    }

    public void OnClick()
    {
        onClick?.Invoke(this);
    }
}