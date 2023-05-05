using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public partial class LauncherGameResultPop : MonoBehaviour
{
    public LauncherSimpleItem itemPrefab;
    
    private int _version;
    
    public void OnButton(string msg)
    {
        switch (msg)
        {
            case "quit":
            case "confirm":
                LauncherGameProcesser.SetFinished();
                break;
            case "replay":
                LauncherGameProcesser.Replay();
                break;
        }
    }
    
    public void SetInfo(LauncherGameJudgeEnum judge, int costTime)
    {
        Txt_judgement.text = $"{judge}";
        Txt_costTime.text = $"{costTime / 1000}s";

        var rewardAvailable = LauncherLoadingHelper.BuildRewards(itemPrefab, List_rewards.transform);
        Switcher_reward.Selected = rewardAvailable;
    }

    private void Awake()
    {
        // List_items.ViewSetter = _OnRenderItem;
    }

    private async void OnEnable()
    {
        var version = _version;
        
        await Task.Delay(10000);
        if (version == _version)
        {
            LauncherGameProcesser.SetFinished();
        }
    }

    private void OnDisable()
    {
        ++_version;
    }

    private void _OnRenderItem(object data, Transform tf)
    {
    }
}