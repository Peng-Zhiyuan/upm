using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using UnityEditor;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public class Exec
{
		
	/// <summary>
	/// Run the specified exec, argument, ignoreError and output.
	/// </summary>
	/// <param name="exec">Exec.</param>
	/// <param name="argument">Argument.</param>
	/// <param name="ignoreError">treat error log as normal log</param>
	/// <param name="output">all normal log and error log</param>
	/// 
	public static Task<ExecResult> RunAsync(string exec, string argument, bool unityPrintOutput, bool ignoreError = false, bool collectOutput = true)
	{
		string s = "";
		System.Diagnostics.Process p = new System.Diagnostics.Process ();
		p.StartInfo.FileName = exec;
		p.StartInfo.Arguments = argument;
		if(unityPrintOutput)
        {
			UnityEngine.Debug.Log("execute:  " + exec + " " + argument);
		}

		p.StartInfo.UseShellExecute = false;
		p.StartInfo.RedirectStandardError = true;
		p.StartInfo.RedirectStandardInput = true;
		p.StartInfo.RedirectStandardOutput = true;
			
		p.OutputDataReceived += (object sender, DataReceivedEventArgs e) => 
		{
			if(e.Data == null) return;
			// print log
			if(unityPrintOutput)
            {
				var temp = Application.stackTraceLogType;
				Application.stackTraceLogType = StackTraceLogType.None;
				UnityEngine.Debug.Log(e.Data);
				Application.stackTraceLogType = temp;
			}
			
			//export log
			//string path = "/Users/mengmin.duan/Project/LCM/core/unity/dev/nativebuilder_log/build.log";
				
				
			// get output
			if(collectOutput)
			{
				s += e.Data.ToString() + "\n";
			}
		};
		p.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => 
		{
			if(e.Data == null) return;
			// print log
			if (unityPrintOutput)
			{
				if (!ignoreError)
				{
					UnityEngine.Debug.LogError(e.Data);
				}
				else
				{
					UnityEngine.Debug.Log(e.Data);
				}
			}
			// get output
			if(collectOutput)
			{
				s += e.Data.ToString() + "\n";
			}
				
		};


		var tcs = new TaskCompletionSource<ExecResult>();
		
		p.Exited += (sender, arg) =>
		{
			var exitCode = p.ExitCode;
			var output = s;
			var result = new ExecResult();
			result.ExitCode = exitCode;
			result.output = output;

			tcs.SetResult(result);
		};


		p.EnableRaisingEvents = true;

		p.Start();


		p.BeginOutputReadLine();
		p.BeginErrorReadLine();

	

		return tcs.Task;
			
	}
		
	public static Task<ExecResult> RunAsync(string exec, string argument, bool ignoreError = false)
	{
		return RunAsync(exec, argument, true, ignoreError, false);
	}
		
	//	public static int RunEx(string Exec, bool ignoreError, params string[] arguments)
	//	{
	//		var enumator = from arg in arguments select EscapeBlank(arg);
	//		var escapedArgument = string.Join(" ", enumator.ToArray());
	//		return Run(Exec, escapedArgument, ignoreError);
	//	}
		
	public static Task<ExecResult> RunGetOutput(string exec, string argument, bool ignoreError = false)
	{
		return RunAsync(exec, argument, false, ignoreError, true);
	}
		
	//	public static string EscapeBlank(string origin)
	//	{
	//		return origin.Replace(" ", @"\ ");
	//		//return origin;
	//
	//	}
}


public class ExecResult
{
	public int ExitCode;
	public string output;
}