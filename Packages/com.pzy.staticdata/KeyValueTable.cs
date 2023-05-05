using UnityEngine;
using System.Collections.Generic;
using CustomLitJson;
using System.Collections.ObjectModel;
using System.Collections;
using System;

public class KeyValueTable<TValue>
{
	public string name;
	public Dictionary<string, TValue> dic = new Dictionary<string, TValue>(); 

	public int Count
	{
		get
		{
			return this.dic.Count;
		}
	}

	public KeyValueTable(string name, string tableEtuString)
	{
		this.name = name;
		var rowEtuStringList = tableEtuString.Split('|');
		for(int i = 0; i< rowEtuStringList.Length ; i++)
		{
			var rowEtuString = rowEtuStringList[i];
			string[] parts = rowEtuString.Split('`');

			if(parts.Length == 2 && parts[0] != "")
			{
				var key = parts[0];
				var value = parts[1];
				TValue finalValue = default(TValue);
				if(typeof(TValue) == typeof(int))
				{
					var intValue = int.Parse(value);
					finalValue = (TValue)(object)intValue;
				}
				else if(typeof(TValue) == typeof(float))
				{
					var floatValue = float.Parse(value);
					finalValue = (TValue)(object)floatValue;
				}
				else if(typeof(TValue) == typeof(string))
				{
					finalValue = (TValue)(object)value;
				}
				dic[key] = finalValue;
			}
			else
			{
				//Debug.LogError("parts.Length:"+parts.Length);
				//throw new Exception($"Etu table can't init by tableString: {tableEtuString}");
			}
		}
	}

	public bool ContainsKey(string key)
	{
		return dic.ContainsKey(key);
	}
	public bool ContainsKey(int key)
	{
		return dic.ContainsKey(key.ToString());
	}

	public TValue this[int index] 
	{
		get 
		{
			var key = index.ToString();
			return this.dic[key];
		}
	}

	public TValue this[string key] 
	{
		get 
		{
			return this.dic[key];
		}
	}

	public object GetValue(string keyString)
	{
		return (object)this.dic[keyString];
	}

    public List<string> GetKeys ()
    {
        var result = new List<string> ();
        foreach (var key in dic.Keys) {
            result.Add (key);
        }
        return result;
    }

}
