using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class StaticDataUtil
{
    static Dictionary<int, int> rowIdToItypeDic = new Dictionary<int, int>();

    public static int GetIType(int rowId)
    {
        if (rowId == 0)
        {
            return 1;
        }
        var b = rowIdToItypeDic.TryGetValue(rowId, out int cachedValue);
        if(b)
        {
            return cachedValue;
        }
        var itype = InternalGetItype(rowId);
        rowIdToItypeDic[rowId] = itype;
        return itype;
    }

    static int InternalGetItype(int rowId)
    {
        var request = new SearchRequest();
        request.id = rowId;
        request.needField = null;
        var info = StaticDataRuntime.SearchOrGetFormCache(request);
        var tableName = info.tableName;
        var tableItype = StaticDataRuntime.GetMetadata(tableName, "itype");
        if (tableItype != "")
        {
            return int.Parse(tableItype);
        }
        var row = info.row;
        var (itype, has) = ReflectionUtil.TryGetPropertyValue<int>(row, "IType");
        if (!has)
        {
            throw new Exception($"[StaticDataUtil] can't resolve itype (rowId: {rowId}, table: {tableName})");
        }
        return itype;
    }

    public static ITypeRow GetITypeRow(int rowId)
    {
        if(rowId == 0)
        {
            return StaticData.ITypeTable[1];
        }
        var itype = GetIType(rowId);
        var row = StaticData.ITypeTable[itype];
        return row;
    }

    public static T GetAnyFieldOfAnyRow<T>(int rowId, string proeprtyName, T @default = default)
    {
        var row = StaticDataRuntime.GetRow(rowId, proeprtyName, true);
        if(row == null)
        {
            return @default;
        }
        var (icon, has) = ReflectionUtil.TryGetPropertyValue<T>(row, proeprtyName);
        if (!has)
        {
            return @default;
        }
        else
        {
            return icon;
        }
    }

    public static string GetServerDataModel(int rowId)
    {
        if(rowId == 0)
        {
            return ServerDataModel.Role;
        }
        var row = GetITypeRow(rowId);
        var dataType = row.Model;
        return dataType;
}
}
