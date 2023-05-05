using System;
using UnityEngine;

public partial class HeroBookHeroCell : MonoBehaviour
{
    private HeroInfo _heroInfo;

    private void Awake()
    {
        Img_hero.alphaHitTestMinimumThreshold = 0.1f;
    }

    public void Display(int heroId, string card, int scale)
    {
        _heroInfo = HeroManager.Instance.GetHeroInfo(heroId);
        
        UiUtil.SetSpriteInBackground(Img_hero, () => $"{card}.png", _ =>
            {
                Img_hero.SetNativeSize();
                Img_hero.SetLocalScale(Vector3.one);// * scale / 100f);

                if (_heroInfo.Unlocked)
                {
                    Img_hero.color = Color.white;
                }
                else
                {
                    Img_hero.color = Color.gray;
                }
            });
    }

    public void OnClick()
    {
        if (!_heroInfo.Unlocked)
        {
            ToastManager.ShowLocalize("M4_hero_unlocked");
            return;
        }
        
        UIEngine.Stuff.ForwardOrBackTo<HeroBookPage>(_heroInfo);
    }
}