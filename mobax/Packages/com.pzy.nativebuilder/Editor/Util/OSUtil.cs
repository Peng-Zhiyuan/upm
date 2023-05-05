using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Threading;

namespace NativeBuilder{

	public enum Platform{
		Windows,
		Mac,
	}
	
	public abstract class OSUtil
	{

		private static OSUtil _instance;
		public static OSUtil Instance
		{
			get
			{
				if(_instance == null)
				{
					_instance = GetNewUtilByOS();
				}
				return _instance;
			}
		}

		public static Platform Platform{

			get{

				return OSUtil.Instance._Platform;
			}
		}

		private static OSUtil GetNewUtilByOS()
		{
			PlatformID platfrom = System.Environment.OSVersion.Platform;
			switch(platfrom)
			{
			case PlatformID.MacOSX:
				return new MacUtil();
			case PlatformID.Unix:
				return new MacUtil();
			default:
				return new WindowsUtil();
			}
		}

		public abstract Platform _Platform{get;}
	}
	

	public class WindowsUtil : OSUtil
	{
		public override Platform _Platform{
			get
			{
				return Platform.Windows;
			}
		}

	}

	public class MacUtil : OSUtil
	{
		public override Platform _Platform{
			get
			{
				return Platform.Mac;
			}
		}
	


	}
}
