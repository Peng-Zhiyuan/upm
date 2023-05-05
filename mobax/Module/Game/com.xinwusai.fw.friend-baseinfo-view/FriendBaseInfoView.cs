using System;
using System.Linq;
using UnityEngine;

public partial class FriendBaseInfoView : MonoBehaviour
{
    private string _uid;

    /// <summary>
    /// 设置改玩家信息的uid
    /// </summary>
    public string UID
    {
        get => this._uid;
        set
        {
            this._uid = value;
            this.Refresh();
        }
    }

    // 头像点击回调
    public Action OnPlayerIconClick;

    async void Refresh()
    {
        var roleInfo = await Database.Stuff.roleDatabase.GetOrRequestIfNeedAsync(this.UID);

        this.RefreshBase(roleInfo);
        this.RefreshGuild(roleInfo);
        this.RefreshStatus();
    }

    void RefreshBase(RoleInfo roleInfo)
    {
        this.PlayerIconView.onClicked = this.OnPlayerIconClick;
        this.PlayerIconView.Uid = this.UID;
        this.Player_Name.text = roleInfo.name;
        this.Last_Time.text = SocializeUtil.GetLoginTimeStr(roleInfo.login);
    }

    async void RefreshGuild(RoleInfo roleInfo)
    {
        var guild = await GuildManager.Stuff.FindGuildAsync(roleInfo.league);
        if (guild == null)
        {
            this.GuildName.text = LocalizationManager.Stuff.GetText("common_none");
        }
        else
        {
            this.GuildName.text = guild.name;
        }
    }

    private async void RefreshStatus()
    {
        var isFans = SocializeManager.Stuff.IsFan(this.UID);
        var isFollow = SocializeManager.Stuff.IsFollow(this.UID);
        var isFriend = SocializeManager.Stuff.IsFriend(this.UID);

        this.Status_Root.SetActive(true);
        this.IntimacyRoot.SetActive(false);
        this.Icon.SetActive(true);

        if (isFriend)
        {
            UiUtil.SetSpriteInBackground(this.Icon, () => "Icon_Friend.png");
            this.IntimacyRoot.SetActive(true);

            var intimacyLv = SocializeManager.Stuff.GetIntimacy(this.UID);
            var tempRows = StaticData.FriendIntimacyTable.ElementList.FindAll(val => val.Point <= intimacyLv);
            var row = tempRows.Last();
            this.Intimacy_Level.text = $"{row.Id}";
        }
        else if (isFollow)
        {
            UiUtil.SetSpriteInBackground(this.Icon, () => "Icon_Follow.png");
        }
        else
        {
            if (isFans)
            {
                UiUtil.SetSpriteInBackground(this.Icon, () => "Icon_Fans.png");
            }
            else
            {
                this.Icon.SetActive(false);
            }
        }
    }
}