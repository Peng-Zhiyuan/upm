using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class OCGateProxy  
{

	#if UNITY_IPHONE
    [DllImport("__Internal")]
    public static extern void gateOnCallAsync(string callId, string clazzName, string method, string arg);

    [DllImport("__Internal")]
    public static extern string gateOnCall(string clazzName, string method, string arg);
    #else

    public static void gateOnCallAsync(string callId, string clazzName, string method, string arg){}

    public static string gateOnCall(string clazzName, string method, string arg){return null;}
    
    #endif
}
