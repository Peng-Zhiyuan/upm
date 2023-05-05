using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MixConversationManager : StuffObject<MixConversationManager>
{
    MixConversation mixConv;
    public MixConversation MixConv
    {
        get
        {
            if (this.mixConv == null)
            {
                this.mixConv = new MixConversation();

                // 增加世界会话
                var worldConv = GameConversationManager.Stuff.worldConversation;
                this.mixConv.AddInput(worldConv);

                // 增加所有好友会话
                this.AddAllPrivateConvToInput();

                // 增加小队会话
                this.ResetTeamConvInput();

                // 增加工会会话
                this.ResetGuildConvInput();

                GameConversationManager.Stuff.PrivateConversationListChanged += PrivateConversationListChanged;
                GameConversationManager.Stuff.TeamConversationChnaged += TeamConvChnaged;
                GameConversationManager.Stuff.GuildConversationChnaged += GuidConvChnaged;
            }
            return this.mixConv;
        }
    }
    void GuidConvChnaged()
    {
        this.ResetGuildConvInput();
    }

    void ResetGuildConvInput()
    {
        if (this.mixConv == null)
        {
            return;
        }
        this.RemoveAllInputConvOfType(GameConversationType.Guild);
        var newConv = GameConversationManager.Stuff.GuildConv;
        if (newConv != null)
        {
            this.mixConv.AddInput(newConv);
        }
    }

    void TeamConvChnaged()
    {
        this.ResetTeamConvInput();
    }

    void ResetTeamConvInput()
    {
        if (this.mixConv == null)
        {
            return;
        }
        this.RemoveAllInputConvOfType(GameConversationType.Team);
        var newTeamConv = GameConversationManager.Stuff.TeamConversation;
        if(newTeamConv != null)
        {
            this.mixConv.AddInput(newTeamConv);
        }
    }

    void PrivateConversationListChanged(ListChnagingType type, GameChatConversation conv)
    {
        if(this.mixConv == null)
        {
            return;
        }

        if(type == ListChnagingType.Append)
        {
            this.mixConv.AddInput(conv);
        }
        else if(type == ListChnagingType.Reset)
        {
            this.RemoveAllInputConvOfType(GameConversationType.Friend);
            this.AddAllPrivateConvToInput();
        }
    }

    void AddAllPrivateConvToInput()
    {
        var allPrivateConvList = GameConversationManager.Stuff.GetPrivateConversationList();
        foreach(var conv in allPrivateConvList)
        {
            mixConv.AddInput(conv);
        }
    }

    void RemoveAllInputConvOfType(GameConversationType convType)
    {
        var toRemoveList = this.mixConv.FindInput(convType);
        foreach (var one in toRemoveList)
        {
            mixConv.RemoveInput(one);
        }
    }
}
