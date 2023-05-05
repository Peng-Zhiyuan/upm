using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomLitJson;
using System;
using System.Text;
using System.Reflection;
using System.Linq;

public static class JsonUtil
{
    private const string INDENT_STRING = "    ";

    public static string Buitify(string json)
    {
        int indentation = 0;
        int quoteCount = 0;
        var result = from ch in json let quotes = ch == '"' ? quoteCount++ : quoteCount let lineBreak = ch == ',' && quotes % 2 == 0 ? ch + Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, indentation)) : null let openChar = ch == '{' || ch == '[' ? ch + Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, ++indentation)) : ch.ToString() let closeChar = ch == '}' || ch == ']' ? Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, --indentation)) + ch : ch.ToString() select lineBreak == null ? openChar.Length > 1 ? openChar : closeChar : lineBreak;
        return String.Concat(result);
    }

    public static JsonData ToJsonData<T>(List<T> list)
    {
        if (null == list) return null;
        var array = new JsonData();
        foreach (var element in list)
        {
            array.Add(element);
        }
        return array;
    }

    public static JsonData ToJsonData<T>(Dictionary<string, T> dictionary)
    {
        if (null == dictionary) return null;
        var map = new JsonData();
        map.SetJsonType(JsonType.Object);
        foreach (var key in dictionary.Keys)
        {
            var val = dictionary[key];
            map[key] = val is JsonData jsonData ? jsonData : new JsonData(val);
        }
        return map;
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
            if (jd == null)
            {
                return 0;
            }
            int intValue;
            if (jd.IsInt)
            {
                var jw = (IJsonWrapper)jd;
                intValue = jw.GetInt();
            }
            else if (jd.IsLong)
            {
                var jw = (IJsonWrapper)jd;
                intValue = (int)jw.GetLong();
            }
            else if (jd.IsString)
            {
                var jw = (IJsonWrapper)jd;
                var stringValue = (string)jw.GetString();
                var b = int.TryParse(stringValue, out var value);
                if (!b)
                {
                    throw new Exception($"target type is {type} but get json data type {jd.GetJsonType()}. and auto parse failed.");
                }
                intValue = value;
            }
            else
            {
                throw new Exception($"target type is {type} but get json data type {jd.GetJsonType()}");
            }
            return intValue;
        }
        else if (type == typeof(long))
        {
            if (jd == null)
            {
                return 0L;
            }
            long longValue;
            if (jd.IsInt)
            {
                var jw = (IJsonWrapper)jd;
                longValue = jw.GetInt();
            }
            else if (jd.IsLong)
            {
                var jw = (IJsonWrapper)jd;
                longValue = (long)jw.GetLong();
            }
            else if (jd.IsString)
            {
                var jw = (IJsonWrapper)jd;
                var stringValue = (string)jw.GetString();
                var b = long.TryParse(stringValue, out var value);
                if (!b)
                {
                    throw new Exception($"target type is {type} but get json data type {jd.GetJsonType()}. and auto parse failed.");
                }
                longValue = value;
            }
            else
            {
                throw new Exception($"taget type is ${type} but get json data type {jd.GetJsonType()}");
            }
            return longValue;
        }
        else if (type == typeof(ulong))
        {
            if (jd == null)
            {
                return 0UL;
            }
            ulong ulongValue;
            if (jd.IsInt)
            {
                var jw = (IJsonWrapper)jd;
                ulongValue = (ulong)jw.GetInt();
            }
            else if (jd.IsLong)
            {
                var jw = (IJsonWrapper)jd;
                ulongValue = (ulong)jw.GetLong();
            }
            else if (jd.IsString)
            {
                var jw = (IJsonWrapper)jd;
                var stringValue = (string)jw.GetString();
                var b = ulong.TryParse(stringValue, out var value);
                if (!b)
                {
                    throw new Exception($"target type is {type} but get json data type {jd.GetJsonType()}. and auto parse failed.");
                }
                ulongValue = value;
            }
            else
            {
                throw new Exception($"taget type is ${type} but get json data type {jd.GetJsonType()}");
            }
            return ulongValue;
        }
        else if (type == typeof(uint))
        {
            if (jd == null)
            {
                return 0U;
            }
            ulong uintValue;
            if (jd.IsInt)
            {
                var jw = (IJsonWrapper)jd;
                uintValue = (uint)jw.GetInt();
            }
            else if (jd.IsLong)
            {
                var jw = (IJsonWrapper)jd;
                uintValue = (ulong)jw.GetLong();
            }
            else if (jd.IsString)
            {
                var jw = (IJsonWrapper)jd;
                var stringValue = (string)jw.GetString();
                var b = uint.TryParse(stringValue, out var value);
                if (!b)
                {
                    throw new Exception($"target type is {type} but get json data type {jd.GetJsonType()}. and auto parse failed.");
                }
                uintValue = value;
            }
            else
            {
                throw new Exception($"taget type is ${type} but get json data type {jd.GetJsonType()}");
            }
            return uintValue;
        }
        else if (type == typeof(string))
        {
            if (jd == null)
            {
                return null;
            }
            string stringValue;
            if (jd.IsString)
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
            if (jd == null)
            {
                return 0f;
            }
            float floatValue;
            if (jd.IsDouble)
            {
                var jw = (IJsonWrapper)jd;
                floatValue = (float)jw.GetDouble();
            }
            else if (jd.IsInt)
            {
                var jw = (IJsonWrapper)jd;
                floatValue = jw.GetInt();
            }
            else if (jd.IsLong)
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
            if (jd == null)
            {
                return 0d;
            }
            float doubleValue;
            if (jd.IsDouble)
            {
                var jw = (IJsonWrapper)jd;
                doubleValue = (float)jw.GetDouble();
            }
            else if (jd.IsInt)
            {
                var jw = (IJsonWrapper)jd;
                doubleValue = jw.GetInt();
            }
            else if (jd.IsLong)
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
        else if (type == typeof(bool))
        {
            if (jd == null)
            {
                return false;
            }
            bool boolValue;
            if (!jd.IsBoolean)
            {
                throw new Exception($"taget type is ${type} but get json data type {jd.GetJsonType()}");
            }
            var jw = (IJsonWrapper)jd;
            boolValue = jw.GetBoolean();
            return boolValue;
        }
        else if (type.IsEnum)
        {
            if (!jd.IsInt
                && !jd.IsLong)
            {
                throw new Exception($"taget type is ${type} but get json data type {jd.GetJsonType()}");
            }
            var intValue = jd.ToInt();
            return Enum.ToObject(type, intValue);
        }
        else if (type.IsGenericType)
        {
            if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                if (jd == null)
                {
                    return null;
                }
                if (!jd.IsObject)
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
                    if (keyType == typeof(int))
                    {
                        key = int.Parse(keyString);
                    }
                    else if (keyType == typeof(string))
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
                var list = Activator.CreateInstance(type) as IList;
                var genericTypeList = type.GetGenericArguments();
                var itemType = genericTypeList[0];
                if (jd != null)
                {
                    if (!jd.IsArray)
                    {
                        throw new Exception($"taget type is ${type} but get json data type {jd.GetJsonType()}");
                    }
                    foreach (JsonData itemJd in (IList)jd)
                    {
                        var item = JsonUtil.JsonDataToObject(itemType, itemJd);
                        list.Add(item);
                    }
                }
                return list;
            }
        }
        else if (typeof(Array).IsAssignableFrom(type))
        {
            if(jd == null)
            {
                return null;
            }

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
        else if (type == typeof(JsonData))
        {
            return jd;
        }
        else
        {
            if (jd == null)
            {
                return null;
            }
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
                        if (fieldType == typeof(string))
                        {
                            field.SetValue(obj, null);
                        }
                        else
                        {
                            var value = Activator.CreateInstance(fieldType);
                            field.SetValue(obj, value);
                        }
                    }
                }
            }
            return obj;
        }
        return null;
    }
}