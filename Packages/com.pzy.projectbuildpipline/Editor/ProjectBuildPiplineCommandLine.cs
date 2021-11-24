using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class ProjectBuildPiplineCommandLine
{
    public static void Build()
    {
        var schema = "";
        CommandLineUtil.ReadOptions((arg, value) =>
        {
            if(arg == "--schema")
            {
                schema = value;
            }
        });

        if(schema == "")
        {
            throw new Exception($"[Project Build Pipline CommandLine] need option '--schema'");
        }

        ProjectBuildPipline.Build(schema);
    }
}
