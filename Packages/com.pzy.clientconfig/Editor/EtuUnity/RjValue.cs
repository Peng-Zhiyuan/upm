using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
public class RjValue:RjElement
{
	public RjValue(string key, string valueString, RjValueType valueType, string des)
	{
		this.classType = RjClassType.VALUE;
		this.key = key;
		this.valueString = valueString;
		this.valueType = valueType;
		this.des = des;
	}
	public override void AddElement (RjElement _element)
	{
		throw new System.NotImplementedException ();
	}
	public override string ToCSharpCode()
	{
		var isIgnore = this.key.StartsWith("$");
		if(isIgnore)
        {
			Debug.Log("ignore");
			return "";
        }

		string _class="\tpublic ";
		switch(valueType)
		{
		case RjValueType.STRING:
			_class+="string";
			break;
		case RjValueType.INT:
			_class+="int";
			break;
		case RjValueType.FLOAT:
			_class+="float";
			break;
		case RjValueType.BOOL:
			_class += "bool";
			break;
		}

		string _final_des=des;
		if(!string.IsNullOrEmpty(_final_des))
		{
			if(_final_des.Contains("<")&&_final_des.Contains(">"))
			{
//				int _b=_final_des.IndexOf("<");
				int _e=_final_des.IndexOf(">");
				_final_des=_final_des.Substring(_e+1);
			}
		}
		string _info=_class+" "+key+";  //"+_final_des;
//		Debug.Log(_info);
		return _info;
	}


 		private string EncodeString (string str)
        {

			var sb = new StringBuilder();
			
            int n = str.Length;
            for (int i = 0; i < n; i++) {
                switch (str[i]) {
                case '\n':
                    sb.Append ("\\n");
                    continue;

                case '\r':
                    sb.Append ("\\r");
                    continue;

                case '\t':
                    sb.Append ("\\t");
                    continue;

                case '"':
                case '\\':
                    sb.Append ('\\');
                    sb.Append (str[i]);
                    continue;

                case '\f':
                    sb.Append ("\\f");
                    continue;

                case '\b':
                    sb.Append ("\\b");
                    continue;
                }

                if ((int) str[i] >= 32 && (int) str[i] <= 126) {
                    sb.Append (str[i]);
                    continue;
                }

                // Default, turn into a \uXXXX sequence
                // IntToHex ((int) str[i], hex_seq);
                // writer.Write ("\\u");
                // writer.Write (hex_seq);
                sb.Append (str[i]);
            }

            return sb.ToString();
        }

	public bool IsValueStringEmpty
	{
		get
		{
			if(this.valueString == "")
			{
				return true;
			}
			else
			{
				return false;
			}
		}

	}

	public override string ToJson()
	{
		string _v="";
		switch(valueType)
		{
		case RjValueType.STRING:

			// pzy:
			// encode all text
			//_v="\""+ this.valueString.ToString().Replace("\\\"","\"").Replace("\"","\\\"")+"\"";
			
			_v="\""+ EncodeString(valueString)+"\"";

			break;
		case RjValueType.INT:
			int _out=0;
			if(int.TryParse(this.valueString.ToString(),out _out))
			{
				_v= _out.ToString();
			}
			else
			{
				_v="0";
			}
			break;
		case RjValueType.FLOAT:
			_v= this.valueString.ToString();
			if(_v == "")
			{
				_v = "0";
			}
			break;
		case RjValueType.BOOL:
			if(this.valueString != "")
			{
				bool value;
				var b = bool.TryParse(this.valueString, out value);
				if(!b)
				{
					value = false;
				}
				_v = value.ToString().ToLower();
			}
			else
			{
				_v = "false";
			}

			break;
		}
		if(string.IsNullOrEmpty(key))
		{
			return _v;
		}
		else
		{
			return "\""+key+"\":"+_v;
		}
	}
	public override string GetDes ()
	{
		return des;
	}
	public override string GetTypeStr ()
	{
		switch(valueType)
		{
		case RjValueType.STRING:
		default:
			return "string";
		case RjValueType.INT:
			return "int";
		case RjValueType.FLOAT:
			return "float";
		case RjValueType.BOOL:
			return "bool";
		}
	}
	public string key;
	public string valueString;
	string des;
	public RjValueType valueType;
}
