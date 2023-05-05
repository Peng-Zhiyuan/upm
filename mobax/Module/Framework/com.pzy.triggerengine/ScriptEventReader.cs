using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ScriptEventReader
{
    string content;
    int pos;
    public ScriptEventReader(string content)
    {
        this.content = content;
        this.pos = 0;
    }

    public List<Trigger> ReadScriptEventInfoList()
    {
        var ret = new List<Trigger>();
        while (true)
        {
            var (b, headBlock) = this.ReadNextHeadBlock();
            if (!b)
            {
                return ret;
            }
            var (b2, actionBlock) = this.ReadNextActionBlock();
            if (!b2)
            {
                return ret;
            }
            var info = new Trigger(headBlock, actionBlock);
            ret.Add(info);
        }
    }

    /// <summary>
    /// 往后移动光标，直到发现搜索目标
    /// </summary>
    /// <param name="searchContent">搜索目标</param>
    /// <param name="postOfContent">是否要读到搜索目标后</param>
    /// <returns></returns>
    bool ScanTo(string searchContent, bool postOfContent)
    {
        var index = content.IndexOf(searchContent, this.pos);
        if (index == -1)
        {
            return false;
        }
        this.pos = index;
        if (postOfContent)
        {
            this.pos += searchContent.Length;
        }
        return true;
    }

    string ReadToLineEnd()
    {
        var index = this.content.IndexOf("\n", this.pos);
        if (index == -1)
        {
            // 到了文件末尾
            var msg2 = this.content.Substring(this.pos);
            this.pos = this.content.Length;
            return msg2;
        }
        var msg = this.content.Substring(this.pos, index - this.pos);
        this.pos = index;
        return msg;
    }

    string ReadToBlockEnd()
    {

        var nextHeadIndex = this.content.IndexOf("head:", this.pos);
        if(nextHeadIndex == -1)
        {
            nextHeadIndex = int.MaxValue;
        }
        var nextActionIndex = this.content.IndexOf("action:", this.pos);
        if (nextActionIndex == -1)
        {
            nextActionIndex = int.MaxValue;
        }
        var endIndex = this.content.Length;

        var index = Mathf.Min(nextHeadIndex, nextActionIndex, endIndex);

        var msg = this.content.Substring(this.pos, index - this.pos);
        this.pos = index;
        return msg;
    }

    bool ScanToNextLine()
    {
        var index = this.content.IndexOf("\n", this.pos);
        if (index == -1)
        {
            return false;
        }
        var indexPostN = index + 1;
        this.pos = indexPostN;
        return true;
    }

    public (bool, string) ReadNextHeadBlock()
    {
        var ret = ReadNextBlock("head:");
        return ret;
    }

    public (bool, string) ReadNextActionBlock()
    {
        var ret = ReadNextBlock("action:");
        return ret;
    }

    public (bool, string) ReadNextBlock(string blockTagWithDot)
    {
        var b = this.ScanTo(blockTagWithDot, true);
        if (!b)
        {
            return (false, null);
        }
        var b2 = this.ScanToNextLine();
        if (!b2)
        {
            return (false, null);
        }
        var ret = this.ReadToBlockEnd();
        return (true, ret);

    }
}
