using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Token
{
    public TokenType type;

    public Token(string text, ResolveResult result)
    {
        if (text != null && result != null)
        {
            throw new Exception("[Token] both text and result is not null");
        }
        if (text == null && result == null)
        {
            throw new Exception("[Token] both text and result is null");
        }

        if (text != null)
        {
            this.text = text;
            this.type = TokenUtil.GetTypeByText(this.text);
        }
        if (result != null)
        {
            this.result = result;
            this.type = TokenType.ResolveResult;
        }
    }
    public ResolveResult result;
    public string text;


    // 计算函数暂存使用
    public Token _pre;
    public Token _next;

    public override string ToString()
    {
        if(this.type == TokenType.ResolveResult)
        {
            return $"[ResolveResult: {this.result.type} {this.result}]";
        }
        return this.text;
    }

}