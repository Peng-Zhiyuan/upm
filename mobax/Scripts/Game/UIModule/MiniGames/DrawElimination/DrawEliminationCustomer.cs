using System;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DrawEliminationCustomer : MonoBehaviour
{
    public DrawEliminationIcon ItemPrefab;
    public Transform ItemsNode;
    public CountdownBehaviour CountDown;
    public Image CountDownIcon;
    public DrawEliminationProgressBar ProgressBar;
    [HideInInspector]
    public Action<DrawEliminationCustomer> OnLeave;

    private string[] _itemProvider;
    private DrawEliminationIcon[] _icons;
    private int _comeTime;
    private Color[] _reminderColors;
    private int _waitStage;
    private bool _stopped;

    private void Awake()
    {
        _reminderColors = new[]
        {
            new Color((float)67 / 255, (float)213 / 255, (float)84 / 255),
            new Color((float)218 / 255, (float)198 / 255, (float)31 / 255),
            new Color((float)255 / 255, (float)151 / 255, (float)59 / 255),
            new Color((float)255 / 255, (float)110 / 255, (float)110 / 255),
        };
    }

    private void Update()
    {
        if (_stopped) return;
        if (!Ready) return;

        var total = DrawEliminationData.CustomerTime * 1000;
        var passedTime = _GetTime() - _comeTime;
        if (passedTime >= total)
        {
            OnLeave?.Invoke(this);
        }

        var stage = DrawEliminationHelper.GetWaitStage(passedTime);
        if (_waitStage != stage)
        {
            _waitStage = stage;

            var color = _reminderColors[stage];
            CountDown.GetComponent<Text>().DOColor(color, .2f);
            CountDownIcon.DOColor(color, .2f);
            ProgressBar.SetBarColor(color, .2f);
        }

        ProgressBar.SetProgress((float)(total - passedTime) / total, false);
    }

    public string[] ItemProvider
    {
        set
        {
            _itemProvider = value;

            if (_icons == null)
            {
                _icons = new DrawEliminationIcon[_itemProvider.Length];
                
                var scene = ItemPrefab.gameObject.scene;
                var onScene = !string.IsNullOrEmpty(scene.name);

                for (var i = 0; i < _itemProvider.Length; i++)
                {
                    if (i == 0 && onScene)
                    {
                        _icons[i] = ItemPrefab;
                    }
                    else
                    {
                        _icons[i] = Instantiate(ItemPrefab, ItemsNode);
                    }
                }
            }
        }

        get => _itemProvider;
    }
    
    // 是否就绪
    public bool Ready { get; private set; }

    // 已经过去的时间
    public int PastTime => _GetTime() - _comeTime;

    public void Start()
    {
        _stopped = false;
    }
    
    public void Stop()
    {
        _stopped = true;
        CountDown.Stop();
    }

    public void Leave(float duration)
    {
        Ready = false;

        // CountDown.SetText("");
        foreach (var icon in _icons)
        {
            icon.transform.DOScale(Vector3.zero, duration).SetEase(Ease.OutCubic);
        }
    }

    public async void Come(float duration = 0)
    {
        DrawEliminationManager.RandomDemands(_itemProvider);
        for (var index = 0; index < _icons.Length; index++)
        {
            var icon = _icons[index];
            icon.Flag = _itemProvider[index];
            if (duration > 0)
            {
                icon.transform.DOScale(Vector3.one, duration).SetEase(Ease.OutCubic);
            }
            else
            {
                icon.transform.localScale = Vector3.one;
            }
        }

        await Task.Delay((int)(duration * 1000));
        
        if (null == CountDown) return;
        // 倒计时开始走起来
        Ready = true;
        _waitStage = -1;
        _comeTime = _GetTime();

        if (!_stopped)
        {
            CountDown.SetEnd(DrawEliminationData.CustomerTime, @"{0:ss}s");
        }
    }

    private int _GetTime()
    {
        return (int) (Time.realtimeSinceStartup * 1000);
    }
}