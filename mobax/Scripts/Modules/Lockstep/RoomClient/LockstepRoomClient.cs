using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CustomLitJson;
using ProtoBuf;
using ProtoBuf;
using Sirenix.OdinInspector;
using UnityEngine;

public class LockstepRoomClient : StuffObject<LockstepRoomClient>
{
    public Action<S2CBattleFrame, bool> onExecuteFrame;

    public Dictionary<string, RoomInfo> roomIdToRoomInfo = new Dictionary<string, RoomInfo>();

    /// <summary>
    /// 当前会话的目标房间的信息
    /// </summary>
    public RoomInfo RoomInfo
    {
        get
        {
            if (session == null)
            {
                throw new Exception("[LockstepRoomClient] session not created");
            }

            //var joined = session.roomJoined;
            //if(!joined)
            //{
            //    throw new Exception("[LockstepRoomClient] session not joined any room");
            //}
            var roomId = session.roomId;
            var roomInfo = this.GetRoomInfo(roomId);
            return roomInfo;
        }
    }

    public Session session;

    public Action<string, string> onChatMsgReceived;

    public async Task JoinAsync(IPEndPoint endpoint, string uid, string roomId, Action<S2CBattleFrame, bool> onExecuteFrame, int executeStartFrameIndex = 1, Action<string, string> onChatMsgReceived = null)
    {
        // 底层不打印 S2cBattleFrame 消息的日志，因为太多了
        NetProtoMessageManager.ignoredLogCodeToTrueDic[(int)NetS2cType.S2cBattleFrame] = true;
        NetProtoMessageManager.ignoredLogCodeToTrueDic[(int)NetC2sType.C2sHeartbeat] = true;
        NetProtoMessageManager.ignoredLogCodeToTrueDic[(int)NetS2cType.S2cConfirm] = true;
        this.onExecuteFrame = onExecuteFrame;
        this.onChatMsgReceived = onChatMsgReceived;
        var roomInfo = this.CreateRoomInfoIfNeed(roomId);
        this.RemoveRoomInfoExlude(roomId);

        // 连接消息传输层
        //await NetMessageManager.Stuff.ConnectAsync(endpoint, OnNetMessageReceived);
        await NetProtoRpcLayer.Stuff.ConnectAsync(endpoint, OnNetMessageReceived);

        // 创建会话
        var session = new Session();
        session.roomId = roomId;
        session.isSocketConnected = true;
        session.nextExecuteFrameIndex = executeStartFrameIndex;
        session.isRoomInfoReceived = false;
        this.session = session;

        // 加入房间
        var protoMsg = new C2SBattleJoin();
        protoMsg.Uid = uid;
        protoMsg.Id = roomId;
        await NetProtoRpcLayer.Stuff.SendAndWaitConfirmAsync(NetC2sType.C2sBattleJoin, protoMsg, true);
        session.roomJoined = true;

        // 获取房间信息
        await this.RequestRoomInfoAsync();
    }

    RoomInfo CreateRoomInfoIfNeed(string roomId)
    {
        var has = roomIdToRoomInfo.ContainsKey(roomId);
        if (!has)
        {
            var roomInfo = new RoomInfo();
            roomInfo.roomId = roomId;
            roomIdToRoomInfo[roomId] = roomInfo;
            return roomInfo;
        }
        else
        {
            var ret = roomIdToRoomInfo[roomId];
            return ret;
        }
    }

    RoomInfo GetRoomInfo(string roomId)
    {
        var info = roomIdToRoomInfo[roomId];
        return info;
    }

    void RemoveRoomInfoExlude(string keepRoomId)
    {
        var info = roomIdToRoomInfo[keepRoomId];
        roomIdToRoomInfo.Clear();
        roomIdToRoomInfo[keepRoomId] = info;
    }

