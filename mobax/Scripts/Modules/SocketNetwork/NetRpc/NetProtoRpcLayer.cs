using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System;
using ProtoBuf;
using UnityEngine;

public class NetProtoRpcLayer : StuffObject<NetProtoRpcLayer>
{
    private bool _isConnect = false;
    private float _lastHeatbeatTime = 0.0f;
    Action<BackMessage> _onNetMessageReceived;
    private IPEndPoint _connectIPEndPoint = null;

    public async Task SocketConnectAsync(Action<BackMessage> CallBackMessage)
    {
        if (SocketManager.Stuff.IsReady)
        {
            _isConnect = true;
            return;
        }
        NetProtoMessageManager.ignoredLogCodeToTrueDic[(int)NetC2sType.C2sHeartbeat] = true;
        NetProtoMessageManager.ignoredLogCodeToTrueDic[(int)NetS2cType.S2cConfirm] = true;
        
        // 连接消息传输层
        // 游戏服的 url
        var gameUrl = LoginManager.Stuff.session.SelectedGameServerInfo.address;
        Debug.Log("gameUrl: " + gameUrl);
        
        // 取 ip
        var splash = gameUrl.LastIndexOf('/');
        if (splash < 0)
        {
            splash = 0;
        }
        var portDot = gameUrl.IndexOf(":", splash, StringComparison.Ordinal);
        if (portDot == -1)
        {
            portDot = gameUrl.Length;
        }
        var ip = gameUrl.Substring(splash, portDot - splash);
        Debug.Log("[CoBattleRoomClient] ip: " + ip);
        
        _connectIPEndPoint = new IPEndPoint(IPAddress.Parse(ip), 0);
        await ConnectAsync(_connectIPEndPoint, CallBackMessage);
        _isConnect = true;
    }

    public async Task ConnectAsync(IPEndPoint endpoint, Action<BackMessage> onNetMessageReceived)
    {
        this._onNetMessageReceived = onNetMessageReceived;
        await NetProtoMessageManager.Stuff.ConnectAsync(endpoint, this.OnBackMessageReceived);
    }

    public void Disconnect()
    {
        _isConnect = false;
        NetProtoMessageManager.Stuff.Disconnect();
    }

    void Update()
    {
        // 只要当前会话连接了 socket，就需要心跳
        if (this._isConnect)
        {
            var now = Time.time;
            var delta = now - _lastHeatbeatTime;
            if (delta >= 3)
            {
                try
                {
                    //Send("C2SHeartbeat", null);
                    _lastHeatbeatTime = now;
                }
                catch (Exception e)
                {
                    this._isConnect = false;
                    ConnectAsync(_connectIPEndPoint, this._onNetMessageReceived);
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }

    void OnBackMessageReceived(BackMessage backMsg)
    {
        if (backMsg.code == NetS2cType.S2cConfirm)
        {
            var confirmProtoMsg = backMsg.protoMessage as S2CConfirm;
            var callId = backMsg.index;
            var hasConfirmHandler = messageIndexToComfirmHandllerDic.ContainsKey(callId);
            if (hasConfirmHandler)
            {
                var handler = messageIndexToComfirmHandllerDic[callId];
                messageIndexToComfirmHandllerDic.Remove(callId);
                handler.Invoke(confirmProtoMsg);
            }
        }
        else
        {
            // 尝试通知订阅
            var backCode = backMsg.code;
            var hasListner = backMsgCodeToHandlerDic.ContainsKey(backCode);
            if (hasListner)
            {
                var handler = backMsgCodeToHandlerDic[backCode];
                backMsgCodeToHandlerDic[backCode] = null;
                handler.Invoke(backMsg);
            }
            _onNetMessageReceived.Invoke(backMsg);
        }
    }

    Dictionary<int, Action<S2CConfirm>> messageIndexToComfirmHandllerDic = new Dictionary<int, Action<S2CConfirm>>();
    Dictionary<NetS2cType, Action<BackMessage>> backMsgCodeToHandlerDic = new Dictionary<NetS2cType, Action<BackMessage>>();

    UInt16 nextMessageIndex = 0;

    UInt16 GetMessageIndex()
    {
        nextMessageIndex++;
        return nextMessageIndex;
    }

    public void Send(NetC2sType code, IExtensible protoMsg, bool needComfirm = true, Action<S2CConfirm> onConfirm = null, NetS2cType ? waitPushCode = null, Action<BackMessage> onPushMessageHandler = null)
    {
        ushort index = 0;
        if (needComfirm)
        {
            index = GetMessageIndex();
            if (onConfirm != null)
            {
                messageIndexToComfirmHandllerDic[index] = onConfirm;
            }
        }
        else
        {
            if (onConfirm != null)
            {
                throw new Exception("[NetRpcLayer] needConfirm set false while onCimfirm is not null");
            }
        }
        if (waitPushCode != null)
        {
            if (onPushMessageHandler == null)
            {
                throw new Exception("[NetRpcLayer] onPushMessageHandler cannot be null");
            }
            backMsgCodeToHandlerDic[waitPushCode.Value] = onPushMessageHandler;
        }
        var sendMsg = new SendMessage();
        sendMsg.code = code;
        sendMsg.index = index;
        sendMsg.protoMessage = protoMsg;
        NetProtoMessageManager.Stuff.TrySend(sendMsg);
    }

    public Task<S2CConfirm> SendAndWaitConfirmAsync(NetC2sType code, IExtensible protoMsg, bool mustSuccess = true)
    {
        var tcs = new TaskCompletionSource<S2CConfirm>();
        Send(code, protoMsg, true, confirmMsg =>
                        {
                            if (mustSuccess)
                            {
                                var confirmCode = confirmMsg.Code;
                                if (confirmCode != 0)
                                {
                                    var serverMsg = confirmMsg.errMsg;
                                    var e = new Exception($"[NetRpcLayer] {code} get comfirm error code: {confirmCode}, server msg: {serverMsg}");
                                    tcs.SetException(e);
                                    return;
                                }
                            }
                            tcs.SetResult(confirmMsg);
                        }
        );
        return tcs.Task;
    }

    public Task<BackMessage> SendAndWaitPush(NetC2sType sendCode, IExtensible protoMsg, NetS2cType waitPushMsgCode)
    {
        var tcs = new TaskCompletionSource<BackMessage>();
        Send(sendCode, protoMsg, true, confirmMsg =>
                        {
                            var confirmCode = confirmMsg.Code;
                            if (confirmCode != 0)
                            {
                                var serverMsg = confirmMsg.errMsg;
                                var e = new Exception($"[NetRpcLayer] {sendCode} get comfirm error code: {confirmCode}, server msg: {serverMsg}");
                                tcs.SetException(e);
                                return;
                            }
                        }, waitPushMsgCode, pushMessage => { tcs.SetResult(pushMessage); }
        );
        return tcs.Task;
    }
}