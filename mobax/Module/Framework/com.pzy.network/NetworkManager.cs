using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomLitJson;
using UnityEngine;
using Sirenix.OdinInspector;

public class NetworkManager : StuffObject<NetworkManager>, IUpdatable 
{
    private readonly List<NetworkRoutine> readonlyOperationProcessingRoutineList = new List<NetworkRoutine> ();

    // 需要发送的写操作队列
    private readonly List<NetworkRoutine> writeOperationProcessingRoutingList = new List<NetworkRoutine>();
    private readonly List<NetworkRoutine> writeOperationWillSendList = new List<NetworkRoutine>();

    private readonly List<NetworkRoutine> compeltedRoutingList = new List<NetworkRoutine>();

    //[CustomReadOnly]
    public int sendCount = 0;
    public static Action<NetworkResult, NetworkRoutine> onAnyRoutingSuccess;
    public static Action<bool> changeRequestBlock;
    public static Action anyRequestSuspended;
    public static event Action<NetworkRoutine> addExtraParamHandler;

    public static void InvokeAddExtraParamHanlder(NetworkRoutine routing)
    {
        addExtraParamHandler?.Invoke(routing);
    }

    [Button]
    /// <summary>
    /// 仅返回ret信息，将 ret 字段下的内容转换为 T 类型后返回
    /// </summary>
    public async Task<JsonData> CallLagacyAsync(ServerType serverType, string api, Dictionary<string, string> postArgs = null, Dictionary<string, string> urlParam = null, bool isBlock = true)
    {
        var jd = new JsonData();
        foreach (var kv in postArgs)
        {
            jd[kv.Key] = kv.Value;
        }
        var msg = await CallAsync<JsonData>(serverType, api, jd, urlParam, isBlock);
        return msg;
    }



    /// <summary>
    /// 仅返回ret信息，将 ret 字段下的内容转换为 T 类型后返回
    /// </summary>
    public async Task<JsonData> CallAsync(ServerType serverType, string api, JsonData arg = null, Dictionary<string, string> urlParam = null, bool isBlock = true, DisplayType isAutoShowReward = DisplayType.None, string goods = "", bool repreatWhenNetError = true)
    {
        var msg = await CallAsync<JsonData>(serverType, api, arg, urlParam, isBlock, isAutoShowReward, null, goods, repreatWhenNetError);
        return msg;
    }


    /// <summary>
    /// 仅返回ret信息，将 ret 字段下的内容转换为 T 类型后返回
    /// </summary>
    public async Task<T> CallAsync<T> (ServerType serverType, string api, JsonData arg = null, Dictionary<string, string> urlParam = null, bool isBlock = true, DisplayType isAutoShowReward = DisplayType.None, bool? isReadonly = null, string goods = "", bool repreatWhenNetError = true) 
    {
        var msg = await CallAndGetMsgAsync<T> (serverType, api, arg, urlParam, isBlock, isAutoShowReward, isReadonly, goods, repreatWhenNetError);
        var ret = msg.data;
        return ret;
    }

    /// <summary>
    /// 返回 data 信息和 cache，将 ret 字段下的内容转换为 T 类型后返回
    /// </summary>
    public async Task<(T, List<JsonData>)> CallAndGetCacheAsync<T>(ServerType serverType, string api, JsonData arg = null, Dictionary<string, string> urlParam = null, bool isBlock = true)
    {
        var msg = await CallAndGetMsgAsync<T>(serverType, api, arg, urlParam, isBlock);
        var ret = msg.data;
        var cache = msg.cache;
        return (ret, cache);
    }


    public static Func<string, bool> isFateErrorHandler;

