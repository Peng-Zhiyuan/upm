using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class LoggerFloating : Floating
{
    public Text messageText;

    public Transform content;

    public GameObject itemPrefab;

    public Button back;

    Queue<LoggerItem> items = new Queue<LoggerItem>();

    LoggerItem selectedItem;

    public void Awake()
    {
        back.onClick.AddListener(() => {
            UIEngine.Stuff.RemoveFloating<LoggerFloating>();
        });
        itemPrefab.SetActive(false);
    }

    public async void OnEnable()
    {
        LoggerManager.Stuff.onRefresh += OnNew;
        InitOnEnable();

        //await TimeWaiterManager.Stuff.WaitFrameAsync(1);
        await Task.Delay(1);
        var trans = (content as RectTransform);
        trans.anchoredPosition = new Vector2(0, trans.sizeDelta.y);
    }

    public void OnDisable()
    {
        LoggerManager.Stuff.onRefresh -= OnNew;
    }

    void InitOnEnable(){
        while(items.Count > 0){
            var delete = items.Dequeue();
            GameObject.DestroyImmediate(delete.gameObject);
        }
        foreach(var a in LoggerManager.Stuff.logs){
            AddNew(a);
        }
    }

    void OnNew(GameUnityLog log){
        AddNew(log);
        ClearOverflow();
    }

    void AddNew(GameUnityLog log){
        var newGo = GameObject.Instantiate(itemPrefab);
        newGo.SetActive(true);
        var comp = newGo.GetComponent<LoggerItem>();
        newGo.transform.SetParent(content, false);
        comp.logInfo = log;
        comp.text.text = $"[{log.time.ToString("T")}] {log.condition}";
        comp.text.color = GetLogColor(log.type);
        items.Enqueue(comp);
    }

    void ClearOverflow(){
        while(items.Count > LoggerManager.Stuff.MaxCount){
            var delete = items.Dequeue();
            GameObject.DestroyImmediate(delete.gameObject);
        }
    }

    public void SetClickLogInfo(LoggerItem log){
        if(selectedItem == log){
            return;
        }
        if(selectedItem != null){
            selectedItem.DeSelect();
        }
        selectedItem = log;
        if(selectedItem != null){
            messageText.text = $"{selectedItem.logInfo.condition}\n{selectedItem.logInfo.stack}";
            messageText.color = GetLogColor(selectedItem.logInfo.type);
        }
        else{
            messageText.text = "";
        }
    }

    public static Color GetLogColor(LogType logType){
        if(logType == LogType.Assert || logType == LogType.Error || logType == LogType.Exception){
            return Color.red;
        }
        else if(logType == LogType.Warning){
            return Color.yellow;
        }
        else{
            return Color.white;
        }
    }

}
