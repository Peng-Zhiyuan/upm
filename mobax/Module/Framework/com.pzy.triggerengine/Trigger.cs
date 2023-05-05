using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger
{
    public string doBlock;



    public Dictionary<string, ResolveResult> headDic;
    public string when;
    public List<Token> whenExpresion;
    public bool enable = true;
    public string id;
    public bool isProcessed;


    public Trigger(string headBlock, string doBlock)
    {
        this.doBlock = doBlock.Trim() ;
        this.ReadHeadInfo(headBlock);
    }

    public T GetHeadValue<T>(string flag, T defaultValue)
    {
        var has = headDic.ContainsKey(flag);
        if(!has)
        {
            return defaultValue;
        }
        var v = headDic[flag];
        var nativeValue = v.ToNativeValue(defaultValue);
        return nativeValue; 
    }

    void ReadHeadInfo(string headBlock)
    {
        headDic = new Dictionary<string, ResolveResult>();
        var reader = new TokenReader(headBlock);
        while(true)
        {
            var tokenList = reader.ReadNextTokenListTilLineEnd();
            if(tokenList == null)
            {
                break;
            } 
            var first = tokenList[0];
            if (first.type == TokenType.Comments)
            {
                continue;
            }
            if(first.type == TokenType.Word)
            {
                var key = first.text;
                if(key == "when")
                {
                    this.when = Expression.Resolve(tokenList[1]).stringValue;
                    var list = new List<Token>();
                    for(int i = 2; i < tokenList.Count; i++)
                    {
                        list.Add(tokenList[i]);
                    }
                    this.whenExpresion = list;
                }
                else if(key == "enable")
                {
                    this.enable = Expression.Resolve(tokenList[1]).boolValue;
                }
                else if(key == "id")
                {
                    this.id = Expression.Resolve(tokenList[1]).stringValue;
                }
                else
                {
                    this.headDic[key] = Expression.Resolve(tokenList[1]);
                }
            }
        }
      
    }
}
