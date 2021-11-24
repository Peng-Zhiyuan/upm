using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomLitJson;
using System;
using System.Text;
using System.Reflection;

public static class JsonUtil
{
    public static int ToInt(this JsonData jd)
    {
        var jw = (IJsonWrapper)jd;
        return jw.GetInt();
    }

    public static List<JsonData> ToList(this JsonData jd)
    {
        var list = new List<JsonData>();
        for (int k = 0; k < jd.Count; k++)
        {
            list.Add(jd[k]);
        }
        return list;
    }


    public static Dictionary<string, JsonData> ToDictionary(this JsonData jd)
    {
        var dic = new Dictionary<string, JsonData>();
        var jdDic = jd as IDictionary;
        foreach (string key in jdDic.Keys)
        {
            dic[key] = jd[key];
        }
        return dic;
    }

    public static bool ToBool(this JsonData jd)
    {
        var jw = (IJsonWrapper)jd;
        return jw.GetBoolean();
    }

    public static string ToString(this JsonData jd)
    {
        var jw = (IJsonWrapper)jd;
        return jw.GetString();
    }

    public static double ToDouble(this JsonData jd)
    {
        var jw = (IJsonWrapper)jd;
        return jw.GetDouble();
    }

    public static long ToLong(this JsonData jd)
    {
        var jw = (IJsonWrapper)jd;
        return jw.GetLong();
    }

    public static bool HasKey(this JsonData jd, string key)
    {
        var dic = jd.ToDictionary();
        var b = dic.ContainsKey(key);
        return b;
    }
    public static string Stringify(object o)
    {
        return CustomLitJson.JsonMapper.Instance.ToJson(o);
    }

    public static object Parse(string json)
    {
        return CustomLitJson.JsonMapper.Instance.ToObject(json);
    }

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

    public static T JsonDataToObject<T>(JsonData jd)
    {
        var type = typeof(T);
        var obj = JsonDataToObject(type, jd);
        return (T)obj;
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
        else if (typeof(Array).IsAssignableFrom(type))
        {
            var itemType = type.GetElementType();
            int n = jd.Count;
            var array = Array.CreateInstance(itemType, n);
            for (int i = 0; i < n; i++)
            {
                var item = JsonUtil.JsonDataToObject(itemType, jd[i]);
                ((Array)array).SetValue(item, i);
            }
            return array;
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