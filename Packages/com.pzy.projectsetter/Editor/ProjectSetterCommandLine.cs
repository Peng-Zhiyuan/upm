using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.Text;
using System.IO;

public class ProjectSetterCommandLine
{
    /// <summary>
    /// 命令行统一编译方法(的CLI接口)
    /// </summary>
    /// 
    public static void Set()
    {
        // 命令行参数检查
        var args = System.Environment.GetCommandLineArgs();

        // 默认参数
        var type = "";


        // 处理命令行选项
        ReadOptions((option, arg) =>
        {
            switch (option)
            {
                case "--type":
                    type = arg;
                    break;
            }

        });


        // game add
        if (type != "")
        {
            ProjectSetter.Set(type);
        }
        else
        {
            throw new Exception("[ProjectSetterCommandLine] --type not passed!");
        }

    }

    private static void ReadOptions(Action<string, string> onOption)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("-", StringComparison.Ordinal))
            {
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
