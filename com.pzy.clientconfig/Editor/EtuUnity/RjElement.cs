using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
//ybzuo


public enum RjClassType
{
	VALUE,
	COLL,
	STUCT,
}
public enum RjValueType
{
	STRING,
	INT,
	FLOAT,
	BOOL,
}

public class RjClassInfo
{
	public string key;
	public RjClassType class_type;
	public RjValueType value_type;
	public int begin_index;
	public int end_index;
}


public class RjGlobalInfo
{
	public List<RjClassInfo> key_list=new List<RjClassInfo>();
}
public abstract class RjElement
{
	public abstract string ToJson();
	public abstract string ToCSharpCode();
	public abstract string GetDes();
	public abstract string GetTypeStr();
	public virtual string GetDesEnd()
	{
		return null;
	}
	public abstract void AddElement(RjElement _element);
	public void SetParent(RjElement _element)
	{
		m_parent=_element;
	}
	public RjElement GetPerent()
	{
		return m_parent;
	}
	public RjClassType GetClassType()
	{
		return classType;
	}
	RjElement m_parent;
	protected RjClassType classType;
}

