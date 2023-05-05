using System;
using UnityEngine;
using UnityEngine.UI;

public class PageTurner : MonoBehaviour
{
    public GameObject leftButton;
    public GameObject rightButton;

    public Action OnTurnPage;

    private BasePager _pager;

    public void SetPager(BasePager pager)
    {
        _pager = pager;
    }

    public void TurnPage(int offset)
    {
        if (_pager.TurnOffset(offset))
        {
            RefreshPagerView();

            if (null != OnTurnPage)
            {
                OnTurnPage();
            }
        }
    }

    public void RefreshPagerView()
    {
        leftButton.SetActive(_pager.CurrentPage > 1);
        rightButton.SetActive(_pager.CurrentPage < _pager.TotalPage);
    }
}