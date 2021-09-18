using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public static class Cmd
{
    public static void ReadOptions(Action<string, string> onOption)
    {
        var args = System.Environment.GetCommandLineArgs ();
        Debug.Log(string.Join(" ", args));
        for (int i = 0; i < args.Length; i++) {
            if (args [i].StartsWith("-", StringComparison.Ordinal)) {
                string option = args[i];
                string arg = "";
                if (i + 1 < args.Length)
                {
                    if (!args[i + 1].StartsWith("-", StringComparison.Ordinal))
                    {
                        arg = args[i + 1];
                    }
                }
                onOption(option, arg);
            }
        }
    }
}