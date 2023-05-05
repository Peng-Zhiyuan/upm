using UnityEngine;
using System;
using System.Reflection;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.Rendering;

public partial class PerformanceFloating : Floating
{
 
	public MainConsoleWind mainConsoleWind;
	//private BatchRendererGroup batchRendererGroup;

	public static Type performanceCommandProvider = typeof(PerformanceCommand);

	public void Awake()
	{
		this.Content.ViewSetter = OnSetCommandButton;
		this.Refresh();
		//batchRendererGroup = this.GetComponent<BatchRendererGroup>();
	}

    public void Update()
    {
		var fps = Mathf.RoundToInt(1f / Time.deltaTime);


		// 获取CPU使用率
		//var cpuUsage = Mathf.RoundToInt((float)System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds);
		//int drawCalls = UnityEngine.Rendering.Graphics.activeDrawCallCount;
		//int drawCalls = UnityEngine.Rendering.RenderPipelineManager.currentPipeline.dr;
		// 更新UI元素
		FPS.text = "FPS: " + fps;
		//CPU.text = "CPU: " + cpuUsage + "ms";
		//GPU.text = 
	}

    void OnSetCommandButton(object data, Transform view)
	{
		var method = data as MethodInfo;
		var commandView = view.GetComponent<CommandView>();
		commandView.Bind(method);
	}


	void Refresh()
	{
		this.RefreshButton();
	}

	private void RefreshButton()
	{
		var type = performanceCommandProvider;
		var originArrary = type.GetMethods(BindingFlags.Static | BindingFlags.Public);

		this.Content.DataList = new List<MethodInfo>(originArrary);
	}

	public void OnButton(string msg)
	{
		if (msg == "close")
		{
			UIEngine.Stuff.RemoveFloating<PerformanceFloating>();
		}
	}
}
