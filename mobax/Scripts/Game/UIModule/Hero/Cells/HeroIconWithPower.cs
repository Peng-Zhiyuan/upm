using UnityEngine;
using UnityEngine.UI;

public class HeroIconWithPower : MonoBehaviour
{
    public HeroIconView heroIcon;
    public Text powerText;

    public void SetInfo(HeroInfo heroInfo)
    {
        var itemData = new ItemViewData();
        itemData.SetAsItemInfo(heroInfo.ItemInfo);
        heroIcon.Bind(itemData);
        powerText.text = $"{heroInfo.Power}";
    }
}