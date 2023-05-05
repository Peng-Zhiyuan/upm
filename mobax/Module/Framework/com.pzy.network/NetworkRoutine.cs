using UnityEngine;
using System;
using System.Collections.Generic;
using CustomLitJson;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Text;

public enum RoutineState
{
	/// <summary>
	/// 正在处理
	/// </summary>
	Processing,

	/// <summary>
	/// 调用成功
	/// </summary>
	Sucess,

	/// <summary>
	/// Http 服务器拒绝
	/// </summary>
	HttpServerRefused,

	/// <summary>
	/// 网络连接错误
	/// </summary>
	HttpConnectionError,

    /// <summary>
    /// 返回内容 JSON 解析错误
    /// </summary>
    ReponseParseError,

	/// <summary>
	/// 被取消
	/// </summary>
	Canceled,
}
public class NetworkRoutine 
{
	public string url = null;
    private JsonData postArg = null;
    private UnityWebRequest request = null;
	private ServerType serverType;
	public bool isReadonly = false;
	public string goods = null;

	// 返回内容是纯文本，而不是json
	public bool isPureText = false;
	
	
    private float processingTime = 0;	


	public RoutineState state;

	public void OnReuse()
	{
	
	}

	public void OnRecycle()
	{
		this.DisposeWWW();
	}

	private void DisposeWWW()
	{
		if(request != null)
		{
			request.Dispose();
			request = null;
		}
	}

    private static int sq = -1;
	public bool isBlock;
	public TaskCompletionSource<NetworkResult> tcs;
	public NetworkResult result;
	public Dictionary<string, string> urlParam = new Dictionary<string, string>();
	public DisplayType isAutoShowReward;
	public bool repreatWhenNetError;
	public void Reset(TaskCompletionSource<NetworkResult> tcs, ServerType serverType, string url, JsonData postArg, Dictionary<string, string> urlParam, 
		bool isBlock, bool isReadonly, bool isPureText = false, DisplayType isAutoShowReward = DisplayType.None, string goods = "", bool repreatWhenNetError = true)
	{
		this.repreatWhenNetError = repreatWhenNetError;
		this.isPureText = isPureText;
		this.isReadonly = isReadonly;
		this.isAutoShowReward = isAutoShowReward;
		this.goods = goods;
		if(postArg == null)
		{
			postArg = new JsonData();
		}
		if(sq == -1)
        {
            sq = (int)UnityEngine.Random.Range(0, 10000);
        }
		this.url = url;
		this.postArg = postArg;
		//this.arg["_rid"] = sq++.ToString();
		this.urlParam.Clear();
		this.urlParam["_rid"] = sq++.ToString();
		if(urlParam != null)
        {
			foreach (var kv in urlParam)
			{
				this.urlParam["_rid"] = kv.Value;
			}
		}
		this.isBlock = isBlock;
		this.tcs = tcs;
		this.serverType = serverType;
	}


	public string FinalUrl
    {
		get
        {
			if(this.urlParam == null || this.urlParam.Count == 0)
            {
				return this.url;
            }
			else
            {
				var sb = new StringBuilder();
				sb.Append(this.url);
				sb.Append("?");
				var first = true;
				foreach(var kv in this.urlParam)
                {
					if(first)
                    {
						first = false;
                    }
					else
                    {
						sb.Append("&");
					}
					var key = kv.Key;
					var value = kv.Value;
					sb.Append(key);
					sb.Append("=");
					sb.Append(value);
                }
				return sb.ToString();
            }
			
        }
    }

	


	public void PostRequest()
	{
		NetworkManager.Stuff.sendCount++;
		DisposeWWW();


		NetworkManager.InvokeAddExtraParamHanlder(this);

		var finalUrl = this.FinalUrl;
		var postJson = "";
		var request = new UnityWebRequest(finalUrl, "POST");
		if (postArg != null && postArg.GetJsonType() != JsonType.None)
		{
			//var form = CreateForm(arg);
			//var bytes = form.data;
			postJson = JsonMapper.Instance.ToJson(postArg);
			var bytes = Encoding.UTF8.GetBytes(postJson);

			request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bytes);
			//var headers = form.headers;
		}
		//request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
		request.SetRequestHeader("Content-Type", "application/json");
        {
            var parts = finalUrl.Split('?');
            var url = parts[0];
            string param = "";

			// 频繁接口，不打印 log
			if(!url.EndsWith("/notify"))
            {
				if (parts.Length > 1)
				{
					param = parts[1];
					Debug.Log($"[SEND]  {this.serverType}   {this.ReadWrite}   {url}<color=#666666>?{param}</color>   {postJson}");
				}
				else
				{
					Debug.Log($"[SEND]  {this.serverType}   {this.ReadWrite}   {finalUrl}  {postJson}");
				}
			}
			
            //Debug.Log($"[SEND]  {this.serverType}   {this.api}  {postJson}");
			
        }
	
		 
		startTime = Time.time;
		processingTime = 0;
		state = RoutineState.Processing;