    /// <summary>
    /// 返回完整消息体，将 ret 字段下的内容转换为 T 类型后返回
    /// </summary>
    public async Task<NetMsg<T>> CallAndGetMsgAsync<T> (ServerType serverType, string api, JsonData arg = null, Dictionary<string, string> urlParam = null, bool isBlock = true, DisplayType isAutoShowReward = DisplayType.None, bool? isReadonly = null, string goods = "", bool repreatWhenNetError =  true) 
    {
        //Debug.Log($"[NetworkManager] {serverType} {api}");

        var result = await RoutingRpcAsync(serverType, api, arg, urlParam, isBlock, isAutoShowReward, false, isReadonly, goods, repreatWhenNetError);

        // 如果有任何传输异常
        if(result.exception != null)
        {
            throw result.exception;
        }

        var msgWithJsonData = result.msgWithDataTypeIsJsonData;
        var isSuccess = msgWithJsonData.IsSuccess;
        if (!isSuccess)
        {
            //var err = result.msgWithDataTypeIsJsonData.err;
            var serverCode = msgWithJsonData.code.ToString();
            var data = result.msgWithDataTypeIsJsonData.data;
            var serverMsg = data.ToString();
            var developerMsg = $"server refused\n api: {api}\nmsg: {serverMsg}";
            var code = $"server_refused_{serverCode}";

            var isFatalError = isFateErrorHandler?.Invoke(code);
            var flag = ExceptionFlag.None;
            if(isFatalError!= null && isFatalError.Value == true)
            {
                flag = ExceptionFlag.Logout;
            }
            //var e = new ServerRefuseException(serverMsg, api);
            var e = new GameException(flag, developerMsg, code);
            throw e;
            //throw new ServerRefuseException ("", data.ToString ());
        }
        var retType = typeof(T);
        if (retType == typeof(JsonData))
        {
            return msgWithJsonData as NetMsg<T>;
        }
        else
        {
            var stringMsg = result.text;
            //var msgDefineType = typeof(NetMsg<>);
            //var msgGenericType = msgDefineType.MakeGenericType(retType);
            //var msg = NetworkUtil.ToObject(msgGenericType, stringMsg) as NetMsg<T>;

            var msg = JsonMapper.Instance.ToObject<NetMsg<T>>(stringMsg);

            return msg;
        }


    }


    public async Task<string> CallGetPureTextAsync(ServerType serverType, string api, JsonData arg = null, Dictionary<string, string> urlParam = null, bool isBlock = true, bool? isReadonly = null)
    {
        var result = await RoutingRpcAsync(serverType, api, arg, urlParam, isBlock, DisplayType.None, true, isReadonly);
        var text = result.text;
        return text;
    }


    /// <summary>
    /// 返回完整消息体，当请求成功时返回对象类型为 NetMsg<T>，当失败时返回对象类型为 NetMsg<string>
    /// 可以通过 IsSuccess 属性判断请求是否为成功返回. *注意：这里的失败指的是服务器明确返回的逻辑失败，
    /// 并不包含网络问题导致的 http 失败。网络问题导致失败时，底层会统一提示用户重试，或者重置游戏。
    /// </summary>
    public async Task<NetBaseMsg> CallAllowLogicFailAsync<T> (ServerType serverType, string api, JsonData arg = null, Dictionary<string, string> urlParam = null, bool isBlock = true, DisplayType isAutoShowReward = DisplayType.None)
    {
        var result = await RoutingRpcAsync (serverType, api, arg, urlParam, isBlock, isAutoShowReward);
        var msgWithJsonData = result.msgWithDataTypeIsJsonData;
        var isSuccess = msgWithJsonData.IsSuccess;
        if (!isSuccess) 
        {
            var failMsg = new NetMsg<string> ();
            //failMsg.err = msgWithJsonData.err;
            failMsg.cache = msgWithJsonData.cache;
            failMsg.data = msgWithJsonData.data.ToString ();
            failMsg.time = msgWithJsonData.time;
            failMsg.code = msgWithJsonData.code;
            return failMsg;
        } 
        else
        {
            var retType = typeof (T);
            if (retType == typeof (JsonData)) 
            {
                return msgWithJsonData;
            } 
            else 
            {
                var stringMsg = result.text;
                var msgDefineType = typeof (NetMsg<>);
                var msgGenericType = msgDefineType.MakeGenericType (retType);
                var msg = NetworkUtil.ToObject (msgGenericType, stringMsg) as NetMsg<T>;
                return msg;
            }
        }
    }

    public static Func<ServerType, string, string> OnGetUrl;

    public string GetUrl(ServerType serverType, string api)
    {
        if(OnGetUrl == null)
        {
            throw new Exception("[NetworkManager] OnGetUrl not set yet");
        }
        var ret = OnGetUrl.Invoke(serverType, api);
        return ret;
    }


    Task<NetworkResult> RoutingRpcAsync (ServerType serverType, string api, JsonData postData, Dictionary<string, string> urlParam, bool isBlock, DisplayType isAutoShowReward = DisplayType.None, bool isPureText = false, bool? isReadonly = false, string goods = "", bool repreatWhenNetError = true) 
    {
        var url = GetUrl (serverType, api);
        var tcs = new TaskCompletionSource<NetworkResult> ();

        var routine = NetworkRoutinePool.Reuse ();
        if(isReadonly == null)
        {
            isReadonly = IsReadonlyOperation(api);
        }

        routine.Reset (tcs, serverType, url, postData, urlParam, isBlock, isReadonly.Value, isPureText, isAutoShowReward, goods, repreatWhenNetError);

        this.AddToMatchedInputQueue(routine);
        this.EnableUpdate = true;
        this.RefreshBlock();
        return tcs.Task;
    }

