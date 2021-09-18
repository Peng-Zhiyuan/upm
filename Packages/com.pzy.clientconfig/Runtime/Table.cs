using UnityEngine;
using System.Collections.Generic;
using CustomLitJson;
using System.Collections.ObjectModel;
using System.Collections;
using System;

public class Table<TRow> where TRow : class
{
	private string name;
	private List<string> keyList= new List<string>();
	private Dictionary<string, string> rowJsonDic = new Dictionary<string, string>(); 
	private Dictionary<string, TRow> rowDic = new Dictionary<string, TRow>(); 

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
					var row = GetRowAtIndex(i);
					_rowList.Add(row);
				}
			}
			return _rowList;
		}
	}
	public int Count
	{
		get
		{
			return rowDic.Count;
		}
	}
	public List<string> Keys
	{
		get
		{
			return keyList;
		}
	}
		
	public Table(string name, string tableEtuString)
	{
		this.name = name;
		var rowEtuStringList = tableEtuString.Split(new string[] { "[|]" }, StringSplitOptions.None);

		for (int i =0; i< rowEtuStringList.Length ; i++)
		{
			var rowEtuString = rowEtuStringList[i];
			//string[] parts = rowEtuString.Split('`');
			string[] parts = rowEtuString.Split(new string[] { "[`]" }, StringSplitOptions.None);

			if (parts.Length == 2 && parts[0] != "" &&  parts[1] != "")
			{
				var key = parts[0];
				var rowJson = parts[1];
				rowJsonDic[key] = rowJson;
				rowDic[key] = null;
				keyList.Add(key);
			}
			else
			{
				Debug.Log("parts.Length: " + parts.Length);
				throw new Exception($"Etu table can't init by tableString: {tableEtuString}");
			}
		}
	}

	public TRow GetRowAtIndex(int index)
	{
		var key = keyList[index];
		var row = GetRow(key);
		return row;
	}

	private TRow GetRow(string key)
	{
		if(!rowDic.ContainsKey(key))
		{
			throw new Exception($"[StaticData]table '{name}' not contains key '{key}'");
		}
		if(rowDic[key] != null)
		{
			var row = rowDic[key];
			return row;
		}
		else
		{
			string json = rowJsonDic[key];
			TRow row = JsonMapper.Instance.ToObject<TRow>(json);
			rowDic[key] = row;
			rowJsonDic[key] = null;
			return row;
		}
	}

	public bool ContainsKey(string key)
	{
		return rowJsonDic.ContainsKey(key);
	}
	public bool ContainsKey(int key)
	{
		return rowJsonDic.ContainsKey(key.ToString());
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

	public TRow TryGet(string key, TRow _default = null)
	{
		if(!rowDic.ContainsKey(key))
		{
			return _default;
		}
		return this[key];
	}
		
	public TRow TryGet(int key, TRow _default = null)
	{
		var keyString = key.ToString();
		if(!rowDic.ContainsKey(keyString))
		{
			return _default;
		}
		return this[keyString];
	}

}
