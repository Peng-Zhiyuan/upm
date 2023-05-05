using ProtoBuf;

public struct BackMessage
{
    public NetS2cType code;
    public int index;
    public IExtensible protoMessage;
}