    /// <summary>
    /// 请求指定范围内缺少的帧
    /// </summary>
    void RequestLostFrameAsync(int frameIndexStart, int frameIndexEnd)
    {
        // 检查 [start, end] 帧 (闭区间) 之间是否有漏帧
        var lostIndexStart = -1;
        var lostIndexEnd = -1;
        for (int i = frameIndexStart; i <= frameIndexEnd; i++)
        {
            var has = this.RoomInfo.HasFrameInfo(i);
            if (!has)
            {
                if (lostIndexStart == -1)
                {
                    lostIndexStart = i;
                }
                if (lostIndexEnd == -1)
                {
                    lostIndexEnd = i;
                }
                if (i > lostIndexEnd)
                {
                    lostIndexEnd = i;
                }
            }
        }
        if (lostIndexStart != -1
            && lostIndexEnd != -1)
        {
            // 请求补帧
            var protoMsg = new C2SBattleReFrame();
            protoMsg.From = lostIndexStart;
            protoMsg.Target = lostIndexEnd;
            NetProtoRpcLayer.Stuff.Send(NetC2sType.C2sBattleReframe, protoMsg);
        }
    }

    async Task RequestRoomInfoAsync()
    {
        var backMsg = await NetProtoRpcLayer.Stuff.SendAndWaitPush(NetC2sType.C2sBattlePlayers, null, NetS2cType.S2cBattlePlayers);
        var roomInfoMsg = backMsg.protoMessage as S2CBattlePlayers;
        var serverFrameId = roomInfoMsg.Frame;

        // 记录随机数种子
        var roomInfo = this.RoomInfo;

        // 记录玩家信息
        var playerList = roomInfoMsg.Lists;
        foreach (var one in playerList)
        {
            // 记录玩家固定状态
            var info = ProtoPlayerToRoomPlayerInfo(one);
            RoomInfo.StorePlayerInfo(info);
            var seed = one.Seed;
            Debug.Log("seed: " + seed);
            info.randomSeed = seed;
            // 记录玩家在线状态
            var uid = one.Uid;
            var isOnline = one.Online;
            RoomInfo.SetOnlineStatus(uid, isOnline == OnlineStatus.Online);
        }

        // 如果服务器当前帧不是 0，表示已经提前开始
        if (serverFrameId != 0)
        {
            // 需要确保的帧号的范围
            var needFrameIdStart = this.session.nextExecuteFrameIndex;
            var needFrameIdEnd = serverFrameId;

            // 请求指定范围内缺少的帧
            this.RequestLostFrameAsync(needFrameIdStart, needFrameIdEnd);
        }

        // 标记已收到房间信息
        this.session.isRoomInfoReceived = true;
    }

    RoomPlayerInfo ProtoPlayerToRoomPlayerInfo(ProtoBuf.Player protoPlayer)
    {
        var info = new RoomPlayerInfo();
        info.name = protoPlayer.Name;
        info.uid = protoPlayer.Uid;
        info.randomSeed = protoPlayer.Seed;
        info.heros = protoPlayer.Heroes;
        // info.heros = protoPlayer.Heroes;
        //info.heros.AddRange(protoPlayer.Heroes);
        /* for (int i = 0; i < protoPlayer.Heroes.Count; i++)
         {
             var hero = protoPlayer.Heroes[i];
             var heroData = new HeroData();
             heroData.attr = hero.Attrs;
             heroData.id = hero.Id;
             heroData._id = hero.Skills
             for (int j = 0; j < hero.Attrs.Length; i++)
             {
                 heroData.attr.Add(hero.Attrs[j]);
             }
             heroData.
             hero.attr = protoPlayer.Heroes[i].Attrs;
             info.heros.Add();
         }*/
        return info;
    }

    public void Exit()
    {
        this.session = null;
        NetProtoRpcLayer.Stuff.Disconnect();
    }

    float lastHeatbeatTime;

    void Update()
    {
        // 只要当前会话连接了 socket，就需要心跳
        if (this.session?.isSocketConnected == true)
        {
            var now = Time.time;
            var delta = now - lastHeatbeatTime;
            if (delta >= 1)
            {
                NetProtoRpcLayer.Stuff.Send(NetC2sType.C2sHeartbeat, null, false);
                lastHeatbeatTime = now;
            }
        }
    }

    //int lastReceivedLiveFrameId;

    /// <summary>
    /// 当从消息传输层获得消息
    /// </summary>
    /// <param name="package"></param>
    void OnNetMessageReceived(BackMessage package)
    {
        var getCode = package.code;
        if (getCode == NetS2cType.S2cBattleFrame)
        {
            // 帧信息
            var protoMsg = package.protoMessage as S2CBattleFrame;
            var frameIndex = protoMsg.Id;
            var roomInfo = this.RoomInfo;
            roomInfo.StoreFrameInfoIfNeed(protoMsg, this.session.nextExecuteFrameIndex);
            this.session.lastReceivedLiveFrameId = frameIndex;
            Debug.Log($"[LockstempManager] live frameIndex: {frameIndex} received");
            this.TryExecute();
        }
        else if (getCode == NetS2cType.S2cBattleStart)
        {
            // 帧同步开始
        }
        else if (getCode == NetS2cType.S2cBattleReframe)
        {
            // 补帧信息
            var protoMsg = package.protoMessage as S2CBattleReFrame;
            var frameInfoList = protoMsg.Frames;
            this.RoomInfo.StoreFrameInfoIfNeed(frameInfoList, this.session.nextExecuteFrameIndex);
            var isFinished = protoMsg.Finish;

            // 补帧信息可能分包传送，当最后一个包到达时 isFinished 为 1
            // 仅当所有补帧信息到达时才尝试执行
            if (isFinished == 1)
            {
                this.TryExecute();
            }
        }
        else if (getCode == NetS2cType.S2cBattleJoin)
        {
            // 有玩家加入房间
            // 自己加入时也会收到广播
            var protoMsg = package.protoMessage as S2CBattleJoin;
            var uid = protoMsg.Uid;
            var roomInfo = this.RoomInfo;
            roomInfo.SetOnlineStatus(uid, true);
        }
        else if (getCode == NetS2cType.S2cBattleChat)
        {
            var protoMsg = package.protoMessage as S2CBattleChat;
            var uid = protoMsg.Uid;
            var msg = protoMsg.Msg;
            this?.onChatMsgReceived(uid, msg);
        }
    }

    /// <summary>
    /// 尽可能的执行帧，直到缺帧信息为止
    /// </summary>
    void TryExecute()
    {
        // 如果房间信息尚未收到，不要执行
        if (!this.session.isRoomInfoReceived)
        {
            return;
        }
        var roomInfo = this.RoomInfo;
        while (roomInfo.HasFrameInfo(this.session.nextExecuteFrameIndex))
        {
            var nextNextFrameId = this.session.nextExecuteFrameIndex + 1;
            var isFastForward = false;
            if (roomInfo.HasFrameInfo(nextNextFrameId))
            {
                isFastForward = true;
            }
            var frameInfo = roomInfo.GetAndDeleteFrameInfo(this.session.nextExecuteFrameIndex);
            this.session.IncreaseNextExecuteFrameIndex();
            if (onExecuteFrame == null)
            {
                throw new Exception("[LockstepManager] onExecuteFrame not set yet");
            }
            onExecuteFrame.Invoke(frameInfo, isFastForward);
        }
    }

    /// <summary>
    /// 发送输入指令
    /// 服务器会在下一次可行的帧循环中派发此指令
    /// </summary>
    /// <param name="inputProtoMsg"></param>
    public void SendInput(C2SBattleEvents inputProtoMsg)
    {
        if (this.session?.isSocketConnected == true)
        {
            var sendMsg = new SendMessage();
            sendMsg.code = NetC2sType.C2sBattleEvents;
            sendMsg.protoMessage = inputProtoMsg;
            NetProtoMessageManager.Stuff.TrySend(sendMsg);
        }
    }

    public void SendMsg(C2SBattleChat inputProtoMsg)
    {
        if (this.session?.isSocketConnected == true)
        {
            var sendMsg = new SendMessage();
            sendMsg.code = NetC2sType.C2sBattleChat;
            sendMsg.protoMessage = inputProtoMsg;
            NetProtoMessageManager.Stuff.TrySend(sendMsg);
        }
    }

    private void OnDestroy()
    {
        this.Exit();
    }
}