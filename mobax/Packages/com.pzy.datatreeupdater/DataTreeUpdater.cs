using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using CustomLitJson;
using UnityEngine;

public static class DataTreeUpdater 
{

    [Obsolete("使用 Update, 可以支持 JsonData 类型自动转换")]
    public static void SetValue<T> (object root, string path, T value) 
    {
        Debug.Log ($"[DataTree] {{{root.GetType().Name}}} '{path}' -> ({value.GetType().Name}){value}");
        nowPath.Clear ();
        var pathParts = path.Split ('.');
        var obj = root;
        for (int i = 0; i < pathParts.Length; i++) 
        {
            var part = pathParts[i];
            if (i != pathParts.Length - 1) 
            {
                // enter
                var isHiybirdDic = IsHybirdDic (obj);
                if (isHiybirdDic) 
                {
                    var filedOrKey = part;
                    var hibirdDic = obj as IDictionary;
                    obj = GotoHybirdDicChild (hibirdDic, filedOrKey);
                    continue;
                }
                var dic = obj as IDictionary;
                if (dic != null) 
                {
                    var key = part;
                    obj = GotoDicChild (dic, key);
                    continue;
                }
                var list = obj as IList;
                if (list != null) 
                {
                    var index = part;
                    obj = GotoListChild (list, index);
                    continue;
                }
                var regularObj = obj;
                if (regularObj != null) 
                {
                    var feild = part;
                    obj = GotoRegularObjChild (regularObj, feild);
                    continue;
                }
                throw new Exception ($"path: {PathToString()} type is {obj.GetType().Name}, which is unsupport  get child");
            } else {
                // set
                var isHiybirdDic = IsHybirdDic (obj);
                if (isHiybirdDic) 
                {
                    var filedOrKey = part;
                    var fieldInfo = GetObjFeildInfoCanNull (obj, filedOrKey);
                    if (fieldInfo != null) 
                    {
                        fieldInfo.SetValue (obj, value as object);
                        return;
                    } 
                    else 
                    {
                        var hibirdDic = obj as IDictionary;
                        hibirdDic[filedOrKey] = value as object;
                        return;
                    }
                }
                var dic = obj as IDictionary;
                if (dic != null) 
                {
                    var key = part;
                    dic[key] = value as object;
                    return;
                }

                var list = obj as IList;
                if (list != null) 
                {
                    var index = int.Parse (part);
                    if (index > 0 && index < list.Count) 
                    {
                        list[index] = value as object;
                        return;
                    } 
                    else 
                    {
                        throw new Exception ($"path: {PathToString()} type is list, which count is {list.Count}, but set{index}");
                    }
                }

                var regularObj = obj;
                if (regularObj != null) {
                    var feild = part;
                    var fieldInfo = GetObjFeildInfoCanNull (obj, feild);
                    if (fieldInfo != null)
                    {
                        //var value = JsonUtil.JsonDataToObject(valueType, valueJd);
                        var typedValue = TryConvertType(fieldInfo.FieldType, value);
                        fieldInfo.SetValue(obj, typedValue as object);
                    }
                    return;
                }
                throw new Exception ($"path: {PathToString()} type is {obj.GetType().Name}, which is unsupport set value");
            }
        }
    }

    public static object TryConvertType(Type targetType, object value)
    {
        if(value is JsonData)
        {
            var jdValue = value as JsonData;
            if(targetType == typeof(int))
            {
                return jdValue.ToInt();
            }
        }
        return value;
    }

