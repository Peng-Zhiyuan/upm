using System.Collections.Generic;
using FancyScrollView;
using UnityEngine;

public class HeroBookGroupCell : FancyCell<HeroBookGroupData, HeroBookGroupContext>
{
    public HeroBookHeroCell heroPrefab;
    public HeroBookNameCell namePrefab;
    public Transform heroesNode;
    public Transform namesNode;
    public Animator animator;

    private Queue<HeroBookHeroCell> _heroPool = new Queue<HeroBookHeroCell>();
    private Queue<HeroBookNameCell> _namePool = new Queue<HeroBookNameCell>();
    private List<HeroBookHeroCell> _heroDisplayList;
    private List<HeroBookNameCell> _nameDisplayList;
    private float _currentPosition = 0;
    
    static class AnimatorHash
    {
        public static readonly int Scroll = Animator.StringToHash("scroll");
    }
    
    void OnEnable() => UpdatePosition(_currentPosition);
    
    public void SetList(List<LiblistRow> list)
    {
        if (null == _heroDisplayList)
        {
            _heroDisplayList = new List<HeroBookHeroCell>(list.Count);
        }
        else
        {
            foreach (var heroItem in _heroDisplayList)
            {
                _PutHero(heroItem);
            }

            _heroDisplayList.Clear();
        }
        
        if (null == _nameDisplayList)
        {
            _nameDisplayList = new List<HeroBookNameCell>(list.Count);
        }
        else
        {
            foreach (var nameItem in _nameDisplayList)
            {
                _PutName(nameItem);
            }

            _nameDisplayList.Clear();
        }
        
        foreach (var hero in list)
        {
            // 英雄部分
            var heroItem = _TakeHero();
            _heroDisplayList.Add(heroItem);
            var heroPos = hero.cardPos;
            heroItem.rectTransform().anchoredPosition = new Vector2(heroPos.X, heroPos.Y);
            heroItem.Display(hero.Roleid, hero.Card, hero.Scale);
            // 名字部分
            var nameItem = _TakeName();
            _nameDisplayList.Add(nameItem);
            var namePos = hero.namePos;
            nameItem.rectTransform().anchoredPosition = new Vector2(namePos.X, namePos.Y);
            nameItem.SetHeroId(hero.Roleid);
        }
    }
    
    public override void UpdateContent(HeroBookGroupData groupData)
    {
        SetList(groupData.HeroList);
    }


    public override void UpdatePosition(float position)
    {
        _currentPosition = position;

        if (animator.isActiveAndEnabled)
        {
            animator.Play(AnimatorHash.Scroll, -1, position);
        }

        animator.speed = 0;
    }

    private HeroBookHeroCell _TakeHero()
    {
        var hero = _heroPool.Count > 0 ? _heroPool.Dequeue() : Instantiate(heroPrefab, heroesNode);
        hero.SetActive(true);
        return hero;
    }

    private void _PutHero(HeroBookHeroCell hero)
    {
        hero.SetActive(false);
        _heroPool.Enqueue(hero);
    }

    private HeroBookNameCell _TakeName()
    {
        var nameInstance = _namePool.Count > 0 ? _namePool.Dequeue() : Instantiate(namePrefab, namesNode);
        nameInstance.SetActive(true);
        return nameInstance;
    }

    private void _PutName(HeroBookNameCell nameInstance)
    {
        nameInstance.SetActive(false);
        _namePool.Enqueue(nameInstance);
    }
}