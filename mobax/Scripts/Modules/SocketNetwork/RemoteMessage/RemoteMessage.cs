using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Text;

public class RemoteMessage : StuffObject<RemoteMessage>
{
    Dictionary<string, Action<SocketMessage>> msgToHandlerDic = new Dictionary<string, Action<SocketMessage>>();

    void Awake()
    {
        SocketManager.Stuff.RegisterHandler(OnNetMessageReceived);
    }

    void OnNetMessageReceived(SocketMessage msg)
    {
        // check wating tcs
        var isDispached = TryInvokeWatingTcs(msg);
        if(!isDispached)
        {
            DispatchForRegisterdMethod(msg);
        }
    }

    public void Register<T>()
    {
        var type = typeof(T);
        var methodList = GetHandlerMethodListInClass(type, true);
        this.AddHandlerList(null, methodList);
    }

    public void Register(object obj)
    {
        var type = obj.GetType();
        var methodList = GetHandlerMethodListInClass(type, false);
        this.AddHandlerList(obj, methodList);
    }

    public void Unregister<T>()
    {
        var type = typeof(T);
        var methodList = GetHandlerMethodListInClass(type, true);
        this.RemoveHandlerList(methodList);
    }

    public void Unregister(object obj)
    {
        var type = obj.GetType();
        var methodList = GetHandlerMethodListInClass(type, false);
        this.RemoveHandlerList(methodList);
    }

    void DispatchForRegisterdMethod(SocketMessage msg)
    {
        var methodName = msg.NonArgPath;
        var found = msgToHandlerDic.TryGetValue(methodName, out var handler);
        if(!found)
        {
            Debug.Log($"[RemoteMessage] msg {msg.FullPath} not found handler");
            return;
        }
        handler.Invoke(msg);
    }

    void RemoveHandlerList(List<MethodInfo> methodList)
    {
        foreach(var method in methodList)
        {
            var name = method.Name;
            this.msgToHandlerDic.Remove(name);
        }
    }

    void AddHandlerList(object obj, List<MethodInfo> methodInfo)
    {
        foreach(var method in methodInfo)
        {
            var path = method.Name;
            var attribute = method.GetCustomAttribute<MessageHandlerAttribute>();
            if (!string.IsNullOrEmpty(attribute.path))
            {
                path = attribute.path;
            }
            if(this.msgToHandlerDic.ContainsKey(path))
            {
                throw new Exception($"[RemoteMessage] handler for msg `{path}` aldready exsits");
            }
            Action<SocketMessage> action = msg =>
            {
                method.Invoke(obj, new object[] { msg });
            };
            msgToHandlerDic[path] = action;
        }
    }

    static List<MethodInfo> GetHandlerMethodListInClass(Type type, bool isStatic)
    {
        var param = BindingFlags.Public | BindingFlags.NonPublic;
        if(isStatic)
        {
            param |= BindingFlags.Static;
        }
        else
        {
            param |= BindingFlags.Instance;
        }
        var methodList = type.GetMethods(param);
        var handlerMethodList = GetHandlerMethodList(methodList);
        return handlerMethodList;
    }

    static List<MethodInfo> GetHandlerMethodList(MethodInfo[] methodList)
    {
        var list = new List<MethodInfo>();
        foreach(var method in methodList)
        {
            var attribute = method.GetCustomAttribute<MessageHandlerAttribute>();
            if(attribute != null)
            {
                list.Add(method);
            }
        }
        return list;
    }

    public void TrySend(string path, Dictionary<string, string> arg = null, byte[] body = null)
    {
        var msg = SocketMessageUtil.Create(path, arg, body);
        SocketManager.Stuff.TrySend(msg);
    }

    Dictionary<string, TaskCompletionSource<SocketMessage>> ridToTcs = new Dictionary<string, TaskCompletionSource<SocketMessage>>();

    static int _nextRid;
    static int NextRid
    {
        get
        {
            return _nextRid++;
        }
    }

    public Task<SocketMessage> CallAsync(string path, Dictionary<string, string> arg = null, byte[] body = null)
    {
        if (arg == null)
        {
            arg = new Dictionary<string, string>();
        }
        var rid = NextRid.ToString();
        arg["rid"] = rid;
        var msg = SocketMessageUtil.Create(path, arg, body);
        SocketManager.Stuff.TrySend(msg);
        var tcs = new TaskCompletionSource<SocketMessage>();
        ridToTcs[rid] = tcs;
        return tcs.Task;
    }

    bool TryInvokeWatingTcs(SocketMessage msg)
    {
        var arg = msg.Arg;
        var b = arg.TryGetValue("rid", out var rid);
        if(b)
        {
            var bb = ridToTcs.TryGetValue(rid, out var tcs);
            if(bb)
            {
                ridToTcs.Remove(rid);
                var code = msg.Code;
                if(code == 0)
                {
                    tcs.SetResult(msg);
                }
                else
                {
                    var body = msg.Body;
                    var error = Encoding.Default.GetString(body);
                    var errorMessage = $"code: {code}, msg: {error}";
                    var e = new Exception(error);
                    tcs.SetException(e);
                }
                return true;
            }
        }
        return false;
    }
}
