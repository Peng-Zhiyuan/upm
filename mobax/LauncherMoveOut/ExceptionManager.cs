using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Text;
using BattleSystem;

public static class ExceptionManager
{
    public static Func<string, string> codeToUserMessageDelegate;

    public static void RegisterHook()
    {
        Application.logMessageReceived += OnLog;
    }

    public static void RemoveHook()
    {
        Application.logMessageReceived -= OnLog;
    }

    static void OnLog(string message, string stackTrace, LogType type)
    {
        if(type == LogType.Exception)
        {
            var ret = Split(message);
            var typeName = ret[0];
            var content = ret[1];
            if(typeName == nameof(GameException))
            {
                OnGameException(typeName, content, stackTrace);
            }
            else 
            {
                // 普通异常
                OnNormalException(typeName, content, stackTrace);
            }
        }
    }

    static void OnNormalException(string typeName, string content, string stackTrace)
    {
        UniversalExceptionProcess(ExceptionFlag.None, typeName, null, content, null, stackTrace);
    }

    static string CodeToUserMessage(string code)
    {
        var ret = codeToUserMessageDelegate?.Invoke(code);
        return ret;
    }

    static bool ShowDeveloperMsg
    {
        get
        {
            if(DeveloperLocalSettings.IsDevelopmentMode)
            {
                return true;
            }
            else
            {
                return EnvManager.GetConfigOfFinalEnv("exception.developerMsg", "false") == "true";
            }
        }
    }

    static async void UniversalExceptionProcess(ExceptionFlag flag, string exceptionTypeName, string code, string developerMsg, string generalUserExplation, string stackTrace)
    {
        // 如果标记为静默，则不进行任何处理
        if(flag == ExceptionFlag.Silent)
        {
            return;
        }

        if (flag == ExceptionFlag.Logout)
        {
            IggSdkManager.Stuff.CleanSdkSession();
        }

        // 如果有错误码
        if (!string.IsNullOrEmpty(code))
        {
            var explaination = CodeToUserMessage(code);
            // 如果有解释，只显示解释
            if (!string.IsNullOrEmpty(explaination))
            {
                await Dialog.ConfirmAsync("", explaination);
            }
            else
            {
                // 使用一般性解释，并显示错误码
                var generalMsg = "";
                if(!string.IsNullOrEmpty(generalUserExplation))
                {
                    generalMsg = generalUserExplation;
                }
                else
                {
                    generalMsg = "general_error_msg".Localize();
                }

                var str = $"{generalMsg}\n(code: {code})";
                if(ShowDeveloperMsg)
                {
                    str += "\n" + developerMsg;
                }
                await Dialog.ConfirmAsync("", str);
            }
        }
        else
        {
            // 如果没有错误码，显示异常信息
            if (ShowDeveloperMsg)
            {
                var data = new ExceptionReportData();
                data.type = exceptionTypeName;
                data.description = developerMsg;
                data.trace = stackTrace;
                await Dialog.ReportExceptionAsync(data);
            }
        }

        if(flag == ExceptionFlag.Logout)
        {
            IggSdkManager.Stuff.CleanSdkSession();
            Application.Quit();
            //LoginManager.Stuff.Logout();
        }
    }

    static void OnGameException(string typeName, string content, string stackTrace)
    {
        var ret = SplitGameExceptionDes(content);
        var flagString = ret[0];
        var code = ret[1];
        var developerMsg = ret[2];
        var generalUserMsg = ret[3];
        var flag = (ExceptionFlag)Enum.Parse(typeof(ExceptionFlag), flagString);
        UniversalExceptionProcess(flag, typeName, code, developerMsg, generalUserMsg, stackTrace);
    }


    static List<string> SplitGameExceptionDes(string msg)
    {
        var ret = new List<string>();
        var parts = Regex.Split(msg, @"\|:\|");
        foreach(var one in parts)
        {
            ret.Add(one);
        }
        return ret;
    }

    static List<string> Split(string msg)
    {
        var ret = new List<string>();
        var index = msg.IndexOf(':');
        var left = msg.Substring(0, index);
        var right = msg.Substring(index + 1, msg.Length - index - 1);
        ret.Add(left.Trim());
        ret.Add(right.Trim());
        return ret;
    } 
}
