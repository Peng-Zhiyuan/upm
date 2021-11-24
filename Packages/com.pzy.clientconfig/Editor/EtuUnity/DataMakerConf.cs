using System.Collections.Generic;
namespace EtuUnity
{
	public class DataMakerConf
	{
		private static DataMakerConf _instance;
		public static DataMakerConf Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new DataMakerConf();
				}
				return _instance;
			}
		}

		//public  string m_code_out_dir = "Assets/[Subsystems]/-DataMaker/Out";
		public string m_client_dir
		{
			get
			{
				return "";
				//return PlayerPrefs.GetString(m_out_key, "");
			}
			set
			{
				//         string path = value.Replace("\\", "/");
				//         if (path[path.Length - 1] != '/')
				//         {
				//             path += "/";
				//         }
				//PlayerPrefs.SetString(m_out_key, path);
				//         PlayerPrefs.Save();
			}
		}
		public string m_server_dir
		{
			get
			{
				return "";
				//return PlayerPrefs.GetString(m_server_key, "");
			}
			set
			{
				//         string path = value.Replace("\\", "/");
				//         if (path[path.Length - 1] != '/')
				//         {
				//             path += "/";
				//         }
				//PlayerPrefs.SetString(m_server_key, path);
				//         PlayerPrefs.Save();
			}
		}

		public string m_excel_dir
		{
			get
			{
				return "";
				//return PlayerPrefs.GetString(m_excel_key, "");
			}
			set
			{
				//string path = value.Replace("\\", "/");
				//if (path[path.Length - 1] != '/')
				//{
				//    path += "/";
				//}
				//PlayerPrefs.SetString(m_excel_key, path);
				//PlayerPrefs.Save();
			}
		}






		public string m_excel_key
		{
			get
			{
				return "EtuExcel";
			}
		}
		public string m_out_key
		{
			get
			{
				return "EtuOut";
			}
		}

		public string m_server_key
		{
			get
			{
				return "EtuServer";
			}
		}





		//	public string m_server_url_path 
		//	{
		//		get
		//		{
		//			return PlayerPrefs.GetString(m_server_key,"http://192.168.2.250:8081/")+CurServerDocument+"/";
		//		}
		//	}

		public void Reset()
		{
			DataMakerConf.Instance.m_excel_dir = "";
		}


		public enum REMOTE_SERVER
		{
			DEV_CLIENT,
			DEV_SERVER,
		}





	}
}