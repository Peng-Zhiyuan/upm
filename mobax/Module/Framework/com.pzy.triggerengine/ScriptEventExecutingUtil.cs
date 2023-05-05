using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Text;

public static class ScriptEventExecutingUtil
{
    public static Func<string, List<ResolveResult>, Task<object>> ExecuteCommand;

    static async Task<object> InternalExecuteCommand(string command, List<ResolveResult> argList)
    {
        if (ExecuteCommand == null)
        {
            throw new Exception($"[Script] {nameof(ScriptEventExecutingUtil)}.{nameof(ExecuteCommand)} not set yet");
        }

        var task = ExecuteCommand.Invoke(command, argList);
        var ret = await task;
        return ret;
    }

    static string TokenListToString(List<Token> tokenList)
    {
        var sb = new StringBuilder();
        foreach (var token in tokenList)
        {
            sb.Append(token);
            sb.Append(' ');
        }

        return sb.ToString();
    }

    public static async Task<bool> ExecuteAsync(Trigger info, TriggerEngine engine)
    {
        try
        {
            Debug.Log("[Script] execute event: " + info.id);
            var block = info.doBlock;
            Debug.Log("[Script] block:\n" + block);
            var reader = new TokenReader(block);
            var skipElse = false;
            while (true)
            {
                var tokenList = reader.ReadNextTokenListTilLineEnd();
                if (tokenList == null || tokenList.Count == 0)
                {
                    Debug.Log("[Script] done");
                    return true;
                }

                var firstToken = tokenList[0];
                if (firstToken.type == TokenType.Comments)
                {
                    // 这一行是注释
                    continue;
                }

                // 这里需要把重置步数给置掉
                GuideManagerV2.Stuff.curTouchStep = 0;
                Debug.Log(string.Format("<color=#6495ED>{0}</color>", "[GuideExceptionStep] --> Reset CurTouchStep To 0"));
                Debug.Log("[Script] execute >>>> " + TokenListToString(tokenList));

                if (firstToken.type != TokenType.Word)
                {
                    throw new Exception($"[ScriptEventExecutingUtil] token '{firstToken.text}' shold be a word");
                }

                var command = firstToken.text;

                if (command == "wait")
                {
                    await ProcessWaitLineAsync(tokenList, engine);
                }
                else if (command == "if" || command == "ifcmd")
                {
                    var b = false;
                    if (command == "if")
                    {
                        b = ProcessIfLineAsync(tokenList);
                    }
                    else if (command == "ifcmd")
                    {
                        b = await ProcessIfCmdLineAsync(tokenList);
                    }

                    if (!b)
                    {
                        var forward = true;
                        // 快进到 end
                        while (forward)
                        {
                            var t = reader.ReadNextTokenListTilLineEnd();
                            if (t[0].type == TokenType.Word && (t[0].text == "end" || t[0].text == "else"))
                            {
                                forward = false;
                            }
                        }
  
                    }
                    else
                    {
                        skipElse = true;
                    }
                }
                else if (command == "else")
                {
                    if (skipElse)
                    {
                        var forward = true;
                        // 快进到 end
                        while (forward)
                        {
                            var t = reader.ReadNextTokenListTilLineEnd();
                            if (t[0].type == TokenType.Word && (t[0].text == "end"))
                            {
                                forward = false;
                            }
                        }

                        skipElse = false;
                    }
                }
                else if (command == "end")
                {
                    skipElse = false;
                }
                else
                {
                    // command
                    var argList = new List<ResolveResult>();
                    for (int i = 1; i < tokenList.Count; i++)
                    {
                        var token = tokenList[i];
                        var result = Expression.Resolve(token);
                        argList.Add(result);
                    }

                    var task = InternalExecuteCommand(command, argList);
                    await task;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            Debug.Log("[Script] stop due to exception");
            return false;
        }
    }

    static bool ProcessIfLineAsync(List<Token> tokenList)
    {
        var expresion = new List<Token>();
        for (int i = 1; i < tokenList.Count; i++)
        {
            var token = tokenList[i];
            expresion.Add(token);
        }

        var b = Expression.Resolve(expresion);
        return b.boolValue;
    }

    static async Task<bool> ProcessIfCmdLineAsync(List<Token> tokenList)
    {
        var command = tokenList[1].text;
        var argList = new List<ResolveResult>();
        for (int i = 2; i < tokenList.Count; i++)
        {
            var token = tokenList[i];
            var result = Expression.Resolve(token);
            argList.Add(result);
        }

        var task = InternalExecuteCommand(command, argList);
        var ret = await task;
        var boolValue = (bool) ret;
        return boolValue;
    }

    static Task ProcessWaitLineAsync(List<Token> tokenList, TriggerEngine engine)
    {
        var tcs = new TaskCompletionSource<bool>();
        var keyword = tokenList[0].text;
        var eventName = Expression.Resolve(tokenList[1]).stringValue;

        var expresion = new List<Token>();
        for (int i = 2; i < tokenList.Count; i++)
        {
            var token = tokenList[i];
            expresion.Add(token);
        }

        Action handler = null;
        handler = () =>
        {
            if (expresion.Count > 0)
            {
                var result = Expression.Resolve(expresion);
                if (result.type != ResultValueType.Bool || result.boolValue != true)
                {
                    engine.RegisterOnetimeListner(eventName, handler);
                    return;
                }
            }

            tcs.SetResult(true);
        };
        engine.RegisterOnetimeListner(eventName, handler);
        return tcs.Task;
    }
}