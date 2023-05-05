using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using CustomLitJson;

public static class MatchApi 
{

    public static async Task<JoinRoomResponse> MatchAsync(string queueType)
    {
        Debug.LogError("MatchAsync, queueType:" + queueType);
        var arg = new JsonData();
        arg["kind"] = queueType;
        var response = await NetworkManager.Stuff.CallAsync<JoinRoomResponse>(ServerType.Game, "queue/join", arg);
        return response;
    }

    public static async Task<LeaveRoomResponse> LeaveMatchAsync()
    {
        Debug.LogError("LeaveRoomAsync");
        var response = await NetworkManager.Stuff.CallAsync<LeaveRoomResponse>(ServerType.Game, "queue/leave", null);
        return response;
    }

    public static async Task<QueueInfoResponse> GetMatchInfoAsync()
    {
        Debug.LogError("GetQueueInfoAsync");
        var response = await NetworkManager.Stuff.CallAsync<QueueInfoResponse>(ServerType.Game, "queue/info", null);
        return response;
    }


}
