using System;
using System.Collections.Generic;
using System.Linq;
using BattleEngine.Logic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// 星星串
/// </summary>
public partial class HeroStarCluster : MonoBehaviour
{
    public HeroStarSpot bigStar;
    public HeroStarSpot smallStar;
    public RectTransform highlightBar;
    public RectTransform bgBar;
    public int top;
    public int bottom;
    public int spacing;
    public Action<int> onSelect;
    public Action<HeroStarSpot> onHighlight;

    private List<HeroStarSpot> _bigStars;
    private List<HeroStarSpot> _smallStars;
    private int _bigStarLength;
    private int _smallStarLength;
    private int _bigNum;
    private int _smallNum;
    private HeroInfo _heroInfo;
    private List<int> _spots;
    private Dictionary<int, int> _spotMap;
    private Dictionary<int, HeroStarSpot> _starMap;
    private Dictionary<HeroStarSpot, int> _starIdMap;
    private HeroStarSpot _currentSelect;

    public void SetInfo(HeroInfo heroInfo)
    {
        _bigNum = 0;
        _smallNum = 0;
        _bigStars ??= new List<HeroStarSpot>();
        _smallStars ??= new List<HeroStarSpot>();
        _spots ??= new List<int>();
        _spotMap ??= new Dictionary<int, int>();
        _starMap ??= new Dictionary<int, HeroStarSpot>();
        _starIdMap ??= new Dictionary<HeroStarSpot, int>();
        _heroInfo = heroInfo;
        // 清掉数组
        if (_spots.Count > 0) _spots.Clear();

        var cfg = _heroInfo.FirstStarConfig;
        var initialized = false;
        var notReached = false;
        int totalLength = bottom;
        do
        {
            if (!initialized)
            {
                initialized = true;
            }
            else
            {
                totalLength += spacing;
            }

            HeroStarSpot starSpot;
            int starLen;
            if (cfg.Starlevel == 0)
            {
                starSpot = _GetBigStar();
                starLen = _bigStarLength;
            }
            else
            {
                starSpot = _GetSmallStar();
                starLen = _smallStarLength;
            }
            starSpot.SetReached(!notReached);
            starSpot.onClick = _OnStarClick;
            starSpot.onSelected = _OnStarSelected;
            // 放置星星的位置
            int spotY = totalLength + starLen / 2;
            starSpot.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, spotY);
            _starMap[cfg.Id] = starSpot;
            _spotMap[cfg.Id] = _spots.Count;
            _starIdMap[starSpot] = cfg.Id;
            _spots.Add(spotY);
            // 加上星星长度
            totalLength += starLen;

            if (!notReached)
            {
                if (cfg == heroInfo.StarConfig)
                {
                    notReached = true;
                }
            }
            cfg = StaticData.HeroStarTable.TryGet(cfg.Next);
        } while (null != cfg);

        totalLength += top;
        GetComponent<RectTransform>().sizeDelta = new Vector2(0, totalLength);
        var firstSpot = _spots.First();
        var lastSpot = _spots.Last();
        bgBar.anchoredPosition = new Vector2(0, firstSpot);
        bgBar.sizeDelta = new Vector2(bgBar.sizeDelta.x, lastSpot - firstSpot);
        var currentSpot = _spots[_spotMap[heroInfo.StarId]];
        highlightBar.anchoredPosition = new Vector2(0, firstSpot);
        highlightBar.sizeDelta = new Vector2(highlightBar.sizeDelta.x, currentSpot - firstSpot);
        _Select(_starMap[heroInfo.StarId]);
        
        // 没用到的都要关掉
        for (var i = _bigNum; i < _bigStars.Count; ++i)
        {
            _bigStars[i].gameObject.SetActive(false);
        }
        for (var i = _smallNum; i < _smallStars.Count; ++i)
        {
            _smallStars[i].gameObject.SetActive(false);
        }
    }

    public void HighlightStar(int starId)
    {
        var currentIndex = _spotMap[starId];
        var currentSpot = _spots[currentIndex];
        var sizeDelta = new Vector2(highlightBar.sizeDelta.x, currentSpot - _spots.First());
        highlightBar.DOSizeDelta(sizeDelta, .2f).OnComplete(() =>
        {
            var starSpot = _starMap[starId];
            starSpot.EaseReached();
            onHighlight?.Invoke(starSpot);
        });
        
        // 选中当前
        _Select(_starMap[starId]);
    }

    public int GetStarY(int starId)
    {
        return _spots[_spotMap[starId]];
    }

    private void Awake()
    {
        _bigStarLength = (int) bigStar.GetComponent<RectTransform>().sizeDelta.x;
        _smallStarLength = (int) smallStar.GetComponent<RectTransform>().sizeDelta.x;
    }

    private HeroStarSpot _GetBigStar()
    {
        HeroStarSpot starItem;
        if (_bigNum < _bigStars.Count)
        {
            starItem = _bigStars[_bigNum];
            starItem.gameObject.SetActive(true);
        }
        else
        {
            starItem = Instantiate(bigStar, transform);
            _bigStars.Add(starItem);
        }
        ++_bigNum;

        return starItem;
    }

    private HeroStarSpot _GetSmallStar()
    {
        HeroStarSpot starItem;
        if (_smallNum < _smallStars.Count)
        {
            starItem = _smallStars[_smallNum];
            starItem.gameObject.SetActive(true);
        }
        else
        {
            starItem = Instantiate(smallStar, transform);
            _smallStars.Add(starItem);
        }
        ++_smallNum;

        return starItem;
    }

    private void _OnStarClick(HeroStarSpot spot)
    {
        _Select(spot);
    }

    private void _OnStarSelected(HeroStarSpot spot)
    {
        var starId = _starIdMap[spot];
        onSelect?.Invoke(starId);
    }

    private void _Select(HeroStarSpot spot)
    {
        if (_currentSelect == spot) return;

        if (null != _currentSelect)
        {
            _currentSelect.Selected = false;
        }

        if (null != spot)
        {
            spot.Selected = true;
        }

        _currentSelect = spot;
    }
}