using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ReserchManager : StuffObject<ReserchManager>
{
    [ShowInInspector]
    public async void ShowReserchIfNeed(bool force = false)
    {
        if(!force)
        {
            var alreadyShown = this.IsShownInLocalSetting;
            if (alreadyShown)
            {
                return;
            }

            var isLevelMeet = this.IsLevelCondittionMeet;
            if (!isLevelMeet)
            {
                return;
            }

            if(notAutoShowInThisGamePlay)
            {
                return;
            }

            var b = await Dialog.AskAsync("", "m1_survey_comfirmdes".Localize()) ;
            if(!b)
            {
                notAutoShowInThisGamePlay = true;
                return;
            }
        }

        var url = this.Url;
        if(!string.IsNullOrEmpty(url))
        {
            Application.OpenURL(url);
            IsShownInLocalSetting = true;
            var ret = await ActiveApi.QuestionAsync();
            Database.Stuff.activeDatabase.Add(ret);
        }
    }

    [ShowInInspector, ReadOnly]
    bool notAutoShowInThisGamePlay;

    [ShowInInspector]
    string Url
    {
        get
        {
            var lan = LocalizationManager.Stuff.Language;
            var row = StaticData.SurveyTable.TryGet(lan);
            return row?.Url;
        }
    }

    [ShowInInspector]
    bool IsLevelCondittionMeet
    {
        get
        {
            return Database.Stuff.roleDatabase.Me.lv >= 15;
        }

    }

    [ShowInInspector]
    bool IsShownInLocalSetting
    {
        get
        {
            var key = "reserch";
            return PlayerPrefs.GetInt(key, 0) == 1;
        }
        set
        {
            var key = "reserch";
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }
    }
}
