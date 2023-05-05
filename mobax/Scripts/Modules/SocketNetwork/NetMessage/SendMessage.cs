using ProtoBuf;

public struct SendMessage
{
    public NetC2sType code;
    public ushort index;
    public IExtensible protoMessage;
}