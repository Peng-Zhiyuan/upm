using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class TokenUtil
{
    public static TokenType GetTypeByText(string token)
    {
        if(string.IsNullOrEmpty(token))
        {
            throw new Exception("[TokenUtil] token is empty");
        }
        if(token.StartsWith("$"))
        {
            return TokenType.Variable;
        }
        if(token.StartsWith("\"") && token.EndsWith("\""))
        {
            return TokenType.StringConst;
        }
        if(token == "(")
        {
            return TokenType.StartQout;
        }
        if(token == ")")
        {
            return TokenType.EndQout;
        }
        if(int.TryParse(token, out _))
        {
            return TokenType.IntConst;
        }
        if(bool.TryParse(token, out _))
        {
            return TokenType.BoolConst;
        }
        if (token == ">" || token == "<" || token == "==" || token == ">=" || token == "<=")
        {
            return TokenType.BinocularOperatorLv1;
        }
        if(token == "+" || token == "-" || token == "&&" || token == "||" )
        {
            return TokenType.BinocularOperatorLv2;
        }
        if(token == "*" || token == "/")
        {
            return TokenType.BinocularOperatorLv3;
        }
        var firstCh = token[0];
        if(Char.IsLetter(firstCh) || firstCh == '_')
        {
            return TokenType.Word;
        }
        if(token == "//")
        {
            return TokenType.Comments;
        }
        return TokenType.Unknown;
        //throw new Exception("[TokenUtil] invalid token: " + token);

    }
}
