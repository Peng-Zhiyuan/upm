using System;
using UnityEngine;

public partial class HeroListItem : MonoBehaviour
{
    public HeroInfo HeroInfo => Cell_card.HeroInfo;

    public void SetInfo(HeroInfo heroInfo, Action<HeroInfo> callback)
    {
        Cell_card.SetInfo(heroInfo);
        Cell_card.onClick = callback;
        
        // 红点绑定
        Reminder.Bind($"{HeroReminderConst.Hero_heroPrefix}{heroInfo.HeroId}", Root);
    }
}