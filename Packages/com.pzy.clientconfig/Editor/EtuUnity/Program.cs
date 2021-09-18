using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using CustomLitJson;
using System.Text;
using Ionic.Zlib;
using UnityEngine;

namespace etucli
{
    class Program
    {
        static string TAG = "etuclisharp";
        public static void Main(string[] args)
        {

// {
//             var json = Content.Test;
//             var t = JsonMapper.Instance.ToObject<TableResult>(json);
//             var toJson = JsonMapper.Instance.ToJson(t);
//             Debug.Log(toJson);
//             return;
// }

            // parse arg
            var paramJd = new JsonData();
            paramJd.SetJsonType(JsonType.Object);
            var optionList = new List<string>();

            Cmd.ReadOptions((key, value)=>
            {
                Debug.Log($"[{TAG}] {key}: {value}");
                switch(key)
                {
                    case "--option":
                        var optionString = value;
                        var parts = optionString.Split(',');
                        foreach(var one in parts)
                        {
                            optionList.Add(one);
                            Debug.Log("options:" + one);
                        }
                        break;
                    default:
                        key = key.TrimStart('-');
                        paramJd[key] = value;
                        break;
                }
            });
            if(paramJd.Count == 0)
            {
                paramJd["a"] = "b"; 
            }

            //var inputDir = args[0];
            var inputDir = "Assets/Editor/ClientConfigExcel";
            var resOutputDir = "Assets/Resources/clientConfigRes";
            var codeOutputDir = "Assets/ClientConfigGenerateCode";

            var cleanCache = false;
            if(args.Length > 2)
            {   
                cleanCache = args[1] == "true"; //bool.parse(args[1]);
            }
            if(cleanCache)
            {
                CacheManager.Clean();
                Debug.Log("[ETU] clean cache");
            }
    
            if(!Directory.Exists(inputDir))
            {
                throw new Exception("[ETU] directory: " + inputDir + " not exsists");
            }
            Debug.Log("[ETU] generate json from directory: " + inputDir);
            // create workspace
            //var workspace = inputDir + "/etu";
            var workspace = "ClientConfigTemp";

            if (Directory.Exists(workspace))
            {
                Directory.Delete(workspace, true);
            }
            Directory.CreateDirectory(workspace);

            Directory.CreateDirectory($"{workspace}/json");
            Directory.CreateDirectory($"{workspace}/code");
            Directory.CreateDirectory($"{workspace}/typescript");
            Directory.CreateDirectory($"{workspace}/go");
            Directory.CreateDirectory($"{workspace}/etuString");


            // build
            var result = new EtuBuildResult();
            DataMaker.Instance.Build(inputDir, result);
            
            // process option
            ProcessOption(result, optionList);

            // error
            if(result.exceptionList.Count > 0)
            {
                Debug.Log("发现异常:");
                foreach(var one in result.exceptionList)
                {
                    var fileName = one.fileName;
                    var sheetName = one.sheet;
                    var e = one.e;
                    Debug.Log($"{fileName}: {sheetName}: {e.Message}");
                }
            }

            // check fail
            if(result.failCount > 0)
            {
                throw new Exception("one or more excel file convert fail");
            }

            // add meta
            var tableList = "";
            foreach(var one in result.tableResultDic)
            {
                var name = one.Key;
                if(tableList != "")
                {
                    tableList += ",";
                }
                tableList += name;
            }
            paramJd["tableList"] = tableList;
            AddMeta(result, paramJd);

            // write each table to a diffrent json file
            foreach (var kv in result.tableResultDic)
            {
                var name = kv.Key;
                var jd = kv.Value.jd;
                var json = jd.ToJson();
                File.WriteAllText($"{workspace}/json/{name}.json", json);
            }

            //remove server table
            var newDic = new Dictionary<string, TableResult>();
            foreach (var kv in result.tableResultDic)
            {
                var name = kv.Key;
                var info = kv.Value;
                if(info.kind == TableKind.Both || info.kind == TableKind.Client)
                {
                    newDic[name] = info;
                }
            }
            result.tableResultDic = newDic;

            // write all table in one json file
            var package = new JsonData();
            foreach (var kv in result.tableResultDic)
            {
                var name = kv.Key;
                var jd = kv.Value.jd;
                package[name] = jd;
            }
            var packageJson = package.ToJson();
            File.WriteAllText($"{workspace}/StaticData.json", packageJson);

            // generate compression bytes
            {
                var gzipBytes = Ionic.Zlib.GZipStream.CompressString (packageJson);
                File.WriteAllBytes($"{workspace}/StaticData.zipedjson", gzipBytes);
            }

            // write each table in diffrent etu table formate string file
            foreach (var kv in result.tableResultDic)
            {
                var name = kv.Key;
                var jd = kv.Value.jd;
                var etuTableString = TableJsonDataToEtuTableString(jd);
                File.WriteAllText($"{workspace}/etuString/{name}.txt", etuTableString);
            }

            // write all table in one etu static data formate stirng file
            var staticDataEtuStringBuilder = new StringBuilder();
            var firstTable = true;
            foreach (var kv in result.tableResultDic)
            {
                var name = kv.Key;
                var jd = kv.Value.jd;
                var etuTableString = TableJsonDataToEtuTableString(jd);
                if(firstTable)
                {
                    firstTable = false;
                }
                else
                {
                    staticDataEtuStringBuilder.Append("[｜]");
                }
                staticDataEtuStringBuilder.Append($"{name}[｀]{etuTableString}");
            }
            var staticDataString = staticDataEtuStringBuilder.ToString();
            File.WriteAllText($"{workspace}/StaticData.txt", staticDataString);

            // generate compression bytes
            {
                var gzipBytes = Ionic.Zlib.GZipStream.CompressString (staticDataString);
                File.WriteAllBytes($"{workspace}/StaticData.byte", gzipBytes);
            }

            // write table code
            foreach(var kv in result.tableResultDic)
            {
                var tableName = kv.Key;
                var tableResult = kv.Value;
                var type = tableResult.tableType;
                if(type == TableType.Nkv)
                {
                    continue;
                }
                File.WriteAllText($"{workspace}/code/{tableResult.rowClazzName}Row.cs", tableResult.code);
            }

            // write static c# code
            var allPropertyCode = "";
            foreach(var kv in result.tableResultDic)
            {
                var tableName = kv.Key;
                var bigTableName = Util.FirstChaUp(tableName);
                var tableResult = kv.Value;
                var type = tableResult.tableType;
                var propertyCode = "";
                var kvValueType = tableResult.kvTableValueCsharpType;
                if(type == TableType.Normal || type == TableType.Array)
                {
                    var template = Content.TablePropertyTemplate;
                    template = template.Replace("{name}", tableName);
                    template = template.Replace("{Name}", bigTableName);
                    propertyCode = template;
                }
                else if(type == TableType.Nkv)
                {
                    var template = Content.KvTablePropertyTemplate;
                    template = template.Replace("{name}", tableName);
                    template = template.Replace("{Name}", bigTableName);
                    template = template.Replace("{type}", kvValueType);
                    propertyCode = template;
                }
                allPropertyCode += propertyCode;
            }
            var staticDataTemp = Content.StaticDataFrame;
            var code = staticDataTemp.Replace("{body}", allPropertyCode);
            File.WriteAllText($"{workspace}/code/ClientConfig.cs", code);

            // 代码
            {
                var from = workspace + "/code";
                //var to = "Assets/Game/ClientConfigGenerateCode";
                var to = codeOutputDir;
                var b = Directory.Exists(to);
                if (b)
                {
                    Directory.Delete(to, true);
                }
                Directory.Move(from, to);
            }

            // 资源
            {
                var from = workspace + "/etuString";
                //var to = "Assets/Game/Res/$clientConfigRes";
                var to = resOutputDir;
                var b = Directory.Exists(to);
                if (b)
                {
                    Directory.Delete(to, true);
                }
                Directory.Move(from, to);
            }

            UnityEditor.AssetDatabase.Refresh();
        }

