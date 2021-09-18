using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUnityConsoleItem : MonoBehaviour
{
    public Text text;

    public Button btn;

    public GameUnityLog logInfo;

    public void Awake()
    {
        btn.onClick.AddListener(Select);
        DeSelect();
    }

    public void Select(){
        var floating = UIEngine.Stuff.FindFloating<GameUnityConsoleFloating>();
        btn.image.color = new Color(1, 1, 1, 0.2f);
        floating.SetClickLogInfo(this);
    }

    public void DeSelect(){
        btn.image.color = new Color(1, 1, 1, 0.1f);
    }
}
