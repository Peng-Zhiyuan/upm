using System;
using System.Collections.Generic;

public static class Expression 
{
    public static Func<string, ResolveResult> OnResolveVaribale;

    public static List<Token> CreateFromString(List<string> tokenStringList)
    {
        var ret = new List<Token>();
        foreach(var str in tokenStringList)
        {
            var token = new Token(str, null);
            ret.Add(token);
        }
        return ret;
    }

    public static ResolveResult Resolve(Token token)
    {
        var list = new List<Token>();
        list.Add(token);
        var result = Resolve(list);
        return result;
    }

    public static ResolveResult Resolve(List<Token> expression)
    {
        var head = LinkToken(expression);

        // 先计算变量
        {
            var variableTokenList = FindTokenList(head, TokenType.Variable);
            foreach (var token in variableTokenList)
            {
                ReolveVairbaleInLink(token, ref head);
            }
        }

        // 然后计算常亮
        {
            {
                var tokenList = FindTokenList(head, TokenType.IntConst);
                foreach (var token in tokenList)
                {
                    ResolveIntConstInLink(token, ref head);
                }
            }
            {
                var tokenList = FindTokenList(head, TokenType.BoolConst);
                foreach (var token in tokenList)
                {
                    ResolveBoolConstInLink(token, ref head);
                }
            }
            {
                var tokenList = FindTokenList(head, TokenType.StringConst);
                foreach (var token in tokenList)
                {
                    ResolveStringConstInLink(token, ref head);
                }
            }
        }

        // 然后计算 3 级二元运算符
        {
            var tokenList = FindTokenList(head, TokenType.BinocularOperatorLv3);
            foreach (var token in tokenList)
            {
                ReolveBinaryOperatorInLink(token, ref head);
            }
        }

        // 然后计算 2 级二元运算符
        {
            var tokenList = FindTokenList(head, TokenType.BinocularOperatorLv2);
            foreach (var token in tokenList)
            {
                ReolveBinaryOperatorInLink(token, ref head);
            }
        }

        // 然后计算 1 级二元运算符
        {
            var tokenList = FindTokenList(head, TokenType.BinocularOperatorLv1);
            foreach (var token in tokenList)
            {
                ReolveBinaryOperatorInLink(token, ref head);
            }
        }

        // 应当只剩下结果 token
        if(head._next != null)
        {
            throw new Exception("[Expression] something is wrong, result is not one token");
        }
        if(head.type != TokenType.ResolveResult)
        {
            throw new Exception("[Expression] something is wrong, result token is not resolve result type");
        }
        var ret = head.result;
        return ret;
    }

    static Token CreateIntResultToken(int value)
    {
        var result = new ResolveResult();
        result.intValue = value;
        result.type = ResultValueType.Int;

        var token = new Token(null, result);
        return token;
    }

    static Token CreateBoolResultToken(bool value)
    {
        var result = new ResolveResult();
        result.boolValue = value;
        result.type = ResultValueType.Bool;

        var token = new Token(null, result);
        return token;
    }

    static Token CreateStringResultToken(string value)
    {
        var result = new ResolveResult();
        result.stringValue = value;
        result.type = ResultValueType.String;

        var token = new Token(null, result);
        return token;
    }

    static void ResolveStringConstInLink(Token stringConstToken, ref Token head)
    {
        var text = stringConstToken.text;
        var result = new ResolveResult();
        result.stringValue = text.Substring(1, text.Length - 2);
        result.type = ResultValueType.String;

        var token = new Token(null, result);
        ReplaceInLink(stringConstToken, token, ref head);
    }

    static void ResolveIntConstInLink(Token intConstToken, ref Token head)
    {
        var text = intConstToken.text;
        var result = new ResolveResult();
        result.intValue = int.Parse(text);
        result.type = ResultValueType.Int;

        var token = new Token(null, result);
        ReplaceInLink(intConstToken, token, ref head);
    }

    static void ResolveBoolConstInLink(Token boolConstToken, ref Token head)
    {
        var text = boolConstToken.text;
        var result = new ResolveResult();
        result.boolValue = bool.Parse(text);
        result.type = ResultValueType.Bool;

        var token = new Token(null, result);
        ReplaceInLink(boolConstToken, token, ref head);
    }

