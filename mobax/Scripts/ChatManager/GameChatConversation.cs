using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GPC.SDK.Chat;
using GPC.SDK.Chat.VO;
using System;
using System.Threading.Tasks;
using System.Threading;
using Sirenix.OdinInspector;
using System.Reflection;
using CustomLitJson;
using Sirenix.OdinInspector;

public class GameChatConversation
{
    public GameConversationType type;

    Conversation raw;

    public GameChatConversation(GameConversationType type, string opponentUid, Conversation raw)
    {
        this.type = type;
        this.uidOrChannelId = opponentUid;
        this.raw = raw;
        //this.LoadHistroyIfNeedInBackground();
    }

    public bool isClosed;

    public Conversation NativeConversaqtion
    {
        get
        {
            if(this.raw == null)
            {
                this.CreateNativeConversationIfNeed();
            }
            return this.raw;
        }
    }


    public async void LoadHistroyIfNeedInBackground()
    {
        await this.LoadHistroyIfNeedAsync();
    }

    bool histroyLoaded;
    /// <summary>
    /// 如果没有加载过历史消息，则会加载 20 条历史消息到列表最前面
    /// </summary>
    /// <returns></returns>
    public Task LoadHistroyIfNeedAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        if (histroyLoaded)
        {
            tcs.SetResult(true);
            return tcs.Task;
        }
        histroyLoaded = true;

