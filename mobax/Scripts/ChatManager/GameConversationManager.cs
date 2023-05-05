using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GPC.SDK.Chat;
using GPC.SDK.Chat.VO;
using System.Threading.Tasks;
using System;
using Sirenix.OdinInspector;

public class GameConversationManager : StuffObject<GameConversationManager>
{
 

    Dictionary<string, GameChatConversation> uidToPrivateConvDic = new Dictionary<string, GameChatConversation>();


    public async Task ResetConversationListBySdkAsync()
    {
        GPC.SDK.Chat.ConversationManager.Instance.CreateBroadcastConversation(false);
        //GPC.SDK.Chat.ConversationManager.Instance.CreateTopicConversation()

        uidToPrivateConvDic.Clear();
        var list = await RawGetConversationListAsync();
        foreach (var rawConversation in list)
        {
            if(rawConversation is PrivateConversation)
            {
                var privateConv = rawConversation as PrivateConversation;
                var opponentUid = privateConv.ContactID;
                var conv = new GameChatConversation(GameConversationType.Friend, opponentUid, privateConv);
                uidToPrivateConvDic[opponentUid] = conv;
            }
        }
        this.PrivateConversationListChanged?.Invoke(ListChnagingType.Reset, null);
        this.PrivateConversationTotalUnreadChnaged?.Invoke();
    }


    Task<List<Conversation>> RawGetConversationListAsync()
    {
        var tcs = new TaskCompletionSource<List<Conversation>>();
        GPC.SDK.Chat.ConversationManager.Instance.GetConversationList((ErrorCode err, List<Conversation> conversations) =>
        {
            if (err.IsNone())
            {
                tcs.SetResult(conversations);
            }
            else
            {
                var e = new Exception(err.ToString());
                tcs.SetException(e);
            }
        });
        return tcs.Task;
    }

    /// <summary>
    /// 变换方式，追加的元素（如果方式是追加）
    /// </summary>
    public event Action<ListChnagingType, GameChatConversation> PrivateConversationListChanged;

    public List<GameChatConversation> GetPrivateConversationList()
    {
        var ret = new List<GameChatConversation>();
        foreach(var kv in this.uidToPrivateConvDic)
        {
            var uid = kv.Key;
            var conv = kv.Value;
            ret.Add(conv);
        }
        return ret;
    }

    public GameChatConversation GetOrCreatePrivateConversation(string opponentUid)
    {
        this.uidToPrivateConvDic.TryGetValue(opponentUid, out var conv);
        if(conv != null)
        {
            return conv;
        }

        var newConv = new GameChatConversation(GameConversationType.Friend, opponentUid, null);
        newConv.CreateNativeConversationIfNeed();
        this.uidToPrivateConvDic[opponentUid] = newConv;
        PrivateConversationListChanged?.Invoke(ListChnagingType.Append, newConv);
        return newConv;
    }

    public GameChatConversation FindTeamOrGuildConv(string channelId)
    {
        var teamConv = this.TeamConversation;
        if(teamConv?.uidOrChannelId == channelId)
        {
            return teamConv;
        }

        var guildConv = this.GuildConv;
        if (GuildConv?.uidOrChannelId == channelId)
        {
            return guildConv;
        }
        return null;
    }

    Dictionary<string, GameChatConversation> channelIdToRoomConvDic = new Dictionary<string, GameChatConversation>();
    public GameChatConversation GetOrCreateTeamConversation(string channelId)
    {
        this.channelIdToRoomConvDic.TryGetValue(channelId, out var conv);
        if (conv != null)
        {
            return conv;
        }

        var newConv = new GameChatConversation(GameConversationType.Team, channelId, null);
        newConv.CreateNativeConversationIfNeed();
        this.channelIdToRoomConvDic[channelId] = newConv;
        return newConv;
    }

    Dictionary<string, GameChatConversation> channelIdToGuildConvDic = new Dictionary<string, GameChatConversation>();
    public GameChatConversation GetOrCreateGuildConversation(string channelId)
    {
        this.channelIdToGuildConvDic.TryGetValue(channelId, out var conv);
        if (conv != null)
        {
            return conv;
        }

        var newConv = new GameChatConversation(GameConversationType.Guild, channelId, null);
        newConv.CreateNativeConversationIfNeed();
        this.channelIdToGuildConvDic[channelId] = newConv;
        return newConv;
    }

    public event Action<ChatInfo> OnInvisibleMessageReceived;

