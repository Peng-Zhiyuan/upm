using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using CustomLitJson;
using System.Text;
using Ionic.Zlib;

public static class Cs2GoConverter
{
    public static string Convert(string code)
    {
        // 替换定义行为 interface
        // code = code.Replace("public partial class", "interface");
        code = code.Replace("using System.Collections.Generic;", "");
        // 替换属性定义
        code = EveryLineCsFieldToTyField(code);
        return code;
    }

    private static string ParseSybol(string oneLineCode, string preParseAnchor)
    {
        var patterStartIndex = oneLineCode.IndexOf(preParseAnchor);
        if(patterStartIndex == -1)
        {
            //不存在指定模式 
            return null;
        }
        var contentStartIndex =  patterStartIndex + preParseAnchor.Length;
        var sb = new StringBuilder();

        var startRead = false;
        for(var index = contentStartIndex; index < oneLineCode.Length; index++)
        {
            var ch = oneLineCode[index];
            if(!startRead && ch == ' ')
            {
                continue;
            }
            if(ch == ';' || ch == ' ')
            {
                break;
            }
            startRead = true;
            sb.Append(ch);
        }
        var ret = sb.ToString();
        return ret;
    }

    private static string EveryLineCsFieldToTyField(string code)
    {
        var lines = code.Split('\n');
        for(int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            // 可能是类定义行
            {
                var pattern = "public partial class";
                if(line.IndexOf(pattern) != -1)
                {
                    // 这一行是类定义
                    var clazzName = ParseSybol(line, pattern);
                    var newCode = $"type {clazzName} struct";
                    lines[i] = newCode;
                    continue;
                }
            }


            // 可能是字段定义行
            {
                var isFieldLine = line.StartsWith("\tpublic");
                if(isFieldLine)
                {
                    var parts = line.Split(' ');
                    var type = parts[1];
                    type = CsTypeToTyType(type);
                    var name = parts[2].TrimEnd(';');
                    
                    var comments = parts[parts.Length-1];
                    if(!comments.StartsWith("//"))
                    {
                        comments = "";
                    }
                    var firstChUpName = FirstCharUp(name);
                    var newCode = $"\t{firstChUpName} {type} `json:\"{name}\"` {comments}";
                    lines[i] = newCode;
                }
            }
            
        }
        var allLines = string.Join("\n", lines);
        return allLines;
    }

    public static string FirstCharUp(string text)
    {
        var firstCh = text[0];
        var post = text.Substring(1);
        firstCh = char.ToUpper(firstCh);
        return firstCh + post;
    }

    public static string CsTypeToTyType(string type)
    {
        if(type == "int")
        {
            return "int32";
        }
        else if(type == "bool")
        {
            return "boolean";
        }
        else if(type == "float")
        {
            return "float32";
        }
        else if(type == "double")
        {
            return "float64";
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
                return $"[]{innerType}";
            }
        }

        // 可能是引用类型，前面加上 *
        return "*" + type;
    }
}