    // 加入到正确的管线处理入口
    void AddToMatchedInputQueue(NetworkRoutine r)
    {
        var isReadonly = r.isReadonly;
        if(isReadonly)
        {
            this.readonlyOperationProcessingRoutineList.Add(r);
            r.PostRequest();
        }
        else
        {
            this.writeOperationWillSendList.Add(r);
        }
    }

    bool IsReadonlyOperation(string api)
    {
        if(api.Contains("getter"))
        {
            return true;
        }

        return false;
    }

    public void CancelAll () 
    {
        foreach (var routine in this.readonlyOperationProcessingRoutineList) 
        {
            routine.TerminalWithCancel ();
        }
        foreach (var routine in this.writeOperationProcessingRoutingList)
        {
            routine.TerminalWithCancel();
        }
        for (var i = writeOperationWillSendList.Count - 1; i >= 0; i--)
        {
            var routine = writeOperationWillSendList[i];
            var tcs = routine.tcs;
            var e = new GameException(ExceptionFlag.Silent, "networkroutine canceled");
            tcs.SetException(e);
            writeOperationWillSendList.RemoveAt(i);
            NetworkRoutinePool.Recycle(routine);
        }
        DisposeAllSuspendedReqeust ();
    }

    public void RepostAllSuspendedRequest () 
    {
        for (var i = suspededRoutineList.Count - 1; i >= 0; i--) 
        {
            var routine = suspededRoutineList[i];
            suspededRoutineList.RemoveAt(i);
            this.AddToMatchedInputQueue(routine);
        }
        this.EnableUpdate = true;
    }

    void RefreshBlock()
    {
        var shouldBlock = this.HasAnyBlockRouting();
        this.IsBlocked = shouldBlock;
    }

    bool HasAnyBlockRouting()
    {
        foreach(var one in this.readonlyOperationProcessingRoutineList)
        {
            var isBlock = one.isBlock;
            if(isBlock)
            {
                return true;
            }
        }
        foreach (var one in this.writeOperationWillSendList)
        {
            var isBlock = one.isBlock;
            if (isBlock)
            {
                return true;
            }
        }
        foreach (var one in this.writeOperationProcessingRoutingList)
        {
            var isBlock = one.isBlock;
            if (isBlock)
            {
                return true;
            }
        }
        foreach (var one in this.compeltedRoutingList)
        {
            var isBlock = one.isBlock;
            if (isBlock)
            {
                return true;
            }
        }
        return false;
    }

    bool _enableUpdate;
    bool EnableUpdate
    {
        set
        {
            if(_enableUpdate == value)
            {
                return;
            }
            _enableUpdate = value;
            if(value)
            {
                UpdateManager.Stuff.Add(this);
                //this.IsBlocked = true;
            }
            else
            {
                UpdateManager.Stuff.Remove(this);
                this.IsBlocked = false;
            }
        }
    }

    bool _isBlocked;
    bool IsBlocked
    {
        set
        {
            if(value == _isBlocked)
            {
                return;
            }
            //Debug.Log("block: " + value);
            _isBlocked = value;
            changeRequestBlock?.Invoke(value);
        }
    }

    public void DisposeAllSuspendedReqeust () 
    {
        for (var i = suspededRoutineList.Count - 1; i >= 0; i--) 
        {
            var routine = suspededRoutineList[i];
            var tcs = routine.tcs;
            var e = new GameException(ExceptionFlag.Silent, "networkroutine canceled");
            tcs.SetException (e);
            suspededRoutineList.RemoveAt (i);
            NetworkRoutinePool.Recycle (routine);
        }
    }

    void MoveAllCompletedRouitngToCompletedList(List<NetworkRoutine> routingList)
    {
        for (var i = routingList.Count - 1; i >= 0; i--)
        {
            var one = routingList[i];
            if (one.state != RoutineState.Processing)
            {
                routingList.RemoveAt(i);
                compeltedRoutingList.Add(one);
            }
        }
    }

