using GPC.SDK.Chat.VO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MixConversation : GameChatConversation
{
    List<GameChatConversation> inputConvList = new List<GameChatConversation>();

    public MixConversation() : base(GameConversationType.Mix, null, null)
    {
    }

    public void AddInput(GameChatConversation conv)
    {
        var b = IsInput(conv);
        if(b)
        {
            return;
        }
        inputConvList.Add(conv);
        conv.ChatInfoListChanged += OnAnyInputChatInfoListChanged;
        conv.UnreadCountChanged += OnAnyInputUnreadCountChnaged;
    }

    void OnAnyInputUnreadCountChnaged()
    {
        this.InvokeUnreadCountChanged();
    }

    bool IsInput(GameChatConversation conv)
    {
        var b = this.inputConvList.Contains(conv);
        return b;
    }

    public List<GameChatConversation> FindInput(GameConversationType type)
    {
        var ret = new List<GameChatConversation>();
        foreach(var one in this.inputConvList)
        {
            if(one.type == type)
            {
                ret.Add(one);
            }
        }
        return ret;
    }

    public void RemoveInput(GameChatConversation conv)
    {
        var isInput = IsInput(conv);
        if(!isInput)
        {
            return;
        }
        conv.ChatInfoListChanged -= OnAnyInputChatInfoListChanged;
        conv.UnreadCountChanged -= OnAnyInputUnreadCountChnaged;
    }

    void OnAnyInputChatInfoListChanged(ListChnagingType type, ChatInfo chatInfo)
    {
        if(type == ListChnagingType.Append)
        {
            this.AddChatMessage(chatInfo);
        }
    }

    public override void CleanUnread()
    {
        // 混合会话无法清除任何未读消息
    }

    public override int UnreadCount
    {
        get
        {
            var sum = 0;
            foreach(var conv in inputConvList)
            {
                // 不统计世界聊天
                if(conv.type == GameConversationType.World)
                {
                    continue;
                }
                sum += conv.UnreadCount;
            }
            return sum;
        }
    }

}
