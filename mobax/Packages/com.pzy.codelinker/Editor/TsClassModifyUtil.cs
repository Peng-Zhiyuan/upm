using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

public static class TsClassModifyUtil 
{
    public static string AddOrUpdateBlock(string code, string blockTag, string newContent)
    {
        var hasTag = HasTag(code, blockTag);
        if(hasTag)
        {
            var ret = UpdateBlock(code, blockTag, newContent);
            return ret;
        }
        else
        {
            var ret = AddBlockInClassFirstLine(code, blockTag, newContent);
            return ret;
        }
    }

    static string UpdateBlock(string code, string blockTag, string newContent)
    {
        var blockBeginMark = $"\t// #region {blockTag}";
        var index = code.IndexOf(blockBeginMark);
        //var beginIndex = code.LastIndexOf("\t/*", index);

        var beforeContent = code.Substring(0, index);

        var blockEndMark = $"\t// #endregion";
        var index2 = code.IndexOf(blockEndMark);
        //var endIndex = code.IndexOf("*/", index2);

        var afterContent = code.Substring(index2 + blockEndMark.Length+2);

        var date = NowTimeStirng;
        var newBlock = CreateTagedContent(newContent, blockTag, date);

        var sb = new StringBuilder();
        sb.Append(beforeContent);
        sb.Append(newBlock);
        sb.Append(afterContent);
        return sb.ToString();
    }

    public static bool HasTag(string code, string blockTag)
    {
        var blockBeginMark = $"\t// #region {blockTag}";
        var contians = code.Contains(blockBeginMark);
        if(contians)
        {
            return true;
        }
        return false;
    }

    static string CreateTagedContent(string originContent, string blockTag, string timeStirng)
    {
        var ret = new StringBuilder();
        //ret.AppendLine("\t/*");
        //ret.AppendLine($"\tBegin Block: {blockTag}");
        //ret.AppendLine($"\tUpdate Time: {timeString}");
        //ret.AppendLine("\t*/");
        ret.AppendLine($"\t// #region {blockTag}");
        ret.AppendLine($"\t// Update Date: {timeStirng}");
        ret.Append(originContent);

        if (!originContent.EndsWith("\n"))
        {
            ret.Append("\n");
        }

        //ret.AppendLine("\t/*");
        //ret.AppendLine($"\tEnd Block: {blockTag}");
        //ret.AppendLine("\t*/");
        ret.AppendLine("\t// #endregion");


        return ret.ToString();
    }

    static string NowTimeStirng
    {
        get
        {
            var nowDate = DateTime.Now;
            var timeString = nowDate.ToString("yyyy/MM/dd HH:mm:ss");
            return timeString;
        }
    }

    static string AddBlockInClassFirstLine(string code, string blockTag, string newContent)
    {
        var timeString = NowTimeStirng;
        var ret = new StringBuilder();
        var lineList = code.Split('\n');

        var findClassDefineLineFlag = false;
        var findClassBeginBracket = false;
        var firstLineInClass = false;
        foreach(var line in lineList)
        {
            ret.Append(line);

            if (!findClassDefineLineFlag)
            {
                if (line.Contains("class "))
                {
                    findClassDefineLineFlag = true;
                }
                continue;
            }


            if(findClassDefineLineFlag)
            {
                if(!findClassBeginBracket)
                {
                    if (line.Contains("{"))
                    {
                        findClassBeginBracket = true;


                        var block = CreateTagedContent(newContent, blockTag, timeString);
                        ret.Append(block);
                        ret.AppendLine();
                        //ret.AppendLine("\t/*");
                        //ret.AppendLine($"\tBegin Block: {blockTag}");
                        //ret.AppendLine($"\tUpdate Time: {timeString}");
                        //ret.AppendLine("\t*/");
                        //ret.AppendLine(newContent);
                        //ret.AppendLine("\t/*");
                        //ret.AppendLine($"\tEnd Block: {blockTag}");
                        //ret.AppendLine("\t*/");
                        //ret.AppendLine();

                        continue;
                    }
                }
            }

        }
        return ret.ToString();
    }
}