    static void ReolveBinaryOperatorInLink(Token op, ref Token head)
    {
        var pre = op._pre;
        var next = op._next;
        if(pre == null)
        {
            throw new Exception("[Expression] pre not found of binary operator " + op.text);
        }
        if (next == null)
        {
            throw new Exception("[Expression] next not found of binary operator " + op.text);
        }

        // 在此之前常亮 token 已经被计算
        // 因此只需要考虑 result 类型 token
        if(op.text == "+")
        {
            if(pre.result.type == ResultValueType.Int && next.result.type == ResultValueType.Int)
            {
                var a = pre.result.intValue;
                var b = next.result.intValue;
                var value = a + b;
                var result = CreateIntResultToken(value);
                ReplaceRangeInLink(pre, next, result, ref head);
            }
            else if (pre.result.type == ResultValueType.String && next.result.type == ResultValueType.String)
            {
                var a = pre.result.stringValue;
                var b = next.result.stringValue;
                var value = a + b;
                var result = CreateStringResultToken(value);
                ReplaceRangeInLink(pre, next, result, ref head);
            }
            else
            {
                throw new Exception("[Expression] pre or next type not valid for operator " + op.text);
            }
        }
        else if(op.text == "-")
        {
            if (pre.result.type == ResultValueType.Int && next.result.type == ResultValueType.Int)
            {
                var a = pre.result.intValue;
                var b = next.result.intValue;
                var value = a - b;
                var result = CreateIntResultToken(value);
                ReplaceRangeInLink(pre, next, result, ref head);
            }
            else
            {
                throw new Exception("[Expression] pre or next type not valid for operator " + op.text);
            }
        }
        else if (op.text == "*")
        {
            if (pre.result.type == ResultValueType.Int && next.result.type == ResultValueType.Int)
            {
                var a = pre.result.intValue;
                var b = next.result.intValue;
                var value = a * b;
                var result = CreateIntResultToken(value);
                ReplaceRangeInLink(pre, next, result, ref head);
            }
            else
            {
                throw new Exception("[Expression] pre or next type not valid for operator " + op.text);
            }
        }
        else if (op.text == "/")
        {
            if (pre.result.type == ResultValueType.Int && next.result.type == ResultValueType.Int)
            {
                var a = pre.result.intValue;
                var b = next.result.intValue;
                var value = a / b;
                var result = CreateIntResultToken(value);
                ReplaceRangeInLink(pre, next, result, ref head);
            }
            else
            {
                throw new Exception("[Expression] pre or next type not valid for operator " + op.text);
            }
        }
        else if (op.text == ">")
        {
            if (pre.result.type == ResultValueType.Int && next.result.type == ResultValueType.Int)
            {
                var a = pre.result.intValue;
                var b = next.result.intValue;
                var value = a > b;
                var result = CreateBoolResultToken(value);
                ReplaceRangeInLink(pre, next, result, ref head);
            }
            else
            {
                throw new Exception("[Expression] pre or next type not valid for operator " + op.text);
            }
        }
        else if (op.text == "<")
        {
            if (pre.result.type == ResultValueType.Int && next.result.type == ResultValueType.Int)
            {
                var a = pre.result.intValue;
                var b = next.result.intValue;
                var value = a < b;
                var result = CreateBoolResultToken(value);
                ReplaceRangeInLink(pre, next, result, ref head);
            }
            else
            {
                throw new Exception("[Expression] pre or next type not valid for operator " + op.text);
            }
        }
        else if (op.text == ">=")
        {
            if (pre.result.type == ResultValueType.Int && next.result.type == ResultValueType.Int)
            {
                var a = pre.result.intValue;
                var b = next.result.intValue;
                var value = a >= b;
                var result = CreateBoolResultToken(value);
                ReplaceRangeInLink(pre, next, result, ref head);
            }
            else
            {
                throw new Exception("[Expression] pre or next type not valid for operator " + op.text);
            }
        }
        else if (op.text == "<=")
        {
            if (pre.result.type == ResultValueType.Int && next.result.type == ResultValueType.Int)
            {
                var a = pre.result.intValue;
                var b = next.result.intValue;
                var value = a <= b;
                var result = CreateBoolResultToken(value);
                ReplaceRangeInLink(pre, next, result, ref head);
            }
            else
            {
                throw new Exception("[Expression] pre or next type not valid for operator " + op.text);
            }
        }
        else if (op.text == "&&")
        {
            if (pre.result.type == ResultValueType.Bool && next.result.type == ResultValueType.Bool)
            {
                var a = pre.result.boolValue;
                var b = next.result.boolValue;
                var value = a && b;
                var result = CreateBoolResultToken(value);
                ReplaceRangeInLink(pre, next, result, ref head);
            }
            else
            {
                throw new Exception("[Expression] pre or next type not valid for operator " + op.text);
            }
        }
        else if (op.text == "||")
        {
            if (pre.result.type == ResultValueType.Bool && next.result.type == ResultValueType.Bool)
            {
                var a = pre.result.boolValue;
                var b = next.result.boolValue;
                var value = a || b;
                var result = CreateBoolResultToken(value);
                ReplaceRangeInLink(pre, next, result, ref head);
            }
            else
            {
                throw new Exception("[Expression] pre or next type not valid for operator " + op.text);
            }
        }
        else if (op.text == "==")
        {
            if (pre.result.type == ResultValueType.Bool && next.result.type == ResultValueType.Bool)
            {
                var a = pre.result.boolValue;
                var b = next.result.boolValue;
                var value = a == b;
                var result = CreateBoolResultToken(value);
                ReplaceRangeInLink(pre, next, result, ref head);
            }
            else if (pre.result.type == ResultValueType.Int && next.result.type == ResultValueType.Int)
            {
                var a = pre.result.intValue;
                var b = next.result.intValue;
                var value = a == b;
                var result = CreateBoolResultToken(value);
                ReplaceRangeInLink(pre, next, result, ref head);
            }
            else if (pre.result.type == ResultValueType.String && next.result.type == ResultValueType.String)
            {
                var a = pre.result.stringValue;
                var b = next.result.stringValue;
                var value = a == b;
                var result = CreateBoolResultToken(value);
                ReplaceRangeInLink(pre, next, result, ref head);
            }
            else
            {
                throw new Exception("[Expression] pre or next type not valid for operator " + op.text);
            }
        }
    }