    public GameChatConversation worldConversation = new GameChatConversation(GameConversationType.World, null, null);
    public void OnNativeChatMessageArrival(Conversation conversation, ChatMessage msg)
    {
        var chatInfo = new ChatInfo(msg);
        if(chatInfo.Type == ChatInfoType.Invisible)
        {
            Debug.Log("[GameConversaqtionManager] invisible message received");
            OnInvisibleMessageReceived?.Invoke(chatInfo);
            return;
        }
        if (conversation.Type == ConversationType.BROADCAST || conversation.Type == ConversationType.CLUSTER_BROADCAST)
        {
            this.worldConversation.AddChatMessage(chatInfo);
        }
        else if(conversation.Type == ConversationType.PRIVATE || conversation.Type == ConversationType.CLUSTER_PRIVATE)
        {
            var uid = msg.Sender.ID;
            var conv = this.GetOrCreatePrivateConversation(uid);
            conv.AddChatMessage(chatInfo);
            this.PrivateConversationTotalUnreadChnaged?.Invoke();
        }
        else if(conversation.Type == ConversationType.CHANNEL || conversation.Type == ConversationType.CLUSTER_CHANNEL)
        {
            var channelRawConv = conversation as ChannelConversation;
            var channelId = channelRawConv.ChannelID;
            var conv = this.FindTeamOrGuildConv(channelId);
            conv?.AddChatMessage(chatInfo);
        }
    }

    public void OnSomeConversationUnreadCountChnaged()
    {
        this.PrivateConversationTotalUnreadChnaged?.Invoke();
    }

    public event Action PrivateConversationTotalUnreadChnaged;

    public int PrivateConversationTotalUnreadCount
    {
        get
        {
            var sum = 0;
            foreach(var kv in this.uidToPrivateConvDic)
            {
                var conv = kv.Value;
                sum += conv.UnreadCount;
            }
            return sum;
        }
    }

    [ShowInInspector, ReadOnly]
    GameChatConversation _roomConversation;
    public GameChatConversation TeamConversation
    {
        get
        {
            return _roomConversation;
        }
        set
        {
            if(_roomConversation == value)
            {
                return;
            }
            var before = _roomConversation;
            _roomConversation = value;
            var after = _roomConversation;
            this.OnRoomConversationChnaged(before, after);
        }
    }

    void OnRoomConversationChnaged(GameChatConversation before, GameChatConversation after)
    {
        if(before != null)
        {
            before.isClosed = true;
        }
        this.TeamConversationChnaged?.Invoke();
    }

    /// <summary>
    /// 加入小队频道，同时只能存在一个小队频道
    /// 如果小队频道已存在，前一个会被替换
    /// </summary>
    /// <param name="channelId"></param>
    public void AddTeamChannel(string channelId)
    {
        var conv = this.GetOrCreateTeamConversation(channelId);
        this.TeamConversation = conv;
    }

    /// <summary>
    /// 离开小队频道
    /// </summary>
    public void RemoveTeamChannel()
    {
        this.TeamConversation = null;
    }


    [ShowInInspector, ReadOnly]
    GameChatConversation _guildConv;
    public GameChatConversation GuildConv
    {
        get
        {
            return _guildConv;
        }
        set
        {
            if (_guildConv == value)
            {
                return;
            }
            var before = _guildConv;
            _guildConv = value;
            var after = _guildConv;
            this.OnGuildConvChnaged(before, after);
        }
    }

    public event Action GuildConversationChnaged;
    void OnGuildConvChnaged(GameChatConversation before, GameChatConversation after)
    {
        if (before != null)
        {
            before.isClosed = true;
        }
        this.GuildConversationChnaged?.Invoke();
    }


    /// <summary>
    /// 加入工会频道，同时只能存在一个工会频道
    /// 如果工会频道已存在，前一个会被替换
    /// </summary>
    /// <param name="channelId"></param>
    public void AddGuildChannel(string channelId)
    {
        var conv = this.GetOrCreateGuildConversation(channelId);
        this.GuildConv = conv;
    }

    /// <summary>
    /// 离开工会频道
    /// </summary>
    public void RemoveGuildChannel()
    {
        this.GuildConv = null;
    }


    public event Action TeamConversationChnaged;
    public void OnNativeChannelChanged(ChannelConversation oldConversation, ChannelConversation newConversation)
    {
        if (newConversation != null)
        {
            var channelId = newConversation.ChannelID;
            Debug.Log("[GameConversationManager] server notify Channel chnaged, id: " + channelId);
            ////var conv = new GameChatConversation(GameConversationType.Room, channelId, newConversation);
            //var conv = this.GetOrCreateRoomConversation(channelId);
            //this.RoomConversation = conv;
        }
        else
        {
            Debug.Log("[GameConversationManager] server notify Channel exists. ");
            //this.RoomConversation = null;
        }
    }

    public void OnChannelUserJoined(ChannelConversation conversation, string[] joinedIDs)
    {
        var myUid = Database.Stuff.roleDatabase.Me._id;
        if (Array.IndexOf(joinedIDs, myUid) != -1)
        {
            // 我加入了房间
            var channelId = conversation.ChannelID;
            Debug.Log("[GameConversationManager] server notify Channel chnaged, id: " + channelId);
            //var conv = this.GetOrCreateRoomConversation(channelId);
            ////var conv = new GameChatConversation(GameConversationType.Room, channelId, conversation);
            //this.RoomConversation = conv;
        }
    }
}
