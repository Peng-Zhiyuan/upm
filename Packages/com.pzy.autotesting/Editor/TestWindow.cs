using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

public class TestWindow : EditorWindow
{
	[MenuItem("AutoTesting/Window")]
	public static void OpenWidnow()
	{
		var window = EditorWindow.GetWindow<TestWindow>("AutoTesting", true);
		window.Show(true);
	}


	List<Type> _testCaseTypeList;
	public List<Type> TestCaseTypeList
    {
		get
        {
			if(_testCaseTypeList == null)
            {
				var assembly = this.GetType().Assembly;
				var list = ReflectionUtil.GetSubClasses<TestCase>(assembly);
				_testCaseTypeList = list;
            }
			return _testCaseTypeList;
        }
    }

    void OnGUI()
    {
		if(TestManager.isTesting)
        {
			GUI.enabled = false;
        }
		

		var list = this.TestCaseTypeList;
		foreach(var type in list)
        {
			var isChecked = this.IsChecked(type);
			var name = type.Name;
			var newIsChecked = GUILayout.Toggle(isChecked, name);
			if(newIsChecked != isChecked)
            {
				this.SetChecked(type, newIsChecked);
            }
		}

		GUILayout.BeginHorizontal();
        {
			var isAll = GUILayout.Button("All");
			if(isAll)
            {
				this.CheckAll();
			}
			var isNone = GUILayout.Button("None");
			if (isNone)
			{
				this.CheckNone();
			}
		}
		
		GUILayout.EndHorizontal();

        {
			var selectedTypeList = this.CheckedTypeList;
			var count = selectedTypeList.Count;
			var msg = "";
			if(count == 0)
            {
				msg = "Select Test Case";
				GUI.enabled = false;
            }
			else
            {
				msg = $"Run {count} Test Case";
			}
			var b = GUILayout.Button(msg);
			if (b)
			{
				TestManager.CreateThenRunAllTest(selectedTypeList);

			}
			GUI.enabled = true;
		}


		GUI.enabled = true;
	}

	void CheckAll()
    {
		var typeList = this.TestCaseTypeList;
		foreach(var type in typeList)
        {
			this.SetChecked(type, true);
        }
	}

	void CheckNone()
    {
		var typeList = this.TestCaseTypeList;
		foreach (var type in typeList)
		{
			this.SetChecked(type, false);
		}
	}


	Dictionary<Type, bool> typeToCheckedDic = new Dictionary<Type, bool>();
	public void SetChecked(Type type, bool b)
    {
		if(b)
        {
			typeToCheckedDic[type] = true;
        }
		else
        {
			typeToCheckedDic.Remove(type);
        }
    }

	public bool IsChecked(Type type)
    {
		var exsits = typeToCheckedDic.ContainsKey(type);
		if(exsits)
        {
			var b = typeToCheckedDic[type];
			return b;
        }
		return false;
    }

	public List<Type> CheckedTypeList
    {
		get
        {
			var list = new List<Type>();
			foreach(var kv in typeToCheckedDic)
            {
				var type = kv.Key;
				var b = kv.Value;
				if(b)
                {
					list.Add(type);
                }
            }
			return list;
        }
    }
}
