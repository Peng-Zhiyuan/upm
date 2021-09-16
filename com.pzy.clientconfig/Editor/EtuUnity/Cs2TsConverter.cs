using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using CustomLitJson;
using System.Text;
using Ionic.Zlib;

public static class Cs2TsConverter
{
    public static string Convert(string code)
    {
        // 替换定义行为 interface
        code = code.Replace("public partial class", "interface");
        code = code.Replace("using System.Collections.Generic;", "");
        // 替换属性定义
        code = EveryLineCsFieldToTyField(code);
        return code;
    }

    private static string EveryLineCsFieldToTyField(string code)
    {
        var lines = code.Split('\n');
        for(int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var isFieldLine = line.StartsWith("\tpublic");
            if(isFieldLine)
            {
                var parts = line.Split(' ');
                var type = parts[1];
                type = CsTypeToTyType(type);
                var name = parts[2].TrimEnd(';');


                // 临时补丁，去除字段中的@后面部分
                // TODO： 应该从现把 cs 代码修正
                if(name.IndexOf("@") != -1)
                {
                    var pparts = name.Split('@');
                    name = pparts[0];
                }

                var comments = parts[parts.Length-1];
                if(!comments.StartsWith("//"))
                {
                    comments = "";
                }
                var newCode = $"\t{name}: {type}  {comments}";
                lines[i] = newCode;
            }
        }
        var allLines = string.Join("\n", lines);
        return allLines;
    }

    public static string CsTypeToTyType(string type)
    {
        if(type == "int")
        {
            return "number";
        }
        else if(type == "bool")
        {
            return "boolean";
        }
        else if(type == "float")
        {
            return "number";
        }
        else if(type == "double")
        {
            return "number";
        }
        else if(type == "string")
        {
            return "string";
        }
        else
        {
            if(type.StartsWith("List<") && type.EndsWith(">"))
            {
                var innerType = type.Substring(5, type.Length - 6);
                innerType = CsTypeToTyType(innerType);
                return $"{innerType}[]";
            }
        }
        return type;
    }
}