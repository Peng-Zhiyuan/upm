using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CircularBuffer;
using UnityEngine;
using System.Runtime.InteropServices;

public class SocketManager : StuffObject<SocketManager>
{
    Socket socket;
    public Action<SocketMessage> messageHandler;
    public static Func<Task> onAuthProcessAsync;
    public static string authPath;

    public void RegisterHandler(Action<SocketMessage> handler)
    {
        if(this.messageHandler != null)
        {
            throw new Exception("[SocketManager] message handler is already registered");
        }
        this.messageHandler = handler;
    }

    bool isAuthed;
    public async Task ConnectAsync(IPEndPoint endpoint)
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        this.socket = socket;
        Debug.Log($"[SocketManager] socket connecting {endpoint} ...");
        await socket.ConnectAsync(endpoint);
        Debug.Log("[SocketManager] socket connected, process auth...");
        StartReceiveLoop();
        if(onAuthProcessAsync != null)
        {
            await onAuthProcessAsync.Invoke();
        }
        isAuthed = true;
        Debug.Log("[SocketManager] socket auth success");
    }

    public event Action Disconencted;

    public void Disconnect()
    {
        if(this.socket == null)
        {
            return;
        }
        this.socket.Dispose();
        this.socket = null;
        this.isAuthed = false;
        Debug.Log("[SocketManager] disconnect");
        Disconencted?.Invoke();
    }

    public bool IsReady
    {
        get 
        {
            if(this.socket == null)
            {
                return false;
            }
            if(!this.socket.Connected)
            {
                return false;
            }
            if (!this.isAuthed)
            {
                return false;
            }
            return true;
        }
    }

    bool SocketConnected
    {
        get
        {
            if (this.socket == null)
            {
                return false;
            }
            if (!this.socket.Connected)
            {
                return false;
            }
            return true;
        }
    }
    static bool IsMessageNeedAuth(SocketMessage msg)
    {
        var path = msg.NonArgPath;
        if(path == authPath)
        {
            return false;
        }
        return true;
    }

    public void TrySend(SocketMessage msg)
    {
        var needAuth = IsMessageNeedAuth(msg);
        if (needAuth)
        {
            if (!IsReady)
            {
                Debug.Log($"[SocketManager] msg {msg} not send, because not connected or not authed");
                return;
            }
        }
        else
        {
            if(!SocketConnected)
            {
                Debug.Log($"[SocketManager] msg {msg} not send, because socket not connected");
                return;
            }
        }
        if(msg.NonArgPath != heartBeatPath)
        {
            Debug.Log($"[SocketManager] send  >>  {msg}");
        }
        var data = SocketMessageUtil.Serilize(msg);
        
        try
        {
            this.socket.Send(data);
        }
        catch(Exception e)
        {
            Debug.Log("[SocketManager] send catch exception: " + e.Message);
            this.Disconnect();
        }
    }

    void OnDestroy()
    {
        this.Disconnect();
    }

    async void StartReceiveLoop()
    {
        try
        {
            Debug.Log("[SocketManager] start receive loop");
            var tempBuffer = new byte[1024];
            var cachedBuffer = new CircularBuffer<byte>(1024 * 1024);
            SocketMessageHead? head = null;
            var headSize = SocketMessageHead.Size;
            while (true)
            {
                var cachedCount = cachedBuffer.Size;
                if (head == null)
                {
                    if (cachedCount >= headSize)
                    {

                        cachedBuffer.Read(tempBuffer, headSize);
                        head = SocketMessageUtil.Deserilize(tempBuffer);
                        //Debug.Log($"[SocketManager] parse head: {head}");
                        continue;
                    }
                }
                else
                {
                    var dataLength = head.Value.path + head.Value.body;
                    if (cachedCount >= dataLength)
                    {
                        //Debug.Log($"[SocketManager] parse path + body : {dataLength}");
                        var dataBuffer = new byte[dataLength];
                        cachedBuffer.Read(dataBuffer, (int)dataLength);
                        var msg = new SocketMessage();
                        msg.head = head.Value;
                        msg.data = dataBuffer;
                        head = null;

                        // 心跳是内部消息，不上报
                        if (msg.NonArgPath != heartBeatPath)
                        {
                            Debug.Log($"[SocketManager] recv  <<  {msg}");
                            this.messageHandler?.Invoke(msg);
                        }
                        continue;
                    }
                }
                var seg = new ArraySegment<byte>(tempBuffer);
                if (socket == null) return;
                
                var count = await socket.ReceiveAsync(seg, SocketFlags.None);
                if (socket == null)
                {
                    Debug.Log("[SocketManager] receive loop stoped, because disconnect() called");
                    this.Disconnect();
                    return;
                }
                if (count == 0)
                {
                    // 远端断开连接
                    Debug.Log("[SocketManager] receive loop stoped, because remote disconnected");
                    this.Disconnect();
                    return;
                }
                //Debug.Log($"[SocketManager] receive bytes: {count}");
                for (int i = 0; i < count; i++)
                {
                    var b = tempBuffer[i];
                    cachedBuffer.PushBack(b);
                }
            }
        }
        catch(Exception e)
        {
            Debug.LogError(e.GetBaseException());
            Debug.Log("[SocketManager] receive loop stoped, because read error");
        }
        finally
        {
            this.Disconnect();
        }
        
    }

    static string heartBeatPath = "C2SHeartbeat";
    double lastSendTime = 0;
    void Update()
    {
        //"C2SHeartbeat"
        if(this.IsReady)
        {
            var now = Time.realtimeSinceStartupAsDouble;
            var lost = now - lastSendTime;
            if (lost >= 1)
            {
                var msg = SocketMessageUtil.Create(heartBeatPath);
                this.TrySend(msg);
                lastSendTime = now;
            }
        }
    }
}