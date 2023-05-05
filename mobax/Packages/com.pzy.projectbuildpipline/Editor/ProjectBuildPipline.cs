using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using CustomLitJson;
using System.Linq;
using System.IO;

public static class ProjectBuildPipline 
{

    static List<Type> _schemaTypeList;
    public static List<Type> SchemaTypeList
    {
        get
        {
            if(_schemaTypeList == null)
            {
                var list = ReflectionUtil.GetSubClassesInAllAssemblies<BuildSchema>();
                _schemaTypeList = list;
            }
            return _schemaTypeList;
        }
    }

    public static List<ParamAttribute> GetSchemaParamList(string schemaName)
    {
        var ret = new List<ParamAttribute>();
        var type = FindSchemaTypeByName(schemaName);
        var attributeList = type.GetCustomAttributes(typeof(ParamAttribute), true);
        if (attributeList == null || attributeList.Length == 0)
        {
            return ret;
        }
        foreach(var one in attributeList)
        {
            ret.Add(one as ParamAttribute);
        }
      
        return ret;
    }

    public static Type FindSchemaTypeByName(string name)
    {
        foreach(var schema in SchemaTypeList)
        {
            if(schema.Name == name)
            {
                return schema;
            }
        }
        throw new Exception($"[ProjectBuildPipline] Schema '{name}' not found");
    }

    static Dictionary<string, string> NormalizeParamDic(string schemaName, Dictionary<string, string> paramDic)
    {
        var newDic = new Dictionary<string, string>();
        var attrList = GetSchemaParamList(schemaName);
        foreach(var attr in attrList)
        {
            var name = attr.key;
            var defaultValue = attr.defaultValue;
            var value = DictionaryUtil.TryGet(paramDic, name, defaultValue);
            newDic[name] = value;
        }
        return newDic;
    }

    public static void Build(string name, Dictionary<string, string> originParamDic = null)
    {
        if(originParamDic == null)
        {
            originParamDic = new Dictionary<string, string>();
        }
        //WriteBuildInfoFile(originParamDic);
        var type = FindSchemaTypeByName(name);
        var schema = Activator.CreateInstance(type) as BuildSchema;
        var schemaNeedParamDic = NormalizeParamDic(name, originParamDic);
        schema.Build(schemaNeedParamDic);
        Debug.Log($"[ProjectBuildPipline] schame '{name}' build compleate");
    }

  

    //static void WriteBuildInfoFile(Dictionary<string, string> dic)
    //{
    //    var json = JsonMapper.Instance.ToJson(dic);
    //    json = JsonUtil.Buitify(json);
    //    var path = "Assets/Resources/build-info.json";
    //    var dir = Path.GetDirectoryName(path);
    //    if(!Directory.Exists(dir))
    //    {
    //        Directory.CreateDirectory(dir);
    //    }
    //    File.WriteAllText(path, json);
    //    Debug.Log("[ProjectBuildPipline] build-info.json: " + json);
    //    AssetDatabase.Refresh();

    //}




}
