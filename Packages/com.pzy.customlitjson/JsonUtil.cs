using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomLitJson;
using System;
using System.Text;
using System.Reflection;

public static class JsonUtil
{
    
    public static string ToJsonStr(this Dictionary<string, List<string>> dic){
        return ToJson(dic).ToJson();
    }

    public static JsonData ToJson(this Dictionary<string, List<string>> dic){
        var data = new JsonData();
        foreach(var lable in dic.Keys){
            data[lable] = new JsonData();
            foreach(var address in dic[lable]){
                data[lable].Add(address);
            }
        }
        return data;
    }

    public static Dictionary<string, List<string>> JsonStr2LabelDictionary(string strJson){
        Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
        JsonData jd = JsonMapper.Instance.ToObject(strJson);
        var dic = jd as IDictionary;
        foreach(var k in dic.Keys){
            result[k.ToString()] = new List<string>();
            var array = dic[k] as JsonData;
            foreach(var i in array){
                result[k.ToString()].Add(i.ToString());
            }
        }
        return result;
    }

    public static object JsonDataToObject(Type type, JsonData jd)
    {
        if (type == typeof(int))
        {
            int intValue;
            if(jd.IsInt)
            {
                var jw = (IJsonWrapper)jd;
                intValue = jw.GetInt();
            }
            else if(jd.IsLong)
            {
                var jw = (IJsonWrapper)jd;
                intValue = (int)jw.GetLong();
            }
            else
            {
                throw new Exception($"taget type is ${type} but get json data type {jd.GetJsonType()}");
            }
            return intValue;
        }
        else if (type == typeof(long))
        {
            long longValue;
            if(jd.IsInt)
            {
                var jw = (IJsonWrapper)jd;
                longValue = jw.GetInt();
            }
            else if(jd.IsLong)
            {
                var jw = (IJsonWrapper)jd;
                longValue = (long)jw.GetLong();
            }
            else
            {
                throw new Exception($"taget type is ${type} but get json data type {jd.GetJsonType()}");
            }
            return longValue;
        }
        else if (type == typeof(string))
        {
            string stringValue;
            if(jd.IsString)
            {
                var jw = (IJsonWrapper)jd;
                stringValue = jw.GetString();
            }
            else
            {
                throw new Exception($"taget type is ${type} but get json data type {jd.GetJsonType()}");
            }
            return stringValue;
        }
        else if (type == typeof(float))
        {
            float floatValue;
            if(jd.IsDouble)
            {
                var jw = (IJsonWrapper)jd;
                floatValue = (float)jw.GetDouble();
            }
            else if(jd.IsInt)
            {
                var jw = (IJsonWrapper)jd;
                floatValue = jw.GetInt();
            }
            else if(jd.IsLong)
            {
                var jw = (IJsonWrapper)jd;
                floatValue = jw.GetLong();
            }
            else
            {
                throw new Exception($"taget type is ${type} but get json data type {jd.GetJsonType()}");
            }
            return floatValue;
        }
        else if (type == typeof(double))
        {
            float doubleValue;
            if(jd.IsDouble)
            {
                var jw = (IJsonWrapper)jd;
                doubleValue = (float)jw.GetDouble();
            }
            else if(jd.IsInt)
            {
                var jw = (IJsonWrapper)jd;
                doubleValue = jw.GetInt();
            }
            else if(jd.IsLong)
            {
                var jw = (IJsonWrapper)jd;
                doubleValue = jw.GetLong();
            }
            else
            {
                throw new Exception($"taget type is ${type} but get json data type {jd.GetJsonType()}");
            }
            return doubleValue;
        }
        else if(type == typeof(bool))
        {
            bool boolValue;
            if(!jd.IsBoolean)
            {
                throw new Exception($"taget type is ${type} but get json data type {jd.GetJsonType()}");
            }
            var jw = (IJsonWrapper)jd;
            boolValue = jw.GetBoolean();
            return boolValue;
        }
        else if (type.IsGenericType)
        {
            if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                if(!jd.IsObject)
                {
                    throw new Exception($"taget type is ${type} but get json data type {jd.GetJsonType()}");
                }
                var dic = Activator.CreateInstance(type) as IDictionary;
                var genericTypeList = type.GetGenericArguments();
                var keyType = genericTypeList[0];
                var valueType = genericTypeList[1];
                foreach (DictionaryEntry kv in (IDictionary)jd)
                {
                    var keyString = kv.Key as string;
                    var valueJd = kv.Value as JsonData;
                    object key;
                    object value;
                    if(keyType == typeof(int))
                    {
                        key = int.Parse(keyString);
                    }
                    else if(keyType == typeof(string))
                    {
                        key = keyString;
                    }
                    else
                    {
                        throw new Exception($"unsport dictionary key type {keyType}");
                    }
                    value = JsonUtil.JsonDataToObject(valueType, valueJd);
                    dic[key] = value;
                }
                return dic;
            }
            else if (type.GetGenericTypeDefinition() == typeof(List<>))
            {
                if(!jd.IsArray)
                {
                    throw new Exception($"taget type is ${type} but get json data type {jd.GetJsonType()}");
                }
                var list = Activator.CreateInstance(type) as IList;
                var genericTypeList = type.GetGenericArguments();
                var itemType = genericTypeList[0];
                foreach (JsonData itemJd in (IList)jd)
                {
                    var item = JsonUtil.JsonDataToObject(itemType, itemJd);
                    list.Add(item);
                }
                return list;
            }
        }
        else
        {
            if (!jd.IsObject)
            {
                throw new Exception($"taget type is {type} but get json data type {jd.GetJsonType()}");
            }
            var fieldList = type.GetFields();
            var obj = Activator.CreateInstance(type);
            var jdDic = (IDictionary)jd;
            foreach (var field in fieldList)
            {
                if (field.IsPublic && !field.IsStatic)
                {
                    var name = field.Name;
                    var fieldType = field.FieldType;
                    var b = jdDic.Contains(name);
                    if (b)
                    {
                        var valueJd = jd[name];
                        var value = JsonUtil.JsonDataToObject(fieldType, valueJd);
                        field.SetValue(obj, value);
                    }
                    else
                    {
                        var value = Activator.CreateInstance(fieldType);
                        field.SetValue(obj, value);
                    }
                }
            }
            return obj;
        }
        return null;

    }
}