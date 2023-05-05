public struct JoinRoomResponse
{
    public string Id;
    public int state;
}

public struct LeaveRoomResponse
{
    public string Id;
    public int state;
}

public struct QueueInfoResponse
{
    public string Id;
    public int Kind;
    public int Status;
    public int Expire;
    public string Address;
}