        this.CreateNativeConversationIfNeed();
        var firstMessage = this.FirstMessage;
        var firstMessageId = firstMessage?.rawMessage.ID;
        if(firstMessageId == null)
        {
            firstMessageId = "";
        }
        ChatManager.Stuff.HistoryPageUp(this.raw, firstMessageId, 20, (Conversation conversation, ErrorCode err, ChatMessage[] msgs) =>
        {
            if (err.IsNone())
            {
                var chatInfoList = ChatUtil.CreateGameChatInfoList(msgs, firstMessageId);

                // 历史查询新的在前面，但是储存需要旧的在前面
                chatInfoList.Reverse();
                var count = chatInfoList.Count;
                Debug.Log($"[GameChatConversation] insert {count} message by history");
                this.InsertChatMessageListToAhead(chatInfoList);
                tcs.SetResult(true);
            }
            else
            {
                var e = new Exception(err.ToString());
                tcs.SetException(e);
            }
        });
        return tcs.Task;
    }

    public event Action UnreadCountChanged;
    protected void InvokeUnreadCountChanged()
    {
        UnreadCountChanged?.Invoke();
    }

    public virtual void CleanUnread()
    {
        var native = this.NativeConversaqtion;
        GPC.SDK.Chat.ConversationManager.Instance.ResetUnreadCount(native);
        UnreadCountChanged?.Invoke();
        GameConversationManager.Stuff.OnSomeConversationUnreadCountChnaged();
    }

    public virtual int UnreadCount
    {
        get
        {
            var native = this.NativeConversaqtion;
            var ret = native.UnreadCount;
            return ret;
        }
    }

    public void CreateNativeConversationIfNeed()
    {
        if(this.raw == null)
        {
            if(this.type == GameConversationType.Friend)
            {
                var rawConv = GPC.SDK.Chat.ConversationManager.Instance.CreatePrivateConversation(this.uidOrChannelId, false);
                this.raw = rawConv;
            }
            else if(this.type == GameConversationType.World)
            {
                var rawConv = GPC.SDK.Chat.ConversationManager.Instance.CreateBroadcastConversation(false);
                this.raw = rawConv;
            }
            else if(this.type == GameConversationType.Team || this.type == GameConversationType.Guild)
            {
                var rawConv = GPC.SDK.Chat.ConversationManager.Instance.CreateChannelConversation(this.uidOrChannelId, false);
                this.raw = rawConv;
            }
            else if(this.type == GameConversationType.Mix)
            {
                return;
            }
            else
            {
                throw new Exception("[GameChatConversation] not support type: " + this.type);
            }
        }
    }


    public string uidOrChannelId;

    public string OpponentLabel
    {
        get
        {
            var info = Database.Stuff.roleDatabase.Find(this.uidOrChannelId);
            if(info != null)
            {
                return info.name;
            }
            else
            {
                return "(" + this.uidOrChannelId + ")";
            }
        }
    }

    public event Action RoleDataChanged;

    public async void SyncRoleInfoInBackgroundIfNeed()
    {
        var info = Database.Stuff.roleDatabase.Find(this.uidOrChannelId);
        if(info == null)
        {
            await Database.Stuff.roleDatabase.SyncUserIfNeedAsync(this.uidOrChannelId);
            this.RoleDataChanged?.Invoke();
        }
    }

    public event Action<ListChnagingType, ChatInfo> ChatInfoListChanged;

    [ShowInInspector, ReadOnly]
    List<ChatInfo> chatInfoList = new List<ChatInfo>();
    
    public void AddChatMessage(ChatInfo info)
    {
        if(info.conversation == null)
        {
            info.conversation = this;
        }
        this.chatInfoList.Add(info);
        ChatInfoListChanged?.Invoke(ListChnagingType.Append, info);
        this.UnreadCountChanged?.Invoke();
    }

    public ChatInfo FirstMessage
    {
        get
        {
            if(this.chatInfoList.Count > 0)
            {
                return this.chatInfoList[0];
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 在列表最前面插入消息
    /// </summary>
    /// <param name="chatMessageList"></param>
    public void InsertChatMessageListToAhead(List<ChatInfo> chatMessageList)
    {
        foreach(var info in chatMessageList)
        {
            info.conversation = this;
        }
        //this.chatMessageList.AddRange(chatMessageList);
        this.chatInfoList.InsertRange(0, chatMessageList);

        ChatInfoListChanged?.Invoke(ListChnagingType.Reset, null);
        this.UnreadCountChanged?.Invoke();
    }

    public virtual List<ChatInfo> GetChatMessageList()
    {
        return this.chatInfoList;
    }

    public bool SendInvite(int inviteRowId, string extra)
    {
        var customInfo = new GameMessage();
        customInfo.type = GameMessageType.Invite;
        customInfo.inviteRowId = inviteRowId;
        customInfo.extra = extra;
        var b = SendNative(customInfo);
        return b;
    }

    public bool SendInvisible(string invisibleContent)
    {
        var customInfo = new GameMessage();
        customInfo.type = GameMessageType.Invisible;
        customInfo.invisibleContent = invisibleContent;
        var b = SendNative(customInfo, true);
        return b;
    }

    public bool SendExpression(string expression)
    {
        if (IggSdkManager.Stuff.IsShutup)
        {
            Dialog.Confirm("", "chat_banned".Localize());
            return false;
        }

        var customInfo = new GameMessage();
        customInfo.type = GameMessageType.Expresion;
        customInfo.expression = expression;
        var b = SendNative(customInfo);
        return b;
    }

    public bool SendText(string text)
    {
        if (IggSdkManager.Stuff.IsShutup)
        {
            Dialog.Confirm("", "chat_banned".Localize());
            return false;
        }

        var (isPass, finalText) = SendMessageCheck(text);
        if (!isPass)
        {
            return false;
        }
        var customInfo = new GameMessage();
        customInfo.type = GameMessageType.Text;
        customInfo.text = finalText;
        var b = SendNative(customInfo);
        return b;
    }

    bool SendNative(GameMessage msg, bool useCustom1 = false)
    {
        var json = JsonMapper.Instance.ToJson(msg);
        var b = this.SendNative(json, useCustom1);
        return b;
    }

    bool SendNative(string content, bool useCustom1 = false)
    {
        if(this.isClosed)
        {
            Debug.LogError("[GameConversation] conversation closed");
            return false;
        }

        // 记录每日数据
        if(this.type == GameConversationType.World)
        {
            GameTaskUtil.ReportDailyEventWorldChat();
        }



        var rawMsg = CreateRawMsg(content, useCustom1);
        ChatManager.Stuff.Send(rawMsg, (error, message) =>
        {
            if (error.IsNone())
            {
                // 发送成功，且对方在线
                Debug.Log($"[ChatManager] Send Message OK");
                var info = new ChatInfo(message);
                this.AddChatMessage(info);

            }
            else
            {
                var code = error.InnerCode();
                if(code == Error.SDKE_CHAT_USER_OFFLINE)
                {
                    // 如果对方离线，sdk 会返回一个错误，但是离线消息已发送
                    Debug.Log($"[ChatManager] Send Message OK, Opponent offline.");
                    var info = new ChatInfo(message);
                    this.AddChatMessage(info);
                }
                else
                {
                    // 错误码
                    // http://game-chat.git-pages.skyunion.net/hausos-docs/docs/protocol/errors.html
                    Debug.LogError($"[ChatManager] Send Message Error: {error}");
                }
            }
        });
        return true;
    }

    public (bool isPass, string outText) SendMessageCheck(string text)
    {
        if (!ChatManager.Stuff.Avaliable)
        {
            ToastManager.Show("chat server not connected");
            return (false, null);
        }

        text = text.Trim();
        text = text.Replace('\n', ' ');
        var length = text.Length;
        if (length > 100)
        {
            var tip = LocalizationManager.Stuff.GetText("M5_chat_msg_num_limit");
            ToastManager.Show(tip);
            return (false, null);
        }

        if (string.IsNullOrEmpty(text))
        {
            return (false, null);
        }
        return (true, text);
    }

    ChatMessage CreateRawMsg(string content, bool useCustom1 = false)
    {
        var option = new MessageOptions();
        option.sign_needed = false;

        var convType = this.type;
        var opponentId = this.uidOrChannelId;


        if(useCustom1)
        {
            //if(convType == GameConversationType.Friend)
            //{
            //    // 需要对方在线
            //    option.sign_needed = true;
            //    var msg = ChatCustomMessage.CreateType1(MessageType.MSG_PRIVATE, content, option);
            //    var t = msg.GetType();
            //    var p = t.GetProperty(nameof(msg.Receiver), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            //    p.SetValue(msg, opponentId);
            //    return msg;
            //}
            //else
            {
                throw new Exception("[ChatManager] NonVisible content only support private routing");
            }
        }

        if (convType == GameConversationType.Friend)
        {
            if (string.IsNullOrEmpty(opponentId))
            {
                throw new Exception("[ChatManager] opponentUid is empty");
            }
            var msg = new ChatTextMessage(MessageType.MSG_PRIVATE, opponentId, content, option);
            return msg;
        }
        else if (this.type == GameConversationType.World)
        {
            var msg = new ChatTextMessage(MessageType.MSG_BROADCAST, "", content, option);
            return msg;
        }
        else if (this.type == GameConversationType.Team)
        {
            var msg = new ChatTextMessage(MessageType.MSG_CHANNEL, opponentId, content, option);
            return msg;
        }
        else if(this.type == GameConversationType.Guild)
        {
            var msg = new ChatTextMessage(MessageType.MSG_CHANNEL, opponentId, content, option);
            return msg;
        }
        else
        {
            throw new Exception("[ChatManager] unsupport raw message type: " + type);
        }


    }

    //ChatMessage CreateRawMsg(GameConversationType convType, ChatInfoType contentType, string opponentId, string text)
    //{
    //    var option = new MessageOptions();
    //    option.sign_needed = false;

    //    if(contentType == ChatInfoType.Text)
    //    {
    //        if (convType == GameConversationType.Friend)
    //        {
    //            if (string.IsNullOrEmpty(opponentId))
    //            {
    //                throw new Exception("[ChatManager] opponentUid is empty");
    //            }
    //            var msg = new ChatTextMessage(MessageType.MSG_PRIVATE, opponentId, text, option);
    //            //var msg = new ChatTextMessage(MessageType.MSG_CLUSTER_PRIVATE, opponentId, text, option);
    //            return msg;
    //        }
    //        else if (this.type == GameConversationType.World)
    //        {
    //            var msg = new ChatTextMessage(MessageType.MSG_BROADCAST, "", text, option);
    //            //var msg = new ChatTextMessage(MessageType.MSG_CLUSTER_BROADCAST, "", text, option);
    //            return msg;
    //        }
    //        else if(this.type == GameConversationType.Room)
    //        {
    //            var msg = new ChatTextMessage(MessageType.MSG_CHANNEL, opponentId, text, option);
    //            //var msg = new ChatTextMessage(MessageType.MSG_CLUSTER_BROADCAST, "", text, option);
    //            return msg;
    //        }
    //        else
    //        {
    //            throw new Exception("[ChatManager] unsupport raw message type: " + type);
    //        }
    //    }
    //    else if (contentType == ChatInfoType.NonVisible)
    //    {
    //        if (convType == GameConversationType.Friend)
    //        {
    //            // 需要对方在线
    //            option.sign_needed = true;
    //            var msg = ChatCustomMessage.CreateType1(MessageType.MSG_PRIVATE, text, option);
    //            var t = msg.GetType();
    //            var p = t.GetProperty(nameof(msg.Receiver), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
    //            p.SetValue(msg, opponentId);
    //            return msg;
    //        }
    //        else
    //        {
    //            throw new Exception("NonVisible content only support private routing");
    //        }
    //    }
    //    else
    //    {
    //        throw new Exception("unsuuport");
    //    }

    //}

}
