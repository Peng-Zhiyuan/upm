using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using GPC.SDK.Chat;
using GPC.SDK.Chat.VO;
using System;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 文档：http://chat-sdk-unity-integration.rtd.skyunion.net/zh_CN/latest/
/// </summary>
public class ChatManager : StuffObject<ChatManager>, IChatEventDelegater
{
    /// <summary>
    /// 初始化
    /// </summary>
    private string partitionValue;
    private string clusterValue;
    void Initialization()
    {
        // 第 1 步： 初始化 SDK。
        // 这里需要：唯一标识符、集群 ID、服务器 ID。
      
        var env = EnvManager.FinalEnv;// VariantManager.Variant;
        if (env == "live")
        {
            clusterValue = "h3zk92t5pe-c1";
            partitionValue = "h3zk92t5pe-c1-p1";
        }
        else 
        {
            clusterValue = "h3zk92t5pe-tc1";
            partitionValue = "h3zk92t5pe-tc1-p1";
        }
        //var partition = this.Partition;
        var cfg = new InitParameters {clientID = this.LoadOrCreateGuestId(), cluster = clusterValue, partition = partitionValue, };
        ChatSDKConfig.InitSDK(cfg);

        DebugHelper.EnableLog = false;

        // 第 2 步： 添加侦听。
        // 侦听聊天服务连接状态和 Token 过期事件。
        RegisterEvent();
#if UNITY_EDITOR
        EditorApplication.quitting += () => { ChatSDK.Instance.Disconnect(); };
#endif
    }

    [ShowInInspector]
    public string Partition
    {
        get
        {
            return partitionValue;
                // pzy:
                // patch
           /* var defaultValue = "h3zk92t5pe-tc1-p1";
            var v = EnvManager.GetConfigOfFinalEnv("chat.partition", defaultValue);
            return v;*/
        }
    }

    string LoadOrCreateGuestId()
    {
        var cached = PlayerPrefs.GetString("clientId", "");
        if (!string.IsNullOrEmpty(cached))
        {
            return cached;
        }

        var guid = Guid.NewGuid();
        var guestGuid = guid.ToString();
        PlayerPrefs.SetString("clientId", guestGuid);
        return guestGuid;
    }

    bool isInited;

    public async void TryInitalizeThenConnectAsync()
    {
        if (!isInited)
        {
            this.Initialization();
            isInited = true;
        }

        await ConnectAsync();
    }

    void OnApplicationQuit()
    {
        // 第 3 步： 关闭 SDK。
        ChatSDK.Instance.Disconnect();
    }

    /// <summary>
    /// 连接 
    /// </summary>
    public async Task ConnectAsync()
    {
        if(this.Status != ChatStatus.Disconnected)
        {
            throw new Exception("[ChatManager] can not connect in status: " + this.Status);
        }

        var msg = await ChatApi.RequestTokenAsync();
        if(!msg.IsSuccess)
        {
            return;
        }
        var msg2 = msg as NetMsg<string>;
        var token = msg2.data;

        // 第 2 步： 连接到聊天服务器；在初始化 SDK 成功后调用。
        // subscribedTopics 字段是话题，可以看做是带有过滤器的广播。
        // 由游戏服务端定义出的话题，在连接 SDK 的时候传入。也可以认为是微博中——#日本核辐射#
        var playerInfo = new PlayerInfo {UserID = LoginManager.Stuff.session.selectedRoleData._id, Token = token, Topics = null,};
        ChatSDKConfig.ConfigPlayerInfo(playerInfo);
        ChatSDK.Instance.Connect();
    }

    /// <summary>
    /// 侦听网络状态的变化
    /// </summary>
    private void RegisterEvent()
    {
        // Optional: 处理网络变化事件
        ChatSDK.Instance.ConnChanged += OnConnectChanged;

        // 处理过期事件，游戏可以通过该接口，使用新的 Token 来续期。
        ChatSDK.Instance.RenewTokenRequired += OnRenewTokenRequired;

        //  聊天事件处理
        ChatSDK.Instance.MessageReceived += OnMessageReceivedHandler;

        ChatSDK.Instance.SetChatEventHandler(this);


    }

    //public List<ChatInfo> chatMessageList = new List<ChatInfo>();

    //public Action OnChatMessageChanged;

    //public string OnSocializeFocusSomebody = "#Focus_Somebody"; // 关注某人
    //public string OnSocializeCancelFocusSomebody = "#Cancel_Focus_Somebody"; // 取消关注某人

    public Action<UserInfo> OnSocializeFocusChanged; // 社交关系修改 -- 关注某人
    public Action<UserInfo> OnSocializeCancelFocusChanged; // 社交关系修改 -- 取消关注

    public void OnMessageReceivedHandler(Conversation conversation, ChatMessage msg)
    {
        this.RunInMainThread(() =>
            {
                Debug.Log($"[ChatManager] received, conversation: {conversation}, msg: {msg}");
                GameConversationManager.Stuff.OnNativeChatMessageArrival(conversation, msg);

                //if (conversation.Type == ConversationType.PRIVATE)
                //{
                //    if (msg.Content.Equals(this.OnSocializeFocusSomebody))
                //    {
                //        OnSocializeFocusChanged?.Invoke(msg.Sender);
                //    }
                //    else if (msg.Content.Equals(this.OnSocializeCancelFocusSomebody))
                //    {
                //        OnSocializeCancelFocusChanged?.Invoke(msg.Sender);
                //    }
                //}
                //else
                //{
                //    var info = new ChatInfo(msg);
                //    chatMessageList.Add(info);
                //    OnChatMessageChanged?.Invoke();
                //}
            }
        );
    }


