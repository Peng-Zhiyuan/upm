using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class ProjectBuildPiplineCommandLine
{
    public static void Build()
    {
        var schema = "";
        var paramDic = new Dictionary<string, string>();
        CommandLineUtil.ReadOptions((arg, value) =>
        {
            if(arg == "--schema")
            {
                schema = value;
            }
            var key = arg.TrimStart('-');
            paramDic[key] = value;
        });


        if(schema == "")
        {
            throw new Exception($"[Project Build Pipline CommandLine] need option '--schema'");
        }

        //var paramNameList = ProjectBuildPipline.GetSchemaParamNameList(schema);
        //var paramDic = CreateParamDic(paramNameList);
        ProjectBuildPipline.Build(schema, paramDic);
    }

    public static Dictionary<string, string> CreateParamDic(List<string> paramNameList)
    {
        var paramDic = new Dictionary<string, string>();
        var cmdParamDic = new Dictionary<string, string>();
        CommandLineUtil.ReadOptions((arg, value) =>
        {
            var key = arg.TrimStart('-');
            cmdParamDic[key] = value;
        });
        foreach(var param in paramNameList)
        {
            if(cmdParamDic.ContainsKey(param))
            {
                var value = cmdParamDic[param];
                paramDic[param] = value;
            }
            else
            {
                throw new Exception($"[Project Build Pipline] schema need param `{param}`");
            }
        }
        return paramDic;
    }
}
