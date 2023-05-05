using CustomLitJson;
using System;

public class NetworkResult
{
    public Exception exception;
    public string text;
    public NetMsg<JsonData> msgWithDataTypeIsJsonData;
}