    public static T GetValue<T> (object root, string path, T _default) 
    {
        nowPath.Clear ();
        var pathParts = path.Split ('.');
        var obj = root;
        for (int i = 0; i < pathParts.Length; i++) 
        {
            var part = pathParts[i];
            if (i != pathParts.Length - 1) 
            {
                // enter

                var isHiybirdDic = IsHybirdDic (obj);
                if (isHiybirdDic) 
                {
                    var filedOrKey = part;
                    var hibirdDic = obj as IDictionary;
                    obj = GotoHybirdDicChild (hibirdDic, filedOrKey);
                    continue;
                }
                var dic = obj as IDictionary;
                if (dic != null) 
                {
                    var key = part;
                    obj = GotoDicChild (dic, key);
                    continue;
                }
                var list = obj as IList;
                if (list != null) 
                {
                    var index = part;
                    obj = GotoListChild (list, index);
                    continue;
                }
                var regularObj = obj;
                if (regularObj != null) 
                {
                    var feild = part;
                    obj = GotoRegularObjChild (regularObj, feild);
                    continue;
                }
                throw new Exception ($"path: {PathToString()} type is {obj.GetType().Name}, which is unsupport  get child");
            } else 
            {
                // get
                if (obj == null) 
                {
                    Debug.LogError ("obj is null" + "  PathToString:" + PathToString ());
                }
                var isHiybirdDic = IsHybirdDic (obj);
                if (isHiybirdDic) 
                {
                    var filedOrKey = part;
                    var fieldInfo = GetObjFeildInfoCanNull (obj, filedOrKey);
                    if (fieldInfo != null) 
                    {
                        var result = fieldInfo.GetValue (obj);
                        return (T) result;
                    } 
                    else 
                    {
                        var hibirdDic = obj as IDictionary;
                        if (hibirdDic.Contains (filedOrKey)) 
                        {
                            var result = hibirdDic[filedOrKey];
                            if (result != null) 
                            {
                                return (T) result;
                            } 
                            else 
                            {
                                return _default;
                            }
                        }
                        else 
                        {
                            return _default;
                        }
                    }
                }

                var dic = obj as IDictionary;
                if (dic != null) 
                {
                    var key = part;
                    if (dic.Contains (key)) 
                    {
                        var result = dic[key];
                        Debug.Log ("key:" + key + "=>" + result.ToString ());

                        return (T) result;
                    } 
                    else 
                    {
                        return _default;
                    }
                }

                var list = obj as IList;
                if (list != null) 
                {
                    var index = int.Parse (part);
                    if (list.Count > index) 
                    {
                        var result = list[index];
                        return (T) result;
                    }
                    else 
                    {
                        return _default;
                    }
                }

                var regularObj = obj;
                if (regularObj != null) 
                {
                    var feild = part;
                    var fieldInfo = GetObjFeildInfoCanNull (obj, feild);
                    if (fieldInfo != null) 
                    {
                        var result = fieldInfo.GetValue (obj);
                        return (T) result;
                    } 
                    else 
                    {
                        return _default;
                    }
                }
                throw new Exception ($"path: {PathToString()} type is {obj.GetType().Name}, which is unsupport  set value");
            }
        }
        throw new Exception ($"code never reach here");
    }
    private static List<string> nowPath = new List<string> ();

    public static void Update (object root, string path, JsonData value) 
    {

        //Debug.Log ($"[DataTree] {{{root.GetType().Name}}}");
        //Debug.Log ($"[DataTree] '{path}' -> {value.ToJson()}");
        nowPath.Clear ();
        var pathParts = path.Split ('.');
        var obj = root;
        for (int i = 0; i < pathParts.Length; i++) 
        {
            var part = pathParts[i];
            if (i != pathParts.Length - 1) 
            {
                // enter
                var isHiybirdDic = IsHybirdDic (obj);
                if (isHiybirdDic) 
                {
                    var filedOrKey = part;
                    var hibirdDic = obj as IDictionary;
                    obj = GotoHybirdDicChild (hibirdDic, filedOrKey);
                    continue;
                }
                var dic = obj as IDictionary;
                if (dic != null) 
                {
                    var key = part;
                    obj = GotoDicChild (dic, key);
                    continue;
                }
                var list = obj as IList;
                if (list != null) 
                {
                    var index = part;
                    obj = GotoListChild (list, index);
                    continue;
                }
                var regularObj = obj;
                if (regularObj != null) 
                {
                    var feild = part;
                    obj = GotoRegularObjChild (regularObj, feild);
                    continue;
                }
                throw new Exception ($"path: {PathToString()} type is {obj.GetType().Name}, which is unsupport  get child");
            } 
            else 
            {

                // set
                var isHiybirdDic = IsHybirdDic (obj);
                if (isHiybirdDic) 
                {
                    var filedOrKey = part;
                    var hibirdDic = obj as IDictionary;
                    ModifyHybirdDic (hibirdDic, filedOrKey, value);
                    continue;
                }
                var dic = obj as IDictionary;
                if (dic != null) 
                {
                    var key = part;
                    SetDic (dic, key, value);
                    continue;
                }
                var list = obj as IList;
                if (list != null) 
                {
                    var index = part;
                    ModifyList (list, index, value);
                    continue;
                }
                var regularObj = obj;
                if (regularObj != null) 
                {
                    var feild = part;
                    ModifyRegularObj (regularObj, feild, value);
                    continue;
                }
                throw new Exception ($"path: {PathToString()} type is {obj.GetType().Name}, which is unsupport  set value");
            }

        }
    }

    private static void ModifyList (IList list, string index, JsonData valueJd) 
    {
        int indexInt; 
        {
            var success = int.TryParse (index, out indexInt);
            if (!success) 
            {
                throw new Exception ($"{PathToString()} is list, but got index: '{index}'");
            }
        }
        var type = list.GetType ();
        var valueType = type.GetGenericArguments () [0];
        var value = JsonUtil.JsonDataToObject (valueType, valueJd);
        list[indexInt] = value;
    }

    private static void SetDic (IDictionary dic, string key, JsonData valueJd) 
    {
        var type = FindTypeWichClosedByDicitonaryInExtensionChain (dic);
        var valueType = type.GetGenericArguments () [1];
        var value = JsonUtil.JsonDataToObject (valueType, valueJd);
        dic[key] = value;
    }

