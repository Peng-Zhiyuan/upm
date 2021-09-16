using System.Collections.Generic;

public class DataObject:RjElement
{
	public DataObject(string _key)
	{

		name=_key;
		if(!string.IsNullOrEmpty(name))
		{
			string _kk=name;
			if(_kk.Contains("<")&&_kk.Contains(">"))
			{
				int _b=_kk.IndexOf("<");
				int _e=_kk.IndexOf(">");
				m_type=_kk.Substring(_b+1,_e-_b-1);
				name=name.Substring(_e+1);
			}
		}
		classType=RjClassType.STUCT;
	}

	public override void AddElement(RjElement _element)
	{
		childList.Add(_element);
		_element.SetParent(this);
	}

    public RjElement FirstChild
    {
        get
        {
            return childList[0];
        }
    }

   public RjElement SecondChild
    {
        get
        {
            return childList[1];
        }
    }

    public bool isRow = false;

	public override string ToCSharpCode()
	{
		string _str="";
		if(!string.IsNullOrEmpty(name) && !isRow)
		{
			_str="\tpublic "+m_type+" "+name;

			if(m_type=="LEESAN_")
			{
				m_type="Dummy"+DmHelper.MagicNum;
				DmHelper.MagicNum++;
				//m_type="LEESAN_";//+DmHelper.DataNameBankList.Count;
				_str="\tpublic "+m_type+" "+name;
				DmHelper.DataNameBankList.Add(m_type);
				DataObject _rj=new DataObject(null);
				for(int i=0;i<childList.Count;++i)
				{
					_rj.AddElement(childList[i]);
				}
				DmHelper.DataBankList.Add(_rj);
			}
			else
			{
				if(!DmHelper.DataNameBankList.Contains(m_type))
				{
					if(!string.IsNullOrEmpty(m_type))
					{
						DmHelper.DataNameBankList.Add(m_type);
					}
					DataObject _rj=new DataObject(null);
					for(int i=0;i<childList.Count;++i)
					{
						_rj.AddElement(childList[i]);
					}
					DmHelper.DataBankList.Add(_rj);
				}
			}
		}
		else
		{

			for(int i = 0; i < childList.Count; i++)
			{
                var child = childList[i];
				var temp = child.ToCSharpCode();
				if(string.IsNullOrEmpty(temp))
                {
					continue;
                }

				_str+= temp;
				if(i < childList.Count-1)
				{
					_str+=";\n";
				}
				else
				{
					_str+=";";
				}
			}
		}

		if(string.IsNullOrEmpty(m_des))
		{
			return _str;
		}
		else
		{
			return _str+"  //"+m_des;
		}
	}

	// public override string ToCSharpCode()
	// {
	// 	string _str="";
	// 	if(!string.IsNullOrEmpty(name))
	// 	{
	// 		_str="\tpublic "+m_type+" "+name;

	// 		if(m_type=="LEESAN_")
	// 		{
	// 			m_type="Dummy"+DmHelper.MagicNum;
	// 			DmHelper.MagicNum++;
	// 			//m_type="LEESAN_";//+DmHelper.DataNameBankList.Count;
	// 			_str="\tpublic "+m_type+" "+name;
	// 			DmHelper.DataNameBankList.Add(m_type);
	// 			DataObject _rj=new DataObject(null);
	// 			for(int i=0;i<childList.Count;++i)
	// 			{
	// 				_rj.AddElement(childList[i]);
	// 			}
	// 			DmHelper.DataBankList.Add(_rj);
	// 		}
	// 		else
	// 		{
	// 			if(!DmHelper.DataNameBankList.Contains(m_type))
	// 			{
	// 				if(!string.IsNullOrEmpty(m_type))
	// 				{
	// 					DmHelper.DataNameBankList.Add(m_type);
	// 				}
	// 				DataObject _rj=new DataObject(null);
	// 				for(int i=0;i<childList.Count;++i)
	// 				{
	// 					_rj.AddElement(childList[i]);
	// 				}
	// 				DmHelper.DataBankList.Add(_rj);
	// 			}
	// 		}
	// 	}
	// 	else
	// 	{

	// 		for(int i = 0; i < childList.Count; i++)
	// 		{
    //             var child = childList[i];
	// 			var temp = child.ToCSharpCode();
	// 			_str+= temp;
	// 			if(i < childList.Count-1)
	// 			{
	// 				_str+=";\n";
	// 			}
	// 			else
	// 			{
	// 				_str+=";";
	// 			}
	// 		}
	// 	}

	// 	if(string.IsNullOrEmpty(m_des))
	// 	{
	// 		return _str;
	// 	}
	// 	else
	// 	{
	// 		return _str+"  //"+m_des;
	// 	}
	// }
	public override string ToJson()
	{
		string _str="";
		if(!string.IsNullOrEmpty(name))
		{
			_str+=("\""+name+"\":");
		}
		_str+="{";
		for(int i=0;i<childList.Count;++i)
		{
			_str+=childList[i].ToJson();
			if(i<childList.Count-1)
			{
				_str+=",";
			}
		}
		return _str+"}";
	}
	public  string ToKeyValue()
	{
		string _str="";
//		if(!string.IsNullOrEmpty(m_key))
//		{
//			_str+=("\""+m_key+"\":");
//		}
//		_str+="{";
		for(int i=0;i<childList.Count;++i)
		{
			string json = childList[i].ToJson();
			int index= json.IndexOf(':')+1;
			_str+=json.Substring(index,json.Length-index).Replace("\"","");
			if(i==0)
			{
				_str+="=";
			}
			if(i>=1)break;
		}
		return _str;
	}

	public  string ToKeyValueString()
	{
		string _str="";
		//		if(!string.IsNullOrEmpty(m_key))
		//		{
		//			_str+=("\""+m_key+"\":");
		//		}
		//		_str+="{";
		for(int i=0;i<childList.Count;++i)
		{
			string json = childList[i].ToJson();
			int index= json.IndexOf(':')+1;
			_str+=json.Substring(index,json.Length-index);
			if(i==0)
			{
				_str+=":";
			}
			if(i>=1)break;
		}
		return _str;
	}

	public bool IsIdNull
	{
		get
		{
			foreach(var child in childList)
			{
				var filed = child as RjValue;
				if(filed != null)
				{
					if(filed.key == "id" || filed.key == "key" || filed.key == "k")
					{
						if(filed.valueString == "")
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}

	public override string GetDes ()
	{
		return m_des;
	}
	public override string GetTypeStr ()
	{
		if(m_type=="LEESAN_")
		{
			m_type="Dummy"+DmHelper.MagicNum;
			DmHelper.MagicNum++;
		}
		return m_type;
	}
	public override string GetDesEnd()
	{
		if(childList.Count>0)
		{
			if(!string.IsNullOrEmpty(childList[0].GetDes()))
			{
				string _dd=childList[0].GetDes();
				if(_dd.Contains("<")&&_dd.Contains(">"))
				{
					int _b=_dd.IndexOf("<");
					int _e=_dd.IndexOf(">");
					return _dd.Substring(_b+1,_e-_b-1);
				}
			}
		}
		return null;
	}
	public string name;
	string m_des="";
	string m_type="LEESAN_";
	public List<RjElement> childList=new List<RjElement>();
}