		request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
		this.request = request;
		this.request.SendWebRequest();

	}

	string ReadWrite
    {
		get
        {
			if(this.isReadonly)
            {
				return "Read";
            }
			else
            {
				return "Write";
            }
        }
    }
	
	float startTime=0.0f;
		
	public void Update()
	{
		if(state != RoutineState.Processing) return;

		if(request != null && request.isDone)
		{
			var result = request.result;
			if(result == UnityWebRequest.Result.Success)
            {
				// http 成功
				OnHttpSuccess();
			}
			else if(result == UnityWebRequest.Result.ConnectionError)
            {
				// http 网络连接错误
				OnHttpConnectionError();
			}
			else
            {
				// http 协议错误
				var code = (int)request.responseCode;
				var errorMsg = request.error;
				//var e = new Exception($"[NetworkRouting] server refused: {code} {msg}");
				var url = this.url;
				//var e = new ServerRefuseException(msg, api);
				var msg = $"http error\n url: {url}\nmsg: {errorMsg}";
				var e = new GameException(ExceptionFlag.None, msg, "HTTP_ERROR");
				Debug.LogError($"[Back] {this.serverType}  {this.ReadWrite}  {request.url}  {code} {errorMsg}");
				var networkResult = new NetworkResult();
				networkResult.exception = e;
				this.result = networkResult;
				this.state = RoutineState.HttpServerRefused;
			}
            
		}
		else
		{
			processingTime += Time.deltaTime;
            if(processingTime > NetworkConfiger.mTimeLimit)
			{
                OnTimeOut();
			}
		}
	}


	private void OnHttpSuccess()
	{
		string response = request.downloadHandler.text;
		var api = this.url;

		if(!api.EndsWith("/notify"))
        {
			Debug.Log($"[BACK]  {this.serverType}  {this.ReadWrite}   {api}  {(Time.time - startTime).ToString("0.0")}s : {response}");
		}

		var result = new NetworkResult();
		result.text = response;


		if(!this.isPureText)
        {
			// 经典 json 消息格式
			if (string.IsNullOrEmpty(response))
			{
				Debug.LogError($"[BACK] {this.serverType}  {this.ReadWrite}  {request.url} {(Time.time - startTime).ToString("0.0")}s : {response}");
				OnSuspend();
				return;
			}

			NetMsg<JsonData> msg = null;
			try
			{
				msg = NetworkUtil.MainJasonMapper().ToObject<NetMsg<JsonData>>(response);
				//var code = msg.code;
			}
			catch (Exception e)
			{
				result.exception = e;
				this.TerminalWithResponseParseError(result);
				return;
			}
			result.msgWithDataTypeIsJsonData = msg;
			TerminalWithSuccess(result);
		}
		else
        {
			TerminalWithSuccess(result);
		}


	}

	void TerminalWithSuccess(NetworkResult result)
	{
		this.result = result;
		this.state = RoutineState.Sucess;
	}

    void OnSuspend()
    {
		this.state = RoutineState.HttpConnectionError;
    }


	void TerminalWithResponseParseError(NetworkResult result)
	{
		this.result = result;
		this.state = RoutineState.ReponseParseError;
	}

	public void TerminalWithCancel()
	{
		this.state = RoutineState.Canceled;
	}



	public void OnHttpConnectionError()
	{
        Debug.LogError($"Http Connection Error: " + this.url);
		OnSuspend();
	}

	public void OnTimeOut()
	{
		Debug.LogError("HTTP TIME OUT");
		OnSuspend();
	}

}


