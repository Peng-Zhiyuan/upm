using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Session {
    /// <summary>
    /// 当次会话的目标房间号
    /// </summary>
    public string roomId;

    /// <summary>
    /// 当次会话是否已加入目标房间
    /// </summary>
    public bool roomJoined;

    /// <summary>
    /// 当次会话加入房间后，是否已获得房间信息
    /// </summary>
    public bool isRoomInfoReceived;

    /// <summary>
    /// 当前会话是否已连接 socket
    /// </summary>
    public bool isSocketConnected;

    /// <summary>
    /// 下一次要执行的帧号
    /// </summary>
    public int nextExecuteFrameIndex = 1;

    public void IncreaseNextExecuteFrameIndex () {
        this.nextExecuteFrameIndex++;
    }

    /// <summary>
    /// 上一次接收到的实时帧
    /// </summary>
    public int lastReceivedLiveFrameId;

    /// <summary>
    /// 当前是否实时
    /// </summary>
    [ShowInInspector]
    public bool IsLive {
        get {
            var executingFrameId = this.nextExecuteFrameIndex - 1;
            if (executingFrameId == this.lastReceivedLiveFrameId) {
                return true;
            } else {
                return false;
            }
        }
    }
}