    public ChatStatus _status;

    public ChatStatus Status
    {
        get
        {
            return _status;
        }
        set
        {
            if(_status == value)
            {
                return;
            }
            _status = value;
            this.AvaliableStatusChanged?.Invoke();
        }
    }

    public CloseReason closeReason;
    public CurrentUserInfo userInfo;


    public event Action AvaliableStatusChanged;
    public bool Avaliable
    {
        get
        {
            return this.Status == ChatStatus.ConversationLoaded;
        }
    }
    private void OnConnectChanged(ConnState state, CurrentUserInfo userInfo, ErrorCode err)
    {
        this.RunInMainThread(async () =>
            {
                // 与服务端成功连接
                if (state == ConnState.Connected)
                {
                    Debug.Log($"[ChatManager] Connected, try load conversation...");
                    this.Status = ChatStatus.Connected;
                    this.closeReason = CloseReason.None;
                    this.userInfo = userInfo;
                    Debug.Log("[ChatManager] my meta: " + userInfo.MetaData);
                    try
                    {
                        await GameConversationManager.Stuff.ResetConversationListBySdkAsync();
                    }
                    catch(Exception e)
                    {
                        Debug.Log($"[ChatManager] Load Raw Conversation Error: {e.Message}");
                        //this.Status = ChatStatus.Disconnected;
                        //this.closeReason = (CloseReason)err.
                        //return;
                    }
                    this.Status = ChatStatus.ConversationLoaded;
                    Debug.Log("[ChatManager] conversation has been reseted, every thing is ready");
                }
                else if (state == ConnState.NotConn)
                {
                    //处理与聊天服务器的连接异常
                    Debug.Log($"[ChatManager] Disconnected {err.InnerCode()}");
                    this.Status = ChatStatus.Disconnected;
                    this.closeReason = (CloseReason) err.InnerCode();
                    //this.userInfo = null;
                }
                else if (state == ConnState.Connecting)
                {
                    Debug.Log($"[ChatManager] Connecting...");
                    Status = ChatStatus.Connecting;
                }
            }
        );
    }

    async void RequestTokenInBackground(Action<string> handler, Action<Exception> onExceptionHandler)
    {
        try
        {
            var msg = await ChatApi.RequestTokenAsync();
            if(msg.IsSuccess)
            {
                var msg2 = msg as NetMsg<string>;
                var token = msg2.data;
                handler?.Invoke(token);
            }
        }
        catch (Exception e)
        {
            onExceptionHandler?.Invoke(e);
        }
    }

    float allowedTime;
    private bool OnRenewTokenRequired(RenewTokenHandler tokenHandler, IntPtr cookies)
    {
        this.RunInMainThread(async () =>
        {
            var now = Time.timeSinceLevelLoad;
            if (now < allowedTime)
            {
                var waitSec = allowedTime - now;
                var ms = (int)(waitSec * 1000);
                await Task.Delay(ms);
            }
            allowedTime = Time.timeSinceLevelLoad + 10;

            RequestTokenInBackground(token => { tokenHandler.Invoke(token, cookies); },
                e => { Debug.Log($"[ChatManager] error in renew token: {e.Message}"); });
        });

  

        // 返回 true 表示客户端将会对 token 进行续期
        return true;
    }

    Queue<Action> jobQueue = new Queue<Action>();

    void Update()
    {
        if(jobQueue == null)
        {
            return;
        }
        while (jobQueue.Count > 0)
        {
            var job = jobQueue.Dequeue();
            job.Invoke();
        }
    }

    void RunInMainThread(Action job)
    {
        jobQueue.Enqueue(job);
    }

  

    public void Send(ChatMessage rawMsg, MessageSendResultHandler handler)
    {

        ChatSDK.Instance.SendMessage(rawMsg, (error, message) =>
            {
                RunInMainThread(() =>
                    {
                        handler?.Invoke(error, message);
                    }
                );
            }
        );
    }

    public void HistoryPageUp(Conversation conv, string anchorMessageId, int searchCount, GPC.SDK.Chat.ConversationManager.HistoryResultHandler callback)
    {
        GPC.SDK.Chat.ConversationManager.Instance.HistoryPageUp(conv, anchorMessageId, searchCount, (Conversation conversation, ErrorCode err, ChatMessage[] msgs) => {
            RunInMainThread(() =>
            {
                callback?.Invoke(conversation, err, msgs);
            }
            );
        });
    }

    public enum MessageRoutingType
    {
        World,
        Private,
    }


    public bool IsSelf(string uid)
    {
        var myUid = this.userInfo.ID;
        if (uid == myUid)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void OnUserChanged(CurrentUserInfo userInfo)
    {
        //throw new NotImplementedException();
    }

    public void OnUserTopicChanged(string[] topics)
    {
        //throw new NotImplementedException();
    }

    public void OnChannelChanged(ChannelConversation oldConversation, ChannelConversation newConversation)
    {
        this.RunInMainThread(() =>
        {
            GameConversationManager.Stuff.OnNativeChannelChanged(oldConversation, newConversation);
        });
    }

    public void OnChannelUserExited(ChannelConversation conversation, string[] exitedIDs)
    {
        throw new NotImplementedException();
    }

    public void OnChannelUserJoined(ChannelConversation conversation, string[] joinedIDs)
    {
        this.RunInMainThread(() =>
        {
            GameConversationManager.Stuff.OnChannelUserJoined(conversation, joinedIDs);
        });
    }
}