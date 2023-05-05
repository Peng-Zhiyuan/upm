using UnityEngine;

public partial class HeroBookNameCell : MonoBehaviour
{
    public void SetHeroId(int heroId)
    {
        var hero = HeroManager.Instance.GetHeroInfo(heroId);
        Txt_name.text = hero.Name.Localize();
    }
}