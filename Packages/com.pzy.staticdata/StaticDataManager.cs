using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Text;
public partial class StaticDataManager : StuffObject<StaticDataManager>
{

	Dictionary<string,string> nameToTableEtuStringDic = new Dictionary<string, string>();	

	private Dictionary<string, object> TableDic = new Dictionary<string, object>();


	public bool IsStaticDataLoaded
	{
		get{
			return this.nameToTableEtuStringDic.Count > 0;
		}
	}

	//public ITable GetTableInterface(string name)
	//{
	//	if (!TableDic.ContainsKey(name))
	//	{
	//		var tableName = $"{name}Table";
	//		TableDic[name] = ReflectUtil.Instance.InvokeGetMethod(tableName, this);
	//	}
	//	return TableDic[name] as ITable;
	//}

	//public Table<T> GetTable<T>(string name) where T:class
	//{
	//	//Debug.LogError("gettable:"+name);
	//	var table = this.GetTableInterface(name);
	//	return  table as Table<T>;
	//}


	public string LoadTableString(string name)
	{
		var json = LoadAndRemoveTableStringFromNetworkDic(name);
		return json;
	}


	string LoadAndRemoveTableStringFromNetworkDic(string name)
	{
		if(!nameToTableEtuStringDic.ContainsKey(name))
		{
			throw new Exception($"StaticData in network mod, but table json of '{name}' not found");
		}
		string json = nameToTableEtuStringDic[name];
		nameToTableEtuStringDic.Remove (name);
		return json;
	}

	string BytesToString(byte[] bytes)
    {
		var str = Encoding.UTF8.GetString(bytes);
		return str;
	}

	void ResetByEtuString(string etuString)
    {
		nameToTableEtuStringDic.Clear();
		var tableWithNameStringList = etuString.Split('｜');//全角｜
		foreach (var tableWithNameString in tableWithNameStringList)
		{
			if (tableWithNameString.Length <= 5)
			{
				continue;
			}
			var parts = tableWithNameString.Split('｀');//全角｀
			if (parts.Length >= 2)
			{
				var name = parts[0];
				var etuTableString = parts[1];
				nameToTableEtuStringDic[name] = etuTableString;
			}
		}
	}

	public void Reset(byte[] bytes, StaticDataFileFormat fileFormat)
	{
		if(fileFormat == StaticDataFileFormat.EtuString)
        {
			var etuStirng = BytesToString(bytes);
			this.ResetByEtuString(etuStirng);
		}
		else if(fileFormat == StaticDataFileFormat.ZipedEtuString)
        {
			var etuString = Ionic.Zlib.GZipStream.UncompressString(bytes);
			this.ResetByEtuString(etuString);
		}
	}

}
