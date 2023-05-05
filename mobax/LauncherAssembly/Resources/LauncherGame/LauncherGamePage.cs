
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public partial class LauncherGamePage : MonoBehaviour
{
    public Transform EffectClear1Prefab;
    public Transform EffectClear2Prefab;
    public Transform EffectTrail1Prefab;
    public Transform EffectTrail2Prefab;
    public Transform EffectLinePrefab;
    public LauncherGameCustomer[] customers;
    
    private const int CustomerNum = 2;
    private const int DemandsNum = 3; // 顾客需求数
    private const float LeaveTime = .2f;
    private const float ComeTime = .2f;

    private int _abandonNum; // 遗弃数量
    private int _displayAbandonNum;
    private int _score;
    private int _displayScore;
    private int _startTime; // 游戏开始的时候记录的时间
    private int _gameTime;  // 整局游戏消耗的时间
    private bool _resultDisplayed;
    private bool _played; // 是否玩过游戏（如果没玩过，下载完了就直接进去）
    private bool _gameOver;
    private bool _skipable;

    public void OnButton(string msg)
    {
        switch (msg)
        {
            case "skip":
                _DoSkip();
                break;
        }
    }
    
    public void ReStart()
    {
        _NewRound();
    }
    
    private void Awake()
    {
        for (var i = 0; i < CustomerNum; ++i)
        {
            customers[i].ItemProvider = new string[DemandsNum];
            customers[i].OnLeave = _OnCustomerLeave;
        }

        GameZone.OnEliminate = _OnEliminate;
        LauncherGameEffect.EffectNode = Node_effects;
        LauncherGameEffect.Register("fx_ui_Sgame_clear", EffectClear1Prefab);
        LauncherGameEffect.Register("fx_ui_Sgame_clear_2", EffectClear2Prefab);
        LauncherGameEffect.Register("fx_ui_Sgame_trail", EffectTrail1Prefab);
        LauncherGameEffect.Register("fx_ui_Sgame_trail_2", EffectTrail2Prefab);
        LauncherGameEffect.Register("fx_ui_Sgame_line", EffectLinePrefab);
    }

    private void Start()
    {
        _NewRound();
    }

    private void OnEnable()
    {
        LauncherGameTalk.Bind(Txt_talk);
    }

    private void OnDisable()
    {
        LauncherGameProcesser.ClearPops();
    }

    private void Update()
    {
        Progress_text.text = Progress.DisplayValue.ToString("P0");

        if (!_skipable)
        {
            if (Progress.DisplayValue >= 1)
            {
                _skipable = true;
                Button_skip.gameObject.SetActive(true);

                // 如果玩都没玩， 那就直接进去了
                if (!_played)
                {
                    _DoSkip();
                }
            }
        }
        
        if (_gameOver) return;
        var total = LauncherGameData.GameTime * 1000;
        var passedTime = _GetTime() - _startTime;
        if (passedTime >= total)
        {
            _HandleGameOver();
            _HandleResult();
        }
    }

    public float UIDownloadProgress
    {
        set
        {
            this.Progress.targetValue = value;
        }
    }

    public string UITipText
    {
        set
        {
            this.Tip_loading.text = value;
        }
    }

    private void _NewRound()
    {
        // 新一局数据重置
        _gameOver = false;
        _resultDisplayed = false;
        _played = false;
        _startTime = _GetTime();
        // 因为provider依赖地图数据，所以地图要先初始化
        GameZone.ResetMap();
        for (var i = 0; i < CustomerNum; ++i)
        {
            customers[i].Start();
            customers[i].Come();
        }
        
        _ResetTrash();
        _ResetScore();
        // 展示倒计时
        Txt_countDown.SetEnd(LauncherGameData.GameTime);
        // 展示欢迎语
        LauncherGameTalk.Goes(LauncherGameTalkType.T1_Welcome);
    }

    private void _OnEliminate(List<int> links, List<string> linkItems)
    {
        var matched = false;
        var abandonNum = 0;
        for (var index = 0; index < CustomerNum; ++index)
        {
            var customer = customers[index];
            if (!customer.Ready) continue;
            
            var provider = customer.ItemProvider;
            var matchFlag = LauncherGameHelper.Match(provider, linkItems);

            if (matchFlag != 0)
            {
                _HandleMatch(index, matchFlag, links);
                _PlayChangeCustom(customer);
                matched = true;
                abandonNum = links.Count - provider.Length;
                break;
            }
        }

        if (!matched)
        {
            abandonNum = links.Count;
            _HandleMiss(links);

            LauncherGameTalk.Goes(LauncherGameTalkType.T3_UnMatched);
        }
        else
        {
            LauncherGameTalk.Goes(LauncherGameTalkType.T2_Matched);
        }

        if (abandonNum > 0)
        {
            _abandonNum += abandonNum;
            if (_abandonNum >= LauncherGameData.AbandonMax)
            {
                _HandleGameOver();
            }
        }
        
        // 标记玩过了
        _played = true;
        // 播放音乐
        Audio_clear.Play();
    }

    /// <summary>
    /// 处理匹配上的效果
    /// </summary>
    /// <param name="index"></param>
    /// <param name="matchFlag"></param>
    /// <param name="links"></param>
    private void _HandleMatch(int index, int matchFlag, List<int> links)
    {
        var customer = customers[index];
        var costTime = customer.PastTime;
        var score = LauncherGameHelper.GetCustomerScore(costTime);
        _score += score;
        Debug.Log($"耗时{costTime}ms， 增加分数{score}");
        
        // 播放飞行特效
        var successPos = Txt_collect.transform.position;
        var missPos = Txt_abandon.transform.position;
        successPos.x -= 50;
        missPos.x -= 20;
        var flew = false;
        for (var i = 0; i < links.Count; i++)
        {
            var flag = 1 << i;
            var spot = links[i]; 
            var itemBg = GameZone.GetItemBg(spot);
            var fromPos = itemBg.transform.position;
            if ((matchFlag & flag) == flag)
            {
                LauncherGameEffect.PlayEffect("fx_ui_Sgame_clear", fromPos);
                LauncherGameEffect.FlyStraightEffect("fx_ui_Sgame_trail", fromPos, successPos, () =>
                {
                    // 就第一次飞一下
                    if (!flew)
                    {
                        flew = true;
                        _RefreshScore();
                        Audio_score.Play();
                    }
                });
            }
            else
            {
                LauncherGameEffect.PlayEffect("fx_ui_Sgame_clear_2", fromPos);
                LauncherGameEffect.FlyStraightEffect("fx_ui_Sgame_trail_2", fromPos, missPos, () =>
                {
                    _displayAbandonNum += 1;
                    _RefreshAbandon();
                });
            }
        }

        if (_score >= LauncherGameData.WinScore)
        {
            _HandleGameOver();
        }
    }

    /// <summary>
    /// 处理都miss的表现效果
    /// </summary>
    /// <param name="links"></param>
    private void _HandleMiss(List<int> links)
    {
        var missPos = Txt_abandon.transform.position;
        missPos.x -= 20;
        foreach (var spot in links)
        {
            var itemBg = GameZone.GetItemBg(spot);
            var fromPos = itemBg.transform.position;
            LauncherGameEffect.PlayEffect("fx_ui_Sgame_clear_2", fromPos);
            LauncherGameEffect.FlyStraightEffect("fx_ui_Sgame_trail_2", fromPos, missPos, () =>
            {
                _displayAbandonNum += 1;
                _RefreshAbandon();
            });
        }
    }

    /// <summary>
    /// 游戏结束操作
    /// </summary>
    private void _HandleGameOver()
    {
        // 防止重复走到这个逻辑
        if (_gameOver) return;
        
        // 标记结束
        _gameOver = true;
        // 这时候就要计算所用时间了
        _gameTime = _GetTime() - _startTime;
        // 倒计时也要停掉
        Txt_countDown.Stop();
        
        foreach (var customer in customers)
        {
            customer.Stop();
        }
        GameZone.GameOver();
    }

    private void _HandleResult()
    {
        if (_resultDisplayed) return;

        _resultDisplayed = true;
        Audio_finish.Play();
        LauncherGameProcesser.Finish(_score, _gameTime, _abandonNum);
    }

    private void _OnCustomerLeave(LauncherGameCustomer customer)
    {
        // 还要扣分（配的负数）
        _score = Mathf.Max(0, _score + LauncherGameHelper.GetCustomerScore(-1));
        _RefreshScore();
        // 离开的时候也要说话
        LauncherGameTalk.Goes(LauncherGameTalkType.T4_CustomerLeave);
        _PlayChangeCustom(customer);
    }

    private async void _PlayChangeCustom(LauncherGameCustomer customer)
    {
        customer.Leave(LeaveTime);
        // 离开时间
        await Task.Delay((int) (LeaveTime * 1000));
        // 中间空白时间
        await Task.Delay((int) (1 - LeaveTime - ComeTime) * 1000);
        // 再次来人
        customer.Come(ComeTime);
    }

    private void _RefreshAbandon(bool ease = true)
    {
        var abandonMax = LauncherGameData.AbandonMax;
        Txt_abandon.text = $"<color=#FF6E6E>{_displayAbandonNum}</color><size=48>/{abandonMax}</size>";
        Progress_abandon.SetProgress((float)_displayAbandonNum / abandonMax, ease);

        if (_abandonNum >= abandonMax && _displayAbandonNum == _abandonNum)
        {
            _HandleResult();
        }
    }

    private void _RefreshScore(bool ease = true)
    {
        var winScore = LauncherGameData.WinScore;
        var to = Mathf.Min(winScore, _score);
        Progress_aim.SetProgress((float)_score / winScore, ease);
        if (ease)
        {
            var from = _displayScore;
            LauncherGameHelper.TweenTo(from, to, 1f, _SetScoreView);
        }
        else
        {
            _SetScoreView(to);
        }

        _displayScore = _score;
        if (_score >= LauncherGameData.WinScore)
        {
            _HandleResult();
        }
    }

    private void _SetScoreView(int score)
    {
        var winScore = LauncherGameData.WinScore;
        Txt_collect.text = $"<color=#1EB9A2>{score}</color><size=48>/{winScore}</size>";
    }

    private void _ResetTrash()
    {
        _abandonNum = 0;
        _displayAbandonNum = 0;
        _RefreshAbandon(false);
    }

    private void _ResetScore()
    {
        _score = 0;
        _RefreshScore(false);
    }

    private int _GetTime()
    {
        return (int) (Time.realtimeSinceStartup * 1000);
    }

    private void _DoSkip()
    {
        if (_skipable)
        {
            LauncherGameProcesser.SetFinished();
        }
    }
}