using System.Collections.Generic;
using System.IO;

public class Util
{
    public static string FindScriptDir()
    {
        //var type = typeof(Util);
        //string currentDirectory = Path.GetDirectoryName(type.Assembly.Location);
        //var programFilePath = FindFileInParent(currentDirectory, "Program.cs");
        //var root = Path.GetDirectoryName(programFilePath);
        //return root;
        //return "Assets/Editor/-ClientConfig/EtuUnity";
        //return "Assets/Subsystems/-ClientConfig/Editor/EtuUnity";
        return "Packages/com.pzy.clientconfig/Editor/EtuUnity";
    }

    public static string FindFileInParent(string searchStartDir, string fileName)
    {
        var hot = searchStartDir;
        here:
        var path = Path.Combine(hot, fileName);
        var b = File.Exists(path);
        if(b)
        {
            return path;
        }
        var newHot = Path.GetDirectoryName(hot);
        if(newHot == hot)
        {
            return "";
        }
        hot = newHot;
        goto here;
    }

    public static string FirstChaUp(string name)
    {
        var ch = name[0];
        var bigCh = char.ToUpper(ch);
        var post = name.Substring(1);
        var final = bigCh + post;
        return final;
    }

    public static DataObject CreateTable(List<DataObject> rowList, TableType tableType)
    {
        if(tableType == TableType.Array)
        {
            var table = new DataObject(null);
            var dic = new Dictionary<string, RjCollection>();
            foreach (var row in rowList)
            {
                var name = row.name;
                var collection = GetDictionaryCollectionSafy(dic, name);
                row.name = null;
                collection.AddElement(row);
            }
            foreach (var kv in dic)
            {
                var id = kv.Key;    // like 80001
                var collection = kv.Value;
                collection.m_key = "Coll";
                var warpObj = new DataObject(id);
                warpObj.AddElement(collection);
                table.AddElement(warpObj);
            }
            return table;
        }
        else if(tableType == TableType.Nkv)
        {
            var table = new DataObject(null);
            foreach (var row in rowList)
            {
                var id = row.name;
                var secondRjValue = row.SecondChild as RjValue;
                var valueString = secondRjValue.valueString;
                var valueType = secondRjValue.valueType;
                var v = new RjValue(id, valueString, valueType, "");
                table.AddElement(v);
            }
            return table;
        }
        else
        {
            var table = new DataObject(null);
            foreach (var row in rowList)
            {
                table.AddElement(row);
            }
            return table;
        }
    }

    public static RjCollection GetDictionaryCollectionSafy(Dictionary<string, RjCollection> dic, string key)
    {
        if(dic.ContainsKey(key))
        {
            return dic[key];
        }
        else
        {
            dic[key] = new RjCollection(key, "");
            return dic[key];
        }
    }

}
    
  