using System.Collections;
using System.Collections.Generic;
using System;

using System.IO;
using System.Text;
public class RjCollection:RjElement
{
	string modifier;
	public RjCollection(string _key, string modifier)
	{
		this.modifier = modifier;
		classType=RjClassType.COLL;
		m_key=_key;
		if(!string.IsNullOrEmpty(m_key))
		{
			string _kk=m_key;
			if(_kk.Contains("<")&&_kk.Contains(">"))
			{
				int _b=_kk.IndexOf("<");
				int _e=_kk.IndexOf(">");
				m_type=_kk.Substring(_b+1,_e-_b-1);
				m_key=m_key.Substring(_e+1);
			}
		}
//		Debug.Log(m_type+" "+m_key);
		m_list=new List<RjElement>();
	}
	public override void AddElement(RjElement _element)
	{
		m_list.Add(_element);
		_element.SetParent(this);
		if(m_type==null)
		{
			if(_element is DataObject)
			{
				m_type=_element.GetTypeStr();
			}
		}
	}
	public override string ToCSharpCode()
	{


		if(m_type!=null)
		{

			if(!DmHelper.DataNameBankList.Contains(m_type))
			{
				DmHelper.DataNameBankList.Add(m_type);
				DmHelper.DataBankList.Add(m_list[0] as DataObject);
			}
		}
		m_des="[";
		for(int i=0;i<m_list.Count;++i)
		{
			m_des+=m_list[i].GetDes();
			if(i<m_list.Count-1)
			{
				m_des+=",";
			}
		}
		m_des+="]";

		if(string.IsNullOrEmpty(m_type))
		{
			return "\tpublic List<"+m_list[0].GetTypeStr()+"> "+m_key+";  //"+m_des;
		}
		else
		{
			return "\tpublic List<"+m_type+"> "+m_key+";  //"+m_des;
		}
	}

	// 动态数组指数组元素个数动态的数组
	// 没有内容的子元素会被剔除掉
	public bool IsDynamic
	{
		get
		{
			modifier = modifier.Trim();
			var parts = modifier.Split(' ');
			var isDynamic = Array.IndexOf(parts, "dynamic") != -1;
			return isDynamic;
		}
	}

	// 静态数组指数组元素个数固定的数组
	// 没有内容的子元素会产生一个默认值
	public bool IsStatic
	{
		get
		{
			return !IsDynamic;
		}
	}

	public override string ToJson()
	{
		string _v="";
		for(int i=0;i<m_list.Count;++i)
		{
			var subElement = m_list[i];

			// if is dynamic array
			// callup empty sub element
			if(this.IsDynamic)
			{
				if(subElement is RjValue)
				{
					var subValue = subElement as RjValue;
					if(subValue.IsValueStringEmpty)
					{
						continue;
					}
				}
				if(subElement is DataObject)
				{
					var obj = subElement as DataObject;
					if(obj.IsIdNull)
					{
						continue;
					}
					
				}
			}
			if(_v != "")
			{
				_v += ",";
			}
			_v += subElement.ToJson();

		}
        if(string.IsNullOrEmpty(m_key))
        {
            return $"["+_v+"]";
        }
        else
        {
            return $"\"{m_key}\":[{_v}]";
        }
		
	}
	public override string GetDes ()
	{
		return m_des;
	}
	public override string GetTypeStr ()
	{
		return "List<"+m_list[0].GetTypeStr()+">";
	}
	public string m_key;
	string m_des="";
	string m_type=null;
	List<RjElement> m_list;
}
