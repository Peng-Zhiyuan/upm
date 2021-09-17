using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using UnityEditor;
using System.Diagnostics;
using System.Linq;

namespace NativeBuilder 
{
	
	public class Exec  {
		
		/// <summary>
		/// Run the specified exec, argument, ignoreError and output.
		/// </summary>
		/// <param name="exec">Exec.</param>
		/// <param name="argument">Argument.</param>
		/// <param name="ignoreError">treat error log as normal log</param>
		/// <param name="output">all normal log and error log</param>
		/// 
		public static int Run(string exec, string argument, out string output, bool ignoreError = false, bool hasOutput = true)
		{
			string s = "";
			System.Diagnostics.Process p = new System.Diagnostics.Process ();
			p.StartInfo.FileName = exec;
			p.StartInfo.Arguments = argument;
			UnityEngine.Debug.Log("execute:  " + exec + " " + argument);
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.RedirectStandardInput = true;
			p.StartInfo.RedirectStandardOutput = true;
			
			p.OutputDataReceived += (object sender, DataReceivedEventArgs e) => 
			{
				if(e.Data == null) return;
				// print log
				UnityEngine.Debug.Log(e.Data);
				//export log
				//string path = "/Users/mengmin.duan/Project/LCM/core/unity/dev/nativebuilder_log/build.log";
				
				
				// get output
				if(hasOutput)
				{
					s += e.Data.ToString() + "\n";
				}
			};
			p.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => 
			{
				if(e.Data == null) return;
				// print log
				if(!ignoreError)
				{
					UnityEngine.Debug.LogError(e.Data);
				}
				else
				{
					UnityEngine.Debug.Log(e.Data);
				}
				// get output
				if(hasOutput)
				{
					s += e.Data.ToString() + "\n";
				}
				
			};
			p.Start ();
			
			
			p.BeginOutputReadLine ();
			p.BeginErrorReadLine();

			var temp = Application.stackTraceLogType;
			Application.stackTraceLogType = StackTraceLogType.None;
			p.WaitForExit();
			Application.stackTraceLogType = temp;

			output = s;
			return p.ExitCode;
			
		}
		
		public static int Run(string exec, string argument, bool ignoreError = false)
		{
			string ret = null;
			return Run(exec, argument, out ret, ignoreError, false);
		}
		
		//	public static int RunEx(string Exec, bool ignoreError, params string[] arguments)
		//	{
		//		var enumator = from arg in arguments select EscapeBlank(arg);
		//		var escapedArgument = string.Join(" ", enumator.ToArray());
		//		return Run(Exec, escapedArgument, ignoreError);
		//	}
		
		public static string RunGetOutput(string exec, string argument, bool ignoreError = false)
		{
			string ret = "";
			Run(exec, argument, out ret, ignoreError, true);
			return ret;
		}
		
		//	public static string EscapeBlank(string origin)
		//	{
		//		return origin.Replace(" ", @"\ ");
		//		//return origin;
		//
		//	}
	}
}