    static void ReplaceInLink(Token origin, Token replacer, ref Token head)
    {
        var pre = origin._pre;
        var next = origin._next;
        replacer._pre = pre;
        if(pre != null)
        {
            pre._next = replacer;
        }
        replacer._next = next;
        if(next != null)
        {
            next._pre = replacer;
        }

        if(pre == null)
        {
            head = replacer;
        }
    }

    static void ReplaceRangeInLink(Token start, Token end, Token inserter, ref Token head)
    {
        var pre = start._pre;
        var next = end._next;

        inserter._pre = pre;
        inserter._next = next;

        if(pre != null)
        {
            pre._next = inserter;
        }
        if(next != null)
        {
            next._pre = inserter;
        }

        if(pre == null)
        {
            head = inserter;
        }
    }

    static Token ReolveVairbaleInLink(Token variableToken, ref Token head)
    {
        var variableName = variableToken.text;
        if(OnResolveVaribale == null)
        {
            throw new Exception("[Expression] OnResolveVaribale not set yet");
        }
        var result = OnResolveVaribale.Invoke(variableName);
        UnityEngine.Debug.Log($"[Script] ${variableName} is {result}");
        var resultToken = new Token(null, result);

        ReplaceInLink(variableToken, resultToken, ref head);

        return resultToken;
    }

 
    static Token LinkToken(List<Token> tokenList)
    {
        for(int i = 1; i < tokenList.Count - 1; i++)
        {
            var token = tokenList[i];
            var pre = tokenList[i - 1];
            var next = tokenList[i + 1];
            token._pre = pre;
            token._next = next;

            pre._next = token;
            next._pre = token;
        }
        tokenList[0]._pre = null;
        tokenList[tokenList.Count - 1]._next = null;
        return tokenList[0];
    }
    static List<Token> FindTokenList(Token head, TokenType tokenType)
    {
        var ret = new List<Token>();
        var pointer = head;
        while(pointer != null)
        {
            if (pointer.type == tokenType)
            {
                ret.Add(pointer);
            }
            pointer = pointer._next;
        }
        return ret;
    }
}