        private static void AddMeta(EtuBuildResult result, JsonData jd)
        {
            var hasMeta = result.tableResultDic.ContainsKey("meta");
            if(hasMeta)
            {
                throw new Exception("find a table named meta, it's denied");
            }
            var tableResult = new TableResult();
            tableResult.code = "";  // key value no class row define
            tableResult.codeFileName = ""; 
            tableResult.kind = TableKind.Both;
            tableResult.kvTableValueCsharpType = "string";
            tableResult.name = "meta";
            tableResult.rowClazzName = "";
            tableResult.tableType = TableType.Nkv;
            tableResult.jd = jd;
            // add jd
            result.tableResultDic["meta"] = tableResult;
        }


        private static void ProcessOption(EtuBuildResult result, List<string> optionList)
        {
            var optionDic = new Dictionary<string, bool>();
            foreach(var option in optionList)
            {
                optionDic[option] = true;
            }
            foreach(var kv in result.tableResultDic)
            {
                var tableName = kv.Key;
                var tableReuslt = kv.Value;
                var root = tableReuslt.jd;
                Debug.Log("process option table: " + tableName);
                var foundOption = false;
                foreach(DictionaryEntry idAndRow in (IDictionary)root)
                {
                    var id = idAndRow.Key.ToString();
                    var rowOrString = idAndRow.Value as JsonData;
                    if(rowOrString.IsObject)
                    {
                        var row = rowOrString;
                        var removeKeyList = new List<string>();
                        var addFieldJdDic = new Dictionary<string, JsonData>();
                        foreach(DictionaryEntry fieldValue in (IDictionary)row)
                        {
                            var field = fieldValue.Key.ToString();
                            var value = fieldValue.Value as JsonData;
                            if(field.IndexOf("@") != -1)
                            {
                                if(!foundOption)
                                {
                                    foundOption = true;
                                    Debug.Log("found field: " + field);
                                }
                                var parts = field.Split('@');
                                var newField = parts[0];
                                var option = parts[1];
                                removeKeyList.Add(field);
                                // is option exsits
                                var exists = optionDic.ContainsKey(option);
                                if(exists)
                                {
                                    addFieldJdDic[newField] = value;
                                }
                            }
                        }

                        // 移除标记列
                        foreach (DictionaryEntry fieldValue in (IDictionary)row)
                        {
                            var field = fieldValue.Key.ToString();
                            var value = fieldValue.Value as JsonData;
                            if (field.StartsWith("$"))
                            {
                                removeKeyList.Add(field);
                            }
                        }

                        foreach (var removeOne in removeKeyList)
                        {
                            var dic = (IDictionary)row;
                            dic.Remove(removeOne);
                        }
                        foreach(var addOneKv in addFieldJdDic)
                        {
                            var field = addOneKv.Key;
                            var value = addOneKv.Value;
                            row[field] = value;
                        }
                    }
                }
            }
        }

        private static string TableJsonDataToEtuTableString(JsonData tableJd)
        {
            //Debug.Log("table:");
            //Debug.Log(tableJd.ToJson());
            var sb = new StringBuilder();
            var first = true;
            foreach(DictionaryEntry idAndRow in (IDictionary)tableJd)
            {
                var id = idAndRow.Key.ToString();
                var row = idAndRow.Value as JsonData;
                string rowJson = null;//row.ToJson();
                if(row.IsString)
                {
                    rowJson = row.ToString();
                }
                else
                {
                    rowJson = row.ToJson();
                }
                // 当row只有id和value字段时，认为是KV类型，rowJson的位置并不是json，而是值字符串
                //Debug.Log(rowJson);
                // if(row.IsObject)
                // {
                //     var dic = (IDictionary)row;
                //     if(dic != null && dic.Keys.Count == 2)
                //     {
                //         if(dic.Contains("id") && dic.Contains("value"))
                //         {
                //             rowJson = row["value"].ToString();
                //         }
                //     }
                // }

                var etuRowStringWithKey = $"{id}[`]{rowJson}";
                if(first)
                {
                    first = false;
                }
                else
                {
                    sb.Append("[|]");
                }
                sb.Append(etuRowStringWithKey);
            }
            var tableString = sb.ToString();
            return tableString;
        }
    }
}
