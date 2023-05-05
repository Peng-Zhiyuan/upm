using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public static class CodeUtil 
{
    public static string AddToNextLine(string code, string search, string newLine)
    {
        var index = code.IndexOf(search);
        if(index == -1)
        {
            throw new Exception("[CodeUtil] not found search text: " + search);
        }
        var nIndex = code.IndexOf('\n', index);
        var newLineIndex = nIndex + 1;
        var before = code.Substring(0, nIndex + 1);
        var post = code.Substring(newLineIndex);
        var newCode = before + newLine +"\n"+ post;

        return newCode;
    }

    public static void AddToNextLineInFile(string filePath, string search, string newLine)
    {
        var code = File.ReadAllText(filePath);
        var newCode = AddToNextLine(code, search, newLine);
        File.WriteAllText(filePath, newCode);
    }

    public static string ReplaceLine(string code, string search, string replaceLine)
    {
        return code.Replace(search, replaceLine);
    }

    public static void ReplaceInFile(string filePath, string search, string replaceLine)
    {
        var code = File.ReadAllText(filePath);
        var newCode = ReplaceLine(code, search, replaceLine);
        File.WriteAllText(filePath, newCode);
    }
}
