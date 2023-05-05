using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using CustomLitJson;
using System.Text;
using Ionic.Zlib;
using ICSharpCode.SharpZipLib.Zip;
using System.Runtime;
using System.Runtime.InteropServices;

namespace etucli
{
    class Program
    {
        static string TAG = "etuclisharp";
        static void Main(string[] args)
        {

// {
//             var json = Content.Test;
//             var t = JsonMapper.Instance.ToObject<TableResult>(json);
//             var toJson = JsonMapper.Instance.ToJson(t);
//             Console.WriteLine(toJson);
//             return;
// }
            
            // parse arg
            var extraInfo = new Dictionary<string, string>();
            var optionList = new List<string>();

            Cmd.ReadOptions((key, value)=>
            {
                Console.WriteLine($"[{TAG}] {key}: {value}");
                switch(key)
                {
                    case "--option":
                        var optionString = value;
                        var parts = optionString.Split(",");
                        foreach(var one in parts)
                        {
                            optionList.Add(one);
                            Console.WriteLine("options:" + one);
                        }
                        break;
                    default:
                        key = key.TrimStart('-');
                        extraInfo[key] = value;
                        break;
                }
            });

            if(!extraInfo.ContainsKey("version"))
            {
                throw new Exception("[etuclisharp] must post extra info -version");
            }

            var version = extraInfo["version"];

            var inputDir = args[0];
            var cleanCache = false;
            if(args.Length > 2)
            {   
                cleanCache = args[1] == "true"; //bool.parse(args[1]);
            }
            if(cleanCache)
            {
                CacheManager.Clean();
                Console.WriteLine("[ETU] clean cache");
            }
    
            if(!Directory.Exists(inputDir))
            {
                throw new Exception("[ETU] directory: " + inputDir + " not exsists");
            }
            Console.WriteLine("[ETU] generate json from directory: " + inputDir);
            // create workspace
            var workspace = inputDir + "/etu";
            if(Directory.Exists(workspace))
            {
                Directory.Delete(workspace, true);
            }
            Directory.CreateDirectory(workspace);

            // 调用内部构建
            var result = new EtuBuildResult();
            result.version = version;
            DataMaker.Instance.Build(inputDir, result);
            
            // 检查内部构建是否成功
            CheckBuildSuccess(result);

            // 处理 json 数据中的选项标记
            // 对于带有标记的字段
            // 1. 满足情况的会生成目标字段
            // 2. 不满足情况的会丢弃
            ProcessOption(result, optionList);

            // 添加 meta 表的生成信息
            GenerateMetaTable(result, extraInfo);

            // 为每个表生成 json 数据文件
            {
                var filenameToContentDic = JsonComponent.GenerateSperateJsonFile(result);
                var generateRoot = $"{workspace}/json";
                foreach(var kv in filenameToContentDic)
                {
                    var filename = kv.Key;
                    var content = kv.Value;
                    var path = $"{generateRoot}/{filename}";
                    WriteFile(path, content);
                }
            }

            // 生成所有表数据整合到一起的 json 数据文件
            {
                var filenameToContentDic = JsonComponent.GenerateAllInOneJsonData(result);
                var generateRoot = $"{workspace}/allInOneJson";
                foreach(var kv in filenameToContentDic)
                {
                    var filename = kv.Key;
                    var content = kv.Value;
                    var path = $"{generateRoot}/{filename}";
                    WriteFile(path, content);
                }
            }

            // 为每个表生成 etuStirng 数据文件
            {
                var filenameToContentDic = EtuStringComponent.GenerateSperateEtuStringFile(result);
                var generateRoot = $"{workspace}/etuString";
                foreach(var kv in filenameToContentDic)
                {
                    var filename = kv.Key;
                    var content = kv.Value;
                    var path = $"{generateRoot}/{filename}";
                    WriteFile(path, content);
                }
            }

            // 生成所有表数据整合到一起的 etuString 数据文件
            {
                var filenameToContentDic = EtuStringComponent.GenerateAllInOneEtuStringData(result);
                var generateRoot = $"{workspace}/allInOneEtuString";
                foreach(var kv in filenameToContentDic)
                {
                    var filename = kv.Key;
                    var content = kv.Value;
                    var path = $"{generateRoot}/{filename}";
                    WriteFile(path, content);
                }
            }

            // 生成 csharp 定义类型
            {
                var filenameToContentDic = CsharpComponent.GenerateCsharpCode(result);
                var generateRoot = $"{workspace}/csharp";
                foreach(var kv in filenameToContentDic)
                {
                    var filename = kv.Key;
                    var content = kv.Value;
                    var path = $"{generateRoot}/{filename}";
                    WriteFile(path, content);
                }
            }

            // 生成 typescript 定义类型
            {
                var filenameToContentDic = TypescriptCompoennt.GenerateTypescriptCode(result);
                var generateRoot = $"{workspace}/typescript";
                foreach(var kv in filenameToContentDic)
                {
                    var filename = kv.Key;
                    var content = kv.Value;
                    var path = $"{generateRoot}/{filename}";
                    WriteFile(path, content);
                }
            }

            // 生成 go 定义类型
            {
                var filenameToContentDic = GoCompoennt.GenerateGoCode(result);
                var generateRoot = $"{workspace}/go";
                foreach(var kv in filenameToContentDic)
                {
                    var filename = kv.Key;
                    var content = kv.Value;
                    var path = $"{generateRoot}/{filename}";
                    WriteFile(path, content);
                }
            }

            // 生成 lua 数据文件
            {
                var filenameToContentDic = LuaComponent.GenerateLuaData(result);
                var generateRoot = $"{workspace}/luaData";
                foreach(var kv in filenameToContentDic)
                {
                    var filename = kv.Key;
                    var content = kv.Value;
                    var path = $"{generateRoot}/{filename}";
                    WriteFile(path, content);
                }
            }

            // 生成统一模式的 proto 协议
            {
                var optionDic = new Dictionary<string, string>();
                optionDic["go_package"] = "./;Pb";
                var filenameToContentDic = ProtobufComponent.GenerateAllInOneProto(result, null, optionDic);
                var generateRoot = $"{workspace}/proto";
                foreach(var kv in filenameToContentDic)
                {
                    var filename = kv.Key;
                    var content = kv.Value;
                    var path = $"{generateRoot}/{filename}";
                    WriteFile(path, content);
                }
            }

            // 生成统一模式的 protobuf 数据
            {
                var jsonFile = $"{workspace}/allInOneJson/StaticData.json";
                var protoFile = $"{workspace}/proto/StaticData.proto";
                var protoType = "StaticData";
                var outFile = $"{workspace}/allInOneBuffer/StaticData.buffer";
                CreateParentIfNeed(outFile);
                ProtobufComponent.GenerateProtoData(jsonFile, protoFile, protoType, outFile);
            }

            // 生成每个表分开模式的 proto 协议
            {
                var optionDic = new Dictionary<string, string>();
                var filenameToContentDic = ProtobufComponent.GenerateSeparateProto(result);
                var generateRoot = $"{workspace}/separatedProto";
                foreach(var kv in filenameToContentDic)
                {
                    var filename = kv.Key;
                    var content = kv.Value;
                    var path = $"{generateRoot}/{filename}";
                    WriteFile(path, content);
                }
            }

            // 生成每个表分开的 protobuf 数据
            {
                var optionDic = new Dictionary<string, string>();
                var filenameToContentDic = ProtobufComponent.GenerateSeparateProtoData(result, workspace, $"{workspace}/separatedProto");
                var generateRoot = $"{workspace}/separatedBuffer";
                foreach(var kv in filenameToContentDic)
                {
                    var filename = kv.Key;
                    var content = kv.Value;
                    var path = $"{generateRoot}/{filename}";
                    WriteFile(path, content);
                }
            }

            // 生成每个表分开的 buffer 数据(压缩到一个文件)
            {
                var fileRoot = $"{workspace}/separatedBuffer"; 
                var generateFile = $"{workspace}/separatedBufferZip/separatedBuffer.zip";
                CreateParentIfNeed(generateFile);
                var code = ExecUtil.Run("zip", $"-r {generateFile} .", false, fileRoot);
                if(code != 0)
                {
                    throw new Exception("error in zip");
                }
            }

            // 生成每个表分开的模式下的 csharp 运行时代码
            {
                // 生成菜单类代码
                {
                    var filenameToContentDic = ProtobufComponent.GenerateSeparateModeCsharpMenuClassCode(result);
                    var generateRoot = $"{workspace}/separatedProtoModeCsharp";
                    foreach(var kv in filenameToContentDic)
                    {
                        var filename = kv.Key;
                        var content = kv.Value;
                        var path = $"{generateRoot}/{filename}";
                        WriteFile(path, content);
                    }
                }

                // 生成 Proto 类型代码
                {
                    var protoDir = $"{workspace}/separatedProto";
                    //var protoParentDir = Path.GetDirectoryName(protoDir);
                    var protoFileList = Directory.GetFiles(protoDir);
                    //var tool = LocalTools.RequireTool("protogen", "it proveded by npm package 'protogen'");
                    var protoGenPath = Util.FindScriptDir() + "/tools/protogen/protogen.exe";
                    var genCsharpRootDir = $"{workspace}/separatedProtoModeCsharp/protoClass";
                    foreach(var one in protoFileList)
                    {
                        var fileName = Path.GetFileName(one);
                        if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        {
                            var mono = LocalTools.RequireTool("mono");
                            var code = ExecUtil.Run(mono, $"{protoGenPath} --csharp_out={genCsharpRootDir} {fileName}", false, protoDir);
                            if(code != 0)
                            {
                                throw new Exception("error in protogen");
                            }
                        }
                        else
                        {
                            var code = ExecUtil.Run(protoGenPath, $"--csharp_out={genCsharpRootDir} {fileName}", false, protoDir);
                            if(code != 0)
                            {
                                throw new Exception("error in protogen");
                            }
                        }
                    }
                }
            }

            
            // 生成每个表分开的模式下的 ts 运行时代码
            {
                // 生成菜单类代码
                {
                    var filenameToContentDic = ProtobufComponent.GenerateSeparateModeTypeScriptMenuClassCode(result);
                    var generateRoot = $"{workspace}/separatedProtoModeTypescript";
                    foreach(var kv in filenameToContentDic)
                    {
                        var filename = kv.Key;
                        var content = kv.Value;
                        var path = $"{generateRoot}/{filename}";
                        WriteFile(path, content);
                    }
                }
                // 生成 Proto 类型代码
                {
                    var protoDir = $"{workspace}/separatedProto";
                    var jsOutFile = $"{workspace}/separatedProtoModeTypescript/StaticDataProtoClasses.js";
                    CreateParentIfNeed(jsOutFile);

                    // 创建 js 文件
                    {
                        //var execPath = "C:/Users/igg/AppData/Roaming/npm/pbjs.cmd";
                        var execPath = LocalTools.RequireTool("pbjs", "this tools provide by npm protobufjs pakcgae");
                        var code = ExecUtil.Run(execPath, $"-t static-module -w commonjs -o {jsOutFile} *.proto", false, protoDir);
                        if(code != 0)
                        {
                            throw new Exception("error in pbjs");
                        }
                    }

                    // 创建 Typing 模块
                    {
                        var typingFilePath = $"{workspace}/separatedProtoModeTypescript/StaticDataProtoClasses.d.ts";
                        CreateParentIfNeed(typingFilePath);

                        //var execPath = "C:/Users/igg/AppData/Roaming/npm/pbts.cmd";
                        var execPath = LocalTools.RequireTool("pbts", "this tools provide by npm protobufjs pakcgae");
                        var code = ExecUtil.Run(execPath, $"-o {typingFilePath} {jsOutFile}", false, protoDir);
                        if(code != 0)
                        {
                            throw new Exception("error in pbts");
                        }
                    }
                }
            }

           
        }

