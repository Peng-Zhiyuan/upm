using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Reflection;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;


public static class StaticDataRuntime
{

    static Dictionary<string, byte[]> tableNameToBufferDic = new Dictionary<string, byte[]>();

    static bool isInitalizedInEditMode;
    static void OnEditModePopTableData(string tableName)
    {
        if (!isInitalizedInEditMode)
        {
            ReloadInEditMode();
            isInitalizedInEditMode = true;
        }
    }
    public static Func<byte[]> editModeRequestDataHandler;
    public static void ReloadInEditMode()
    {
        if (editModeRequestDataHandler == null)
        {
            throw new Exception("[StaticDataRuntime] editModeRequestDataHandler not set");
        }

        var fileData = editModeRequestDataHandler.Invoke();
        var tableNameToBufferDic = UnzipFileDataToTableDataDic(fileData);
        Reset(tableNameToBufferDic);
        Debug.Log("[StaticDataRuntime] data loaded");
    }

    public static bool dataFilled;

    public static byte[] PopTableData(string tableName)
    {
        if (!Application.isPlaying)
        {
            OnEditModePopTableData(tableName);
        }

        if (tableNameToBufferDic.ContainsKey(tableName))
        {
            var buffer = tableNameToBufferDic[tableName];
            tableNameToBufferDic.Remove(tableName);
            return buffer;
        }
        else
        {
            throw new Exception($"[StaticDataRuntime] table '{tableName}' buffer not exsists. maybe already poped");
        }
    }




    public static event Action OnRequestedHandler;
    public static Func<Task<byte[]>> requestDataHandler;
    public static async Task ReloadAsync()
    {
        if (requestDataHandler == null)
        {
            throw new Exception("[StaticDataRuntime] requestDataHandler not set");
        }

        var task = requestDataHandler.Invoke();
        var fileData = await task;
        var tableNameToBufferDic = UnzipFileDataToTableDataDic(fileData);
        Reset(tableNameToBufferDic);
        Debug.Log("[StaticDataRuntime] data loaded");
        
        OnRequestedHandler?.Invoke();
    }

    static Dictionary<string, byte[]> UnzipFileDataToTableDataDic(byte[] fileData)
    {

        //ICSharpCode.SharpZipLib.Zip.ZipConstants.DefaultCodePage = 437;
        ZipStrings.CodePage = 437;

        var steam = new MemoryStream(fileData);
        var zipFile = new ZipFile(steam);
        var e = zipFile.GetEnumerator();
        var nameToBuffer = new Dictionary<string, byte[]>();
        while (e.MoveNext())
        {
            var entry = e.Current as ZipEntry;
            if (entry.IsFile)
            {
                var size = entry.Size;
                var name = entry.Name;
                using (var stream = zipFile.GetInputStream(entry))
                {
                    var reader = new BinaryReader(stream);
                    var buffer = reader.ReadBytes((int)size);
                    var nameWithoutExtension = Path.GetFileNameWithoutExtension(name);
                    nameToBuffer[nameWithoutExtension] = buffer;
                }
            }
        }
        steam.Dispose();
        return nameToBuffer;

    }

    static void Reset(Dictionary<string, byte[]> theTableNameToBufferDic)
    {
        Clean();
        foreach (var kv in theTableNameToBufferDic)
        {
            var tableName = kv.Key;
            var buffer = kv.Value;
            tableNameToBufferDic[tableName] = buffer;
        }
        dataFilled = true;
        Debug.Log("[StaticDataRuntime] reseted");
    }

    public static void Clean()
    {
        _dataVersion = null;

        // 移除 buffer 暂存
        tableNameToBufferDic.Clear();

        // 移除已生成的表对象
        var genType = GenType;
        var files = genType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        foreach (var f in files)
        {
            var name = f.Name;
            if (name.StartsWith("_") && name.EndsWith("Table"))
            {
                f.SetValue(null, null);
                //Debug.Log($"[ProtoStaticDataRuntime] clean field '{name}'");
            }
        }
    }

    static List<PropertyInfo> _tablePropertyList;
    static List<PropertyInfo> AllTablePropertyList
    {
        get
        {
            if(_tablePropertyList == null)
            {
                _tablePropertyList = new List<PropertyInfo>();
                var genType = GenType;
                var list = genType.GetProperties(BindingFlags.Public | BindingFlags.Static);
                foreach(var p in list)
                {
                    var name = p.Name;
                    if(name.EndsWith("Table"))
                    {
                        _tablePropertyList.Add(p);
                    }
                }
            }
            return _tablePropertyList;
        }
    }

    static Type _genType;
    static Type GenType
    {
        get
        {
            if(_genType == null)
            {
                var assembly = MainAssembly;
                _genType = assembly.GetType("StaticData");
            }
            return _genType;
        }
    }