    private static void ModifyHybirdDic (IDictionary obj, string keyOrField, JsonData valueJd) 
    {
        var fieldInfo = GetObjFeildInfoCanNull (obj, keyOrField);
        if (fieldInfo != null) 
        {
            ModifyRegularObj (obj, keyOrField, valueJd);
        } 
        else 
        {
            SetDic (obj, keyOrField, valueJd);
        }
    }

    private static void ModifyRegularObj (object obj, string field, JsonData valueJd) 
    {
        var fieldInfo = GetObjFeildInfoCanNull (obj, field);
        if (fieldInfo == null) 
        {
            Debug.LogError ("fieldInfo is null:" + field);
            // user may do not want to save this field
            return;
        }
        var valueType = fieldInfo.FieldType;
        var value = JsonUtil.JsonDataToObject (valueType, valueJd);
        fieldInfo.SetValue (obj, value);
    }

    private static FieldInfo GetObjFeildInfoCanNull (object obj, string field) 
    {
        var type = obj.GetType ();
        var fieldInfo = type.GetField (field, BindingFlags.Public | BindingFlags.Instance);
        return fieldInfo;
    }

    private static string PathToString () 
    {
        var sb = new StringBuilder ();
        var first = true;
        foreach (var p in nowPath) 
        {
            if (first) 
            {
                first = false;
            } 
            else 
            {
                sb.Append (".");
            }
            sb.Append (p);
        }
        return sb.ToString ();
    }

    private static object GotoDicChild (IDictionary dic, string key) 
    {
        object ret;
        if (dic.Contains (key))
        {
            ret = dic[key];
        } 
        else 
        {
            var closedDicType = FindTypeWichClosedByDicitonaryInExtensionChain (dic);
            if (closedDicType == null) 
            {
                throw new Exception ($"path: '{PathToString()}.{key}' has no extension node is directly closed by generac type Dictionary<,>");
            }

            var keyType = closedDicType.GetGenericArguments () [0];
            if (keyType != typeof (string)) 
            {
                throw new Exception ("path: " + PathToString () + "." + key + " is a dic, but key type is not string");
            }

            var valueType = closedDicType.GetGenericArguments () [1];
            var obj = Activator.CreateInstance (valueType);
            dic[key] = obj;
            ret = obj;
        }
        nowPath.Add (key);
        return ret;
    }

    private static object GotoListChild (IList list, string index)
    {
        //Debug.LogError("GotoListChild:"+index);
        object ret;
        int indexInt;
        var success = int.TryParse (index, out indexInt);
        if (!success) 
        {
            throw new Exception ("path: " + PathToString () + " is list, but got index: " + index);
        }
        if (indexInt < list.Count && indexInt >= 0)
        {
            ret = list[indexInt];
        } else {
            throw new Exception ("path: " + PathToString () + " is list, now count is " + list.Count + ", but got index: " + indexInt);
        }
        nowPath.Add (index);
        return ret;
    }

    private static object GotoRegularObjChild (object obj, string field) 
    {
        var fieldInfo = GetObjFeildInfoCanNull (obj, field);
        if (fieldInfo == null) 
        {
            throw new Exception ($"path: '{PathToString()}' is a regular object of type '{obj.GetType().Name}', which not defines a public instance field '{field}'");
        }
        var ret = fieldInfo.GetValue (obj);
        if (ret == null) 
        {
            var childType = fieldInfo.FieldType;
            var child = Activator.CreateInstance (childType);
            fieldInfo.SetValue (obj, child);
            ret = child;
        }
        nowPath.Add (field);
        return ret;
    }

    private static object GotoHybirdDicChild (IDictionary obj, string fieldOrKey) 
    {
        var fieldInfo = GetObjFeildInfoCanNull (obj, fieldOrKey);
        if (fieldInfo != null) 
        {
            return GotoRegularObjChild (obj, fieldOrKey);
        }
        else 
        {
            return GotoDicChild (obj, fieldOrKey);
        }
    }

    private static Type FindTypeWichClosedByDicitonaryInExtensionChain (object obj) 
    {
        var type = obj.GetType ();
        while (type != null) 
        {
            var genericDefineType = type.IsGenericType ? type.GetGenericTypeDefinition () : null;
            if (genericDefineType != typeof (Dictionary<,>)) 
            {
                type = type.BaseType;
            } 
            else 
            {
                return type;
            }
        }
        return null;
    }

    private static bool IsHybirdDic (object obj) 
    {
        var type = obj.GetType ();
        var generacDefineType = type.IsGenericType ? type.GetGenericTypeDefinition () : null;
        if (generacDefineType != typeof (Dictionary<,>)) 
        {
            var dicClosedType = FindTypeWichClosedByDicitonaryInExtensionChain (obj);
            if (dicClosedType != null) 
            {
                return true;
            }
        }
        return false;
    }

}