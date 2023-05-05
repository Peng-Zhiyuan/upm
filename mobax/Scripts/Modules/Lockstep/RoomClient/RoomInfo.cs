using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProtoBuf;

public class RoomInfo
{
    public string roomId;

    /// <summary>
    /// 玩家信息在创建房间时就固定，并不表示当前连入的玩家
    /// </summary>
    public Dictionary<string, RoomPlayerInfo> uidToPlayerInfoDic = new Dictionary<string, RoomPlayerInfo>();

    /// <summary>
    /// 已连接的玩家
    /// </summary>
    public Dictionary<string, bool> uidToConnectedPlayer = new Dictionary<string, bool>();

    public List<string> uids = new List<string>();

    public void StorePlayerInfo(RoomPlayerInfo info)
    {
        var uid = info.uid;
        uidToPlayerInfoDic[uid] = info;
        uids.Add(uid);
    }

    public void SetOnlineStatus(string uid, bool isOnline)
    {
        this.uidToConnectedPlayer[uid] = isOnline;
    }

    /// <summary>
    /// 获得玩家在线状态
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    public bool GetOnlineStatus(string uid)
    {
        if (this.uidToConnectedPlayer.ContainsKey(uid))
        {
            var b = this.uidToConnectedPlayer[uid];
            return b;
        }
        else
        {
            return false;
        }
    }

    Dictionary<int, S2CBattleFrame> frameIndexToInputDic = new Dictionary<int, S2CBattleFrame>();

    public void StoreFrameInfoIfNeed(S2CBattleFrame frameInfo, int nextExecuteFrameId)
    {
        var frameId = frameInfo.Id;
        if (frameId >= nextExecuteFrameId)
        {
            frameIndexToInputDic[frameId] = frameInfo;
        }
    }

    public void StoreFrameInfoIfNeed(List<S2CBattleFrame> frameInfoList, int nextExecuteFrameId)
    {
        foreach (var frameInfo in frameInfoList)
        {
            StoreFrameInfoIfNeed(frameInfo, nextExecuteFrameId);
        }
    }

    public S2CBattleFrame GetAndDeleteFrameInfo(int frameId)
    {
        var info = frameIndexToInputDic[frameId];
        frameIndexToInputDic.Remove(frameId);
        return info;
    }

    public bool HasFrameInfo(int frameId)
    {
        var has = frameIndexToInputDic.ContainsKey(frameId);
        return has;
    }
}