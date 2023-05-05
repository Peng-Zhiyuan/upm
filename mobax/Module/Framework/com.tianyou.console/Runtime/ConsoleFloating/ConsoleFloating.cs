using UnityEngine;
using System;
using System.Reflection;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections.Generic;


public partial class ConsoleFloating : Floating
{

	public MainConsoleWind mainConsoleWind;

	public static Type uiCommandProvider;

	public void Awake()
	{
		this.Content.ViewSetter = OnSetCommandButton;
		this.Refresh();
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
		var type = uiCommandProvider;
		var originArrary = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
		if (this.Content != null)
		{
			this.Content.DataList = new List<MethodInfo>(originArrary);
		}

		
	}

	public void OnButton(string msg)
	{
		if(msg == "close")
		{
			UIEngine.Stuff.RemoveFloating<ConsoleFloating>();
		}
	}
}


