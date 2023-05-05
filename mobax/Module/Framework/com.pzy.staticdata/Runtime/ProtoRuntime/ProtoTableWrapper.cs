using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProtoBuf;
using System.IO;
using UnityEngine.Scripting;
using System;
public class ProtoTableWrapper<TTable, TKey, TValue>
{
    public string tableName;
    public TTable table;

    public ProtoTableWrapper(string tableName, byte[] buffer)
    {
        this.tableName = tableName;
        var stream = new MemoryStream(buffer);
        //Debug.Log($"[ProtoTableWrapper] deserilize: {tableName}");
        try
        {
            this.table = Serializer.Deserialize<TTable>(stream);
        }
        catch
        {
            var codeVersion = StaticData.originDataVersion;
            var dataVersion = StaticDataRuntime.DataVersion;
            var msg = $"table `{tableName}` deserialize error, code version; {codeVersion}, data version: {dataVersion}";
            var ee = new Exception(msg);
            throw ee;
        }
        //Debug.Log($"[ProtoTableWrapper] deserilize: {tableName} complete");
        stream.Dispose();
    }

    // 此函数用于指示 AOT 编译器预先进行类型生成
    // 虽然没有任何调用，但是不可移除
    [Preserve]
    object AOTRef()
    {
        var noUse = new ProtoBuf.Serializers.MapDecoratorAOTRef<Dictionary<TKey, TValue>, TKey, TValue>();
        return noUse;
    }

    Dictionary<TKey, TValue> _tableRootDic;
    Dictionary<TKey, TValue> TableRootDic
    {
        get
        {
            if(_tableRootDic == null)
            {
                //Debug.Log($"[ProtoTableWrapper] will get tableRootDic Dics: {tableName}");
                var type = table.GetType();
                var propertyInfo = type.GetProperty("Dics");
                _tableRootDic = propertyInfo.GetValue(table) as Dictionary<TKey, TValue>;
                //Debug.Log($"[ProtoTableWrapper] get tableRootDic: {tableName}");
            }
            return _tableRootDic;
        }
    }

    public TValue this[TKey index]
    {
        get
        {
            var dic = this.TableRootDic;
            if(!dic.ContainsKey(index))
            {
                throw new Exception($"[ProtoTableWrapper] table '{this.tableName}' not contains a key '{index}'");
            }
            var ret = dic[index];
            return ret;
        }
    }

    public TValue TryGet(TKey key, TValue defaultValue = default(TValue))
    {
        if(TableRootDic.ContainsKey(key))
        {
            var ret = TableRootDic[key];
            return ret;
        }
        else
        {
            return defaultValue;
        }
    }

    public bool ContainsKey(TKey key)
    {
        var root = this.TableRootDic;
        var b = root.ContainsKey(key);
        return b;
    }

    public int Count
    {
        get
        {
            var root = this.TableRootDic;
            var count = root.Count;
            return count;
        }
    }

    List<TValue> _elementList;
    public List<TValue> ElementList
    {
        get
        {
            if(_elementList == null)
            {
                var list = new List<TValue>();
                var root = this.TableRootDic;
                foreach(var kv in root)
                {
                    var value = kv.Value;
                    list.Add(value);
                }
                _elementList = list;
            }
            return _elementList;
        }
    }
}
