using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class GameUnityConsoleFloating : Floating
{
    public Text messageText;

    public Transform content;

    public GameObject itemPrefab;

    public Button back;

    Queue<GameUnityConsoleItem> items = new Queue<GameUnityConsoleItem>();

    GameUnityConsoleItem selectedItem;

    public void Awake()
    {
        back.onClick.AddListener(() => {
            UIEngine.Stuff.RemoveFloating<GameUnityConsoleFloating>();
        });
        itemPrefab.SetActive(false);
    }

    public async void OnEnable()
    {
        ConsoleManager.Stuff.onRefresh += OnNew;
        InitOnEnable();

        //await TimeWaiterManager.Stuff.WaitFrameAsync(1);
        await Task.Delay(1);
        var trans = (content as RectTransform);
        trans.anchoredPosition = new Vector2(0, trans.sizeDelta.y);
    }

    public void OnDisable()
    {
        ConsoleManager.Stuff.onRefresh -= OnNew;
    }

    void InitOnEnable(){
        while(items.Count > 0){
            var delete = items.Dequeue();
            GameObject.DestroyImmediate(delete.gameObject);
        }
        foreach(var a in ConsoleManager.Stuff.logs){
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
        var comp = newGo.GetComponent<GameUnityConsoleItem>();
        newGo.transform.SetParent(content, false);
        comp.logInfo = log;
        comp.text.text = $"[{log.time.ToString("T")}] {log.condition}";
        comp.text.color = GetLogColor(log.type);
        items.Enqueue(comp);
    }

    void ClearOverflow(){
        while(items.Count > ConsoleManager.Stuff.MaxCount){
            var delete = items.Dequeue();
            GameObject.DestroyImmediate(delete.gameObject);
        }
    }

    public void SetClickLogInfo(GameUnityConsoleItem log){
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
