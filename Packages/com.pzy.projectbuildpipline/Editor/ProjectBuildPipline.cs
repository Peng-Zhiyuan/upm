using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

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

    static Type FindSchemaTypeByName(string name)
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

    public static void Build(string name)
    {
        var type = FindSchemaTypeByName(name);
        var schema = Activator.CreateInstance(type) as BuildSchema;
        schema.Build();
        Debug.Log($"[ProjectBuildPipline] schame '{name}' build compleate");
    }

   
}