    static Assembly _mainAssembly;
    static Assembly MainAssembly
    {
        get
        {
            if (_mainAssembly == null)
            {
                var thisType = typeof(StaticDataRuntime);
                if (thisType.Assembly.GetName().Name == "Assembly-CSharp")
                {
                    _mainAssembly = thisType.Assembly;
                }
                else
                {
                    var list = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var one in list)
                    {
                        var name = one.GetName().Name;
                        if (name == "Assembly-CSharp")
                        {
                            _mainAssembly = one;
                            break;
                        }
                    }
                }
            }
            return _mainAssembly;
        }
    }

    static string _dataVersion;
    public static string DataVersion
    {
        get
        {
            if (_dataVersion == null)
            {
                try
                {
                    _dataVersion = StaticData.MetaTable["version"];
                }
                catch
                {
                    _dataVersion = "unknown";
                }
            }
            return _dataVersion;
        }
    }


    public static void AddFirstGroupTable(string tableName)
    {
        var lower = tableName.ToLower();
        firstGroupTableNameSet.Add(lower);
    }

    static HashSet<string> firstGroupTableNameSet = new HashSet<string>();

    static List<PropertyInfo> _firstGroupTablePropertyList;
    static List<PropertyInfo> FirstGroupTablePropertyList
    {
        get
        {
            if (_firstGroupTablePropertyList == null)
            {
                _firstGroupTablePropertyList = new List<PropertyInfo>();
                var genType = GenType;
                var list = genType.GetProperties(BindingFlags.Public | BindingFlags.Static);
                foreach (var p in list)
                {
                    var name = p.Name;
                    if (name.EndsWith("Table"))
                    {
                        var tableWrapper = p.GetValue(null);
                        var tableName = ReflectionUtil.GetField<string>(tableWrapper, "tableName");
                        var lowerTableName = tableName.ToLower();
                        if(firstGroupTableNameSet.Contains(lowerTableName))
                        {
                            _firstGroupTablePropertyList.Add(p);
                        }
                    }
                }
            }
            return _firstGroupTablePropertyList;
        }
    }

    static (bool found, object row, string tableName) SearchInTablePropertyList(List<PropertyInfo> list, SearchRequest request)
    {
        foreach (var p in list)
        {
            var valueType = p.PropertyType;
            var genericTypeList = valueType.GenericTypeArguments;
            var tableType = genericTypeList[0];
            var keyType = genericTypeList[1];
            var elementType = genericTypeList[2];
            if (keyType == typeof(int))
            {
                if (elementType != typeof(int))
                {
                    var tableWrapper = p.GetValue(null);
                    var row = ReflectionUtil.CallMethod<object>(tableWrapper, "TryGet", new object[] { request.id, null });
                    if (row != null)
                    {
                        if(!string.IsNullOrEmpty(request.needField))
                        {
                            var b = HasProperty(row, request.needField);
                            if(!b)
                            {
                                continue;
                            }
                        }
                        var tableName = ReflectionUtil.GetField<string>(tableWrapper, "tableName");
                        return (true, row, tableName);
                    }
                }
            }
        }
        return (false, null, null);
    }

    public static bool HasProperty(object obj, string propertyName)
    {
        var type = obj.GetType();
        var propertyInfo = type.GetProperty(propertyName);
        if(propertyInfo != null)
        {
            return true;
        }
        return false;
    }


    static SearchResult ActulySerach(SearchRequest request)
    {
        var list = FirstGroupTablePropertyList;
        var ret = SearchInTablePropertyList(list, request);
        if(!ret.found)
        {
            var list2 = AllTablePropertyList;
            ret = SearchInTablePropertyList(list2, request);
        }
        var info = new SearchResult();
        info.tableName = ret.tableName;
        info.row = ret.row;
        return info;
    }

    static Dictionary<int, SearchResult> requestHashToResultDic = new Dictionary<int, SearchResult>();
    public static SearchResult SearchOrGetFormCache(SearchRequest request)
    {
        var hash = request.GetHashCode();
        var b = requestHashToResultDic.TryGetValue(hash, out var cachedResult);
        if (b)
        {
            return cachedResult;
        }

        var result = ActulySerach(request);
        requestHashToResultDic[hash] = result;
        return result;
    }


    public static object GetRow(int rowId, string needPropertyName = null, bool allowNotFound = false)
    {
        var request = new SearchRequest();
        request.id = rowId;
        request.needField = needPropertyName;
        var info = SearchOrGetFormCache(request);
        if (info.row == null)
        {
            if (!allowNotFound)
            {
                if (string.IsNullOrEmpty(needPropertyName))
                {
                    throw new GameException(ExceptionFlag.None, $"[StaticDataRuntime] not found id `{rowId}` ", "STATIC_DATA_ID_NOT_FOUND");
                }
                else
                {
                    throw new GameException(ExceptionFlag.None, $"[StaticDataRuntime] not found id `{rowId}` with property `{needPropertyName}`", "STATIC_DATA_ID_NOT_FOUND");
                }

            }
        }
        return info.row;
    }

    static Dictionary<string, Dictionary<string, string>> tableNameToMetadataDic = new Dictionary<string, Dictionary<string, string>>();
    public static void AddMetadata(string tableName, string key, string value)
    {
        tableName = tableName.ToLower();
        var tableMetadataDic = DictionaryUtil.GetOrCreateDic(tableNameToMetadataDic, tableName);
        tableMetadataDic[key] = value;
    }

    public static string GetMetadata(string tableName, string key)
    {
        tableName = tableName.ToLower();
        var b = tableNameToMetadataDic.TryGetValue(tableName, out var tableMetaDic);
        if(!b)
        {
            return "";
        }
        var bb = tableMetaDic.TryGetValue(key, out var value);
        if(!bb)
        {
            return "";
        }
        return value;
    }

}