    void ProcessCompeltedList()
    {
        for (var i = compeltedRoutingList.Count - 1; i >= 0; i--)
        {
            var routine = compeltedRoutingList[i];
            var state = routine.state;

            if (state == RoutineState.Sucess)
            {
                // 成功取回数据
                compeltedRoutingList.RemoveAt(i);
                NetworkRoutinePool.Recycle(routine);

                var tcs = routine.tcs;
                var result = routine.result;
                Exception autoProcessError = null;
                try
                {
                    onAnyRoutingSuccess?.Invoke(result, routine);
                }
                catch(Exception e)
                {
                    autoProcessError = e;
                }
              
                if(autoProcessError != null)
                {
                    tcs.SetException(autoProcessError);
                }
                else
                {
                    tcs.SetResult(result);
                }
            }
            else if (state == RoutineState.HttpServerRefused)
            {
                // 服务器底层 web 服务器相应拒绝
                // 这是预期之外的情况，可能是服务器部署错误

                var tcs = routine.tcs;
                var result = routine.result;
                var e = result.exception;
                tcs.SetException(e);
                compeltedRoutingList.RemoveAt(i);
                NetworkRoutinePool.Recycle(routine);
            }
            else if (state == RoutineState.HttpConnectionError)
            {
                // 没有连接到服务器
                // 可能是网络波动
                // 自动重试，如果仍然无法解决需要提示用户无网络

                var b = routine.repreatWhenNetError;

                if(b)
                {
                    compeltedRoutingList.RemoveAt(i);
                    suspededRoutineList.Add(routine);
                    anyRequestSuspended?.Invoke();
                }
                else
                {
                    var tcs = routine.tcs;
                    var e = new Exception("Http Connection Error");
                    tcs.SetException(e);
                    compeltedRoutingList.RemoveAt(i);
                    NetworkRoutinePool.Recycle(routine);
                }
            }
            else if (state == RoutineState.ReponseParseError)
            {
                var tcs = routine.tcs;
                var result = routine.result;
                var e = result.exception;
                tcs.SetException(e);
                compeltedRoutingList.RemoveAt(i);
                NetworkRoutinePool.Recycle(routine);

            }
            else if (state == RoutineState.Canceled)
            {
                // 请求被用户取消
                // 请求通常是不能被取消的
                // 由于是用户自己取消的，因此不用再进行任何 UI 通知

                var tcs = routine.tcs;
                tcs.SetException(new GameException(ExceptionFlag.Silent, "user canceld"));
                compeltedRoutingList.RemoveAt(i);
                NetworkRoutinePool.Recycle(routine);
            }
        }
    }

    bool IsAllPiplineListClean()
    {
        if (readonlyOperationProcessingRoutineList.Count != 0)
        {
            return false;
        }
        else if (writeOperationProcessingRoutingList.Count != 0)
        {
            return false;
        }
        else if (writeOperationWillSendList.Count != 0)
        {
            return false;
        }
        else if(compeltedRoutingList.Count != 0)
        {
            return false;
        }
        return true;

    }

    void UpdateAllProcessingList()
    {
        foreach (var routine in this.readonlyOperationProcessingRoutineList)
        {
            routine.Update();
        }
        foreach (var routine in this.writeOperationProcessingRoutingList)
        {
            routine.Update();
        }
    }

    void MoveNextWriteRoutingToProcessQueue()
    {
        if(writeOperationWillSendList.Count == 0)
        {
            return;
        }
        if(writeOperationProcessingRoutingList.Count == 0)
        {
            var nextRouting = writeOperationWillSendList[0];
            writeOperationWillSendList.RemoveAt(0);
            nextRouting.PostRequest();
            writeOperationProcessingRoutingList.Add(nextRouting);
        }
    }

    List<NetworkRoutine> suspededRoutineList = new List<NetworkRoutine> ();
    float t = 0;
    public void OnUpdate () 
    {
        t += Time.deltaTime;
        if (t < 0.1f)
        {
            return;
        }
        t = 0;

        // 如果写操作任务可以处理下一个
        this.MoveNextWriteRoutingToProcessQueue();

        // 更新所有正在处理的任务
        this.UpdateAllProcessingList();

        // 如果任何任务已完成，则将其移动到已完成队列
        this.MoveAllCompletedRouitngToCompletedList(this.readonlyOperationProcessingRoutineList);
        this.MoveAllCompletedRouitngToCompletedList(this.writeOperationProcessingRoutingList);

        // 处理已完成队列
        this.ProcessCompeltedList();

        this.RefreshBlock();

        var isPiplineEmpty = IsAllPiplineListClean();
        if (isPiplineEmpty) 
        {
            this.EnableUpdate = false;
        }
    }
}