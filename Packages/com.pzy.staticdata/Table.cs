using UnityEngine;
using System.Collections.Generic;
using CustomLitJson;
using System.Collections.ObjectModel;
using System.Collections;
using System;

public class Table<TRow> where TRow : class
{
	private string name;
	private List<string> idList = new List<string>();
	private Dictionary<string, string> idToRowJsonDic = new Dictionary<string, string>(); 
	private Dictionary<string, TRow> idToRowDic = new Dictionary<string, TRow>(); 

	private List<TRow> _rowList;
	public List<TRow> RowList
	{
		get
		{
			if(_rowList == null)
			{
				_rowList = new List<TRow>();
				for(int i = 0; i < Count; i++)
				{
					var row = GetValueByIndex(i);
					_rowList.Add(row);
				}
			}
			return _rowList;
		}
	}

	public TRow[] GetArray()
	{
		return RowList.ToArray();
	}
	public int Count
	{
		get
		{
			return idToRowDic.Count;
		}
	}

	public List<string> Keys
	{
		get
		{
			return idList;
		}
	}
    public List<string> GetKeys ()
    {
        return Keys;
    }

    public Table(string name, string tableEtuString)
	{
		this.name = name;
		var rowEtuStringList = tableEtuString.Split('|');
		for(int i =0; i< rowEtuStringList.Length ; i++)
		{
			var rowEtuString = rowEtuStringList[i];
			string[] parts = rowEtuString.Split('`');

			if(parts.Length == 2 && parts[0] != "" &&  parts[1] != "")
			{
				var key = parts[0];
				var rowJson = parts[1];
				idToRowJsonDic[key] = rowJson;
				idToRowDic[key] = null;
				idList.Add(key);
			}
			else
			{
				throw new Exception($"Etu table can't init by tableString: {tableEtuString}");
			}
		}
	}
    public void Init(Dictionary<string, string> rowJsonDic)
    {
        this.idToRowJsonDic = rowJsonDic;
        idToRowDic.Clear ();
        idList.Clear ();
        _rowList = null;
        foreach (var kv in rowJsonDic) {
            idToRowDic.Add (kv.Key, null);
            idList.Add (kv.Key);
        }
    }

    public TRow GetValueByIndex(int index)
	{
		var key = idList[index];
		var row = GetRow(key);
		return row;
	}

	private TRow GetRow(string id)
	{
		if(!idToRowDic.ContainsKey(id))
		{
			throw new Exception($"[StaticData] table '{name}' not contains id '{id}'");
		}
		if(idToRowDic[id] != null)
		{
			var row = idToRowDic[id];
			return row;
		}
		else
		{
			string json = idToRowJsonDic[id];
			TRow row = JsonMapper.Instance.ToObject<TRow>(json);
			idToRowDic[id] = row;
			idToRowJsonDic[id] = null;
			return row;
		}
	}

	public bool ContainsKey(string key)
	{
		return idToRowJsonDic.ContainsKey(key);
	}
	public bool ContainsKey(int key)
	{
		return idToRowJsonDic.ContainsKey(key.ToString());
	}

	public TRow this[int key] 
	{
		get 
		{
			var keyString = key.ToString();
			return GetRow(keyString); 
		}
	}

	

	public TRow this[string key] 
	{
		get 
		{
			return GetRow(key); 
		}
	}

	public object GetValue(string keyString)
	{
		return (object)GetRow(keyString);
	}

	public TRow TryGetValue(string key, TRow _default = null)
	{
		if(!idToRowDic.ContainsKey(key))
		{
			return _default;
		}
		return this[key];
	}
		
	public TRow TryGetValue(int key, TRow _default = null)
	{
		var keyString = key.ToString();
		return TryGetValue(keyString, _default);
	}

}
