using System;
using UnityEngine;
using UnityEngine.UI;

public class PagerView : MonoBehaviour
{
    public GameObject leftButton;
    public GameObject rightButton;
    public InputField pageTxt;
    public Text totalTxt;

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

    public void RefreshPage()
    {
        if (!int.TryParse(pageTxt.text, out var pageNum)) return;
        var currentPage = _pager.CurrentPage;
        if (currentPage == pageNum) return;

        _pager.TurnTo(pageNum);
        // 如果有事实翻页，则翻页
        if (currentPage != _pager.CurrentPage)
        {
            if (null != OnTurnPage)
            {
                OnTurnPage();
            }
        }
        RefreshPagerView();
    }

    public void RefreshPagerView()
    {
        pageTxt.SetTextWithoutNotify($"{_pager.CurrentPage}");
        totalTxt.text = $"{_pager.TotalPage}";
    }
}