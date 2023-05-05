//using LuaInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogInfo
{
    public string str;
    public LogType type;
    public string stackTrace;

   
}
public class LogMessage : MonoBehaviour
{
    public Text m_stack = null;
    public GameObject m_item = null;
    public Transform m_content = null;

    private List<LogInfo> logs = new List<LogInfo>();

    //LuaState lua = null;
    //LuaFunction func = null;

    private string script =
       @"  


            local GameObject = UnityEngine.GameObject
            local go = GameObject.Find('CanvasUI')
            
            go.name = 'iii'                   
        ";
    // Start is called before the first frame update
    void Start()
    {
        Application.logMessageReceived += Log;

        //new LuaResLoader();
        //lua = new LuaState();
        //lua.Start();
        //LuaBinder.Bind(lua);

        //DoString(script);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void Log(string msg, string stackTrace, LogType type)
    {
        //if (type != LogType.Error)
            //return;

        LogInfo info = new LogInfo();
        info.str = msg;
        info.stackTrace = stackTrace;
        info.type = type;
        logs.Add(info);
        UpdateItems();
    }

    private void UpdateItems()
    {
        int count = m_content.childCount;
        for(int i = count + 1; i < logs.Count; i++)
        {
            GameObject go = GameObject.Instantiate(m_item) as GameObject;
            go.GetComponentInChildren<Text>().text = logs[i].str;
            go.transform.SetParent(m_content);
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = Vector3.zero;
            go.name = i.ToString();
            go.GetComponent<Button>().onClick.AddListener(
                 delegate ()
                 {
                     ShowDetail(logs[i].stackTrace);
                     DoString(script);
                 }
             );
        }
    }

    private void ShowDetail(string str)
    {
        m_stack.text = str;
    }

    private void DoString(string str)
    {
        //lua.DoString(str, "LogMessage.cs");
        //LuaEngine.Stuff.DoString(str, "LogMessage.cs");
    }

    //void OnApplicationQuit()
    //{
    //    //lua.Dispose();
    //    //lua = null;
    //    Application.logMessageReceived -= Log;  
    //}


}