        static void WriteFile(string path, object stringOrBuffer)
        {
            // 确保父目录已创建
            var parent = Path.GetDirectoryName(path);
            if(!Directory.Exists(parent))
            {
                Directory.CreateDirectory(parent);
            }

            if(stringOrBuffer is string)
            {
                File.WriteAllText(path, (string)stringOrBuffer);
            }
            else if(stringOrBuffer is byte[])
            {
                File.WriteAllBytes(path, (byte[])stringOrBuffer);
            }
            else
            {
                throw new Exception("unsuport type");
            }
        }

        static void CreateDirectoryIfNeed(string path)
        {
            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        static void CreateParentIfNeed(string path)
        {
            var parent = Path.GetDirectoryName(path);
            if(!Directory.Exists(parent))
            {
                Directory.CreateDirectory(parent);
            }
        }

        static void GenerateMetaTable(EtuBuildResult result, Dictionary<string, string> extraInfo)
        {
            var paramJd = new JsonData();
            paramJd.SetJsonType(JsonType.Object);

            foreach(var kv in extraInfo)
            {
                var key = kv.Key;
                var value = kv.Value;
                paramJd[key] = value;
            }

            var sb = new StringBuilder();
            var first = true;
            foreach(var one in result.tableResultDic)
            {
                var name = one.Key;
                if(first)
                {
                    first = false;
                }
                else
                {
                    sb.AppendLine(",");
                }
                sb.Append(name);
            }
            paramJd["tableList"] = sb.ToString();
            AddMeta(result, paramJd);
        }

        static void CheckBuildSuccess(EtuBuildResult result)
        {
            // error
            if(result.exceptionList.Count > 0)
            {
                Console.WriteLine("发现异常:");
                foreach(var one in result.exceptionList)
                {
                    var fileName = one.fileName;
                    var sheetName = one.sheet;
                    var e = one.e;
                    Console.WriteLine($"{fileName}: {sheetName}: {e.Message}");
                }
            }

            // check fail
            if(result.failCount > 0)
            {
                throw new Exception("one or more excel file convert fail");
            }
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
            tableResult.keyCsharpType = "string";
            tableResult.kvTableValueCsharpType = "string";
            tableResult.name = "meta";
            tableResult.rowClazzName = "";
            tableResult.tableType = TableType.Nkv;
            tableResult.jd = jd;
            // add jd
            result.tableResultDic["meta"] = tableResult;
        }


        // 将 json 中带有选项的字段，根据情况丢弃或者生成目标字段
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
                Console.WriteLine("process option table: " + tableName);
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
                                    Console.WriteLine("found field: " + field);
                                }
                                var parts = field.Split("@");
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
                        foreach(var removeOne in removeKeyList)
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


        
    }
}
