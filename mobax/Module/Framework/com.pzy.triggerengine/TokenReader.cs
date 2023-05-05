using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TokenReader
{
    string content;
    int pos;
    public TokenReader(string content)
    {
        this.content = content;
        this.pos = 0;
    }

    /// <summary>
    /// 读取下一个 token，已经没有，则返回 null
    /// </summary>
    /// <returns></returns>
    public Token ReadNextToken(int maxPos)
    {
        this.ScanToNonWhiteSpace();
        if(!this.IsPosValid)
        {
            return null;
        }
        if(pos > maxPos)
        {
            return null;
        }

        var ch = this.CurrentChar;
        var startIndex = this.pos;
        if(ch == '"')
        {
            var endQoutIndex = GetNextSecondQouteIndex(this.pos + 1);
            if(endQoutIndex == -1)
            {
                throw new System.Exception("[TokenReader] not found end qout");
            }
            startIndex = endQoutIndex + 1;
        }

        var index = GetNextWhiteSpaceOrEndIndex(startIndex);
        if(index > maxPos)
        {
            return null;
        }
        var tokenString = ReadTo(index);
        var token = new Token(tokenString, null);
        return token;
    }

    char CurrentChar
    {
        get
        {
            var ch = this.content[this.pos];
            return ch;
        }
    }

    /// <summary>
    /// 读取下一个 token，已经没有，则返回 null
    /// </summary>
    /// <returns></returns>
    public List<Token> ReadNextTokenListTilLineEnd()
    {
        this.ScanToNonWhiteSpace();
        if (!this.IsPosValid)
        {
            return null;
        }
        var lineEndIndex = GetNextLineEndIndex();
        var ret = new List<Token>();
        while (this.pos < lineEndIndex)
        {
            var token = this.ReadNextToken(lineEndIndex);
            if(token == null)
            {
                break;
            }
            ret.Add(token);
        }
        return ret;
    }

    string ReadTo(int indexExlusive)
    {
        var startIndex = this.pos;
        var endIndex = indexExlusive;
        var length = endIndex - startIndex;
        var str = this.content.Substring(startIndex, length);
        this.pos = indexExlusive;
        return str;
    }

    int GetNextWhiteSpaceOrEndIndex(int startIndex)
    {
        var index = startIndex;
        while (index < content.Length)
        {
            var ch = this.content[index];
            if(char.IsWhiteSpace(ch))
            {
                return index;
            }
            index++;
        }
        return index;
    }

    int GetNextSecondQouteIndex(int startIndex)
    {
        var index = startIndex;
        while (index < content.Length)
        {
            var ch = this.content[index];
            if(ch == '"')
            {
                return index;
            }
            index++;
        }
        return -1;
    }

    int GetNextLineEndIndex()
    {
        var index = this.pos;
        while (index < content.Length && this.content[index] != '\n')
        {
            index++;
        }
        return index;
    }

    //public bool HasNextNoneWhiteSpaceChar()
    //{
    //    var index = this.pos;
    //    while(index < content.Length)
    //    {
    //        var ch = this.content[index];
    //        if (!char.IsWhiteSpace(ch))
    //        {
    //            return true;
    //        }
    //        index++;
    //    }
    //    return false;
    //}

    bool ScanToNonWhiteSpace()
    {
        while(this.pos < content.Length)
        {
            var ch = this.content[this.pos];
            if(!char.IsWhiteSpace(ch))
            {
                return true;
            }
            this.pos++;
        }
        return false;
    }

    bool IsPosValid
    {
        get
        {
            if (this.pos < content.Length)
            {
                return true;
            }
            return false;
        }
    }

}
