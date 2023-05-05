using System;
using UnityEngine;

public class HeroBookBasePane : MonoBehaviour
{
    protected HeroInfo _heroInfo;
    protected bool _heroChanged;

    private void OnEnable()
    {
        if (null == _heroInfo) return;
        
        _InternalRefreshHero();
    }

    public void SetHero(HeroInfo heroInfo)
    {
        _heroChanged = _heroInfo != heroInfo;
        
        if (_heroChanged)
        {
            _heroInfo = heroInfo;
        }

        if (gameObject.activeSelf)
        {
            _InternalRefreshHero();
        }
    }

    protected virtual void _InternalRefreshHero()
    {
    }
}