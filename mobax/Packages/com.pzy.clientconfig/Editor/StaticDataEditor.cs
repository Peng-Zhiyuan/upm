//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//using System.Text;
//using CustomLitJson;

//[CustomEditor(typeof(ClientConfig))]
//public class ClientConfigEditor : Editor
//{
//    ClientConfig Component
//    {
//        get
//        {
//            return target as ClientConfig;
//        }
//    }

//    private static List<string> tableNameList = null;
//    private static List<string> TableNameList
//    {
//        get
//        {
//            if(tableNameList == null)
//            {
//                var type = typeof(ClientConfig);
//                var propertyList = type.GetProperties();
//                tableNameList = new List<string>();
//                foreach(var p in propertyList)
//                {
//                    var name = p.Name;
//                    var isTable = name.EndsWith("Table");
//                    if(isTable)
//                    {
//                        var tableName = name.Substring(0, name.Length - "Table".Length);
//                        tableNameList.Add(tableName);
//                    }
//                }
//            }
//            return tableNameList;
//        }
//    }

//    private static Dictionary<string, bool> foldoutDic = new Dictionary<string, bool>();
//    private static bool IsFoldout(string tableName)
//    {
//        if(foldoutDic.ContainsKey(tableName))
//        {
//            return foldoutDic[tableName];
//        }
//        return false;
//    }
//    private static void SetFoldout(string tableName, bool b)
//    {
//        foldoutDic[tableName] = b;
//    }

//    private object GetTableByName(string name)
//    {
//        var type = typeof(ClientConfig);
//        var property = type.GetProperty($"{name}Table");
//        var table = property.GetValue(Component);
//        return table;
//    }


//    public override void OnInspectorGUI()
//    {
//        var list = TableNameList;
//        foreach(var name in list)
//        {
//            var isFoldout = IsFoldout(name);
//            var b = EditorGUILayout.Foldout(isFoldout, name);
//            if(b != isFoldout)
//            {
//                SetFoldout(name, b);
//            }
//            if(b)
//            {
//                var table = GetTableByName(name);
//                var type = table.GetType();
//                var defineType = type.GetGenericTypeDefinition();
//                var isKeyValyeTable = (defineType == typeof(KeyValueTable<>));
//                if(!isKeyValyeTable)
//                {
//                    var genericTypeDefine = type.GetGenericTypeDefinition();
//                    if(genericTypeDefine == typeof(Table<>))
//                    {
//                        DrawGenericTable(table);
//                    }
//                }
//                else
//                {
//                    DrawKeyValueTable(table);
//                }
                

//                // var isGenericType = type.IsGenericType;
//                // var genericTypeList = type.GenericTypeArguments;
//                //EditorGUILayout.LabelField($"genericTypeDefine: {genericTypeDefine}");
//            }
//            //EditorGUILayout.LabelField(name);
//        }

//    }

//    private void DrawGenericTable(object table)
//    {
//        // type implete from Table<TRow>
//        var genericType = table.GetType();
//        var genericParamType = genericType.GenericTypeArguments[0];
//        var property = genericType.GetProperty("RowList");
//        var rowList = property.GetValue(table) as IList;
//        EditorGUILayout.LabelField($"C# Type: Table<{genericParamType.Name}>");
//        foreach(var row in rowList)
//        {
//            var json = JsonMapper.Instance.ToJson(row);
//            EditorStyles.label.wordWrap = true;
//            EditorGUILayout.LabelField(json);
//        }
//    }

//    private void DrawKeyValueTable(object table)
//    {
//        // type implete from KeyValueTable<TValue>
//        var genericType = table.GetType();
//        var genericParamType = genericType.GenericTypeArguments[0];
//        var valueTypeName = genericParamType.Name;
//        if(valueTypeName == "Single")
//        {
//            valueTypeName = "float";
//        }
//        var field = genericType.GetField("dic");
//        var dic = field.GetValue(table) as IDictionary;
//        EditorGUILayout.LabelField($"C# Type: KeyValueTable<{valueTypeName}>");
//        foreach(dynamic kv in dic)
//        {
//            var key = kv.Key;
//            var value = kv.Value;
//            EditorGUILayout.LabelField($"{key}: {value}");
//        }

//        // var kvtable = table as KeyValueTable;
//        // EditorGUILayout.LabelField($"C# Type: KeyValueTable");
//        // foreach(var kv in kvtable.dic)
//        // {
//        //     var key = kv.Key;
//        //     var value = kv.Value;
//        //     EditorGUILayout.LabelField($"{key}: {value}");
//        // }

//    }
//    //     // stack
//    //     EditorGUILayout.PrefixLabel("Stack");
//    //    // EditorGUI.indentLevel++;
//    //     EditorGUI.BeginDisabledGroup(true);
//    //     for(var i = 0; i < list.Length; i++)
//    //     {
//    //         var page = list[i];
//    //         EditorGUILayout.ObjectField((list.Length - 1 - i).ToString(), page, typeof(Page), true);
//    //     }
        
//    //     EditorGUILayout.Separator();
//    //     EditorGUILayout.Separator();
//    //     //EditorGUI.indentLevel--;
//    //     EditorGUILayout.PrefixLabel("Pool");
//        // EditorGUILayout.Separator();
//        // EditorGUILayout.PrefixLabel("None");
        

//        // pool
//        // foreach(var kv in Component.pagePool.dic)
//        // {
//        //     var name = kv.Key;
//        //     var instanceQueue = kv.Value;
//        //     var firstPage = instanceQueue.Peek();
//        //     EditorGUILayout.ObjectField(name, firstPage, typeof(Page), false);

//        // }


//        // EditorGUI.EndDisabledGroup();

//        // if (GUI.changed)
//        // {
//        //     EditorUtility.SetDirty(target);
//        // }
//        // serializedObject.ApplyModifiedProperties ();
    

//}
