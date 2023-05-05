using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using ProtoBuf;
using UnityEngine;

public class ChatRoomClient : StuffObject<ChatRoomClient>
{
    private bool IsChatSession = false;
    private string roomID = "ChatRoom";
    private float lastHeatbeatTime;
    private System.Action<ChatMsg> onNetMessageReceivedTS;

    public System.Action<ChatMsg> delegateNetMessageReceiveCSharp;

    /// <summary>
    /// 战斗聊天界面临时存储，因目前战斗界面还是cs
    /// </summary>
    /// <typeparam name="ChatMsg"></typeparam>
    /// <returns></returns>
    public List<ChatMsg> TempBattlePageChatList = new List<ChatMsg>();

    public async Task JoinChatNetWorkAsync(System.Action<ChatMsg> _onNetMessageReceived)
    {
        if (this.IsChatSession)
            return;
        NetProtoMessageManager.ignoredLogCodeToTrueDic[(int)NetC2sType.C2sHeartbeat] = true;
        NetProtoMessageManager.ignoredLogCodeToTrueDic[(int)NetS2cType.S2cConfirm] = true;

        // 连接消息传输层
        // 游戏服的 url
        // eg: http://71.132.4.205:85
        var gameUrl = LoginManager.Stuff.session.SelectedGameServerInfo.address; //LoginCsharpInfo.gameServerUrl;
        Debug.Log("gameUrl: " + gameUrl);

        // 取 ip
        var splash = gameUrl.LastIndexOf('/');
        var portDot = gameUrl.IndexOf(":", splash);
        if (portDot == -1)
        {
            portDot = gameUrl.Length;
        }
        var ip = gameUrl.Substring(splash + 1, portDot - splash - 1);
        Debug.Log("[ChatRoomClient] ip: " + ip);
        IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ip), 88);
        await NetProtoRpcLayer.Stuff.ConnectAsync(endpoint, OnNetMessageReceived);
        this.IsChatSession = true;
        Debug.Log("Connet ChatRoom");
        this.onNetMessageReceivedTS = _onNetMessageReceived;
        TempBattlePageChatList.Clear();
    }

    public void QuitChatNetWorkAsync()
    {
        this.IsChatSession = false;
        this.onNetMessageReceivedTS = null;
        NetProtoMessageManager.Stuff.Disconnect();
        TempBattlePageChatList.Clear();
    }

    void Update()
    {
        // 只要当前会话连接了 socket，就需要心跳
        if (this.IsChatSession)
        {
            var now = Time.time;
            var delta = now - lastHeatbeatTime;
            if (delta >= 20)
            {
                try
                {
                    NetProtoRpcLayer.Stuff.Send(NetC2sType.C2sHeartbeat, null);
                    lastHeatbeatTime = now;
                }
                catch (Exception e)
                {
                    this.IsChatSession = false;
                    this.JoinChatNetWorkAsync(this.onNetMessageReceivedTS);
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }

    public void ShowAllChatPanel() { }

    public void SendChatMsg(string msg)
    {
        ChatMsg chatMsg = new ChatMsg();
        chatMsg._id = LoginCsharpInfo.selectedRoleId;
        chatMsg.name = LoginCsharpInfo.selectedRoleName;
        chatMsg.headIcon = "";
        chatMsg.msg = msg;
        ReceiveChatMsg(chatMsg);
    }

    void OnNetMessageReceived(BackMessage package)
    {
        var getCode = package.code;
        var protoMsg = package.protoMessage as C2SBattleChat;
        ReceiveChatMsg(JsonUtility.FromJson<ChatMsg>(protoMsg.Msg));
    }

    public void ReceiveChatMsg(ChatMsg chatMsg)
    {
        TempBattlePageChatList.Add(chatMsg);
        Debug.LogWarning($"Receive Msg {chatMsg._id} :  {chatMsg.msg}");
        onNetMessageReceivedTS?.Invoke(chatMsg);
        delegateNetMessageReceiveCSharp?.Invoke(chatMsg);
    }
}