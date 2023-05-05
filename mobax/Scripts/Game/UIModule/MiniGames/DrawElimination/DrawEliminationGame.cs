
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public partial class DrawEliminationGame : Page
{
    private const int CustomerNum = 2;
    private const int DemandsNum = 3; // 顾客需求数
    private const float LeaveTime = .2f;
    private const float ComeTime = .2f;

    public DrawEliminationCustomer[] customers;

    private int _abandonNum; // 遗弃数量
    private int _displayAbandonNum;
    private int _score;
    private int _displayScore;
    private int _startTime; // 游戏开始的时候记录的时间
    private int _gameTime;  // 整局游戏消耗的时间
    private bool _resultDisplayed;
    private bool _gameOver;

    private void Awake()
    {
        for (var i = 0; i < CustomerNum; ++i)
        {
            customers[i].ItemProvider = new string[DemandsNum];
            customers[i].OnLeave = _OnCustomerLeave;
        }

        GameZone.OnEliminate = _OnEliminate;
        DrawEliminationEffect.EffectNode = Node_effects;
    }

    private void Start()
    {
        _NewRound();
    }

    private void OnEnable()
    {
        DrawEliminationTalk.Bind(Txt_talk);
    }

    private void Update()
    {
        if (_gameOver) return;
        
        var total = DrawEliminationData.GameTime * 1000;
        var passedTime = _GetTime() - _startTime;
        if (passedTime >= total)
        {
            _HandleGameOver();
            _HandleResult();
        }
    }

    private void _NewRound()
    {
        // 新一局数据重置
        _gameOver = false;
        _resultDisplayed = false;
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
        Txt_countDown.SetEnd(DrawEliminationData.GameTime);
        // 展示欢迎语
        DrawEliminationTalk.Goes(DrawEliminationTalkType.T1_Welcome);
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
            var matchFlag = DrawEliminationHelper.Match(provider, linkItems);

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

            DrawEliminationTalk.Goes(DrawEliminationTalkType.T3_UnMatched);
        }
        else
        {
            DrawEliminationTalk.Goes(DrawEliminationTalkType.T2_Matched);
        }

        if (abandonNum > 0)
        {
            _abandonNum += abandonNum;
            if (_abandonNum >= DrawEliminationData.AbandonMax)
            {
                _HandleGameOver();
            }
        }
        
        // 播放音乐
        WwiseEventManager.SendEvent(TransformTable.Custom, "SgameItemClear");
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
        var score = DrawEliminationHelper.GetCustomerScore(costTime);
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
                DrawEliminationEffect.PlayEffect("fx_ui_Sgame_clear", fromPos);
                DrawEliminationEffect.FlyStraightEffect("fx_ui_Sgame_trail", fromPos, successPos, () =>
                {
                    // 就第一次飞一下
                    if (!flew)
                    {
                        flew = true;
                        _RefreshScore();
                        WwiseEventManager.SendEvent(TransformTable.Custom, "SgameGetPoint");
                    }
                });
            }
            else
            {
                DrawEliminationEffect.PlayEffect("fx_ui_Sgame_clear_2", fromPos);
                DrawEliminationEffect.FlyStraightEffect("fx_ui_Sgame_trail_2", fromPos, missPos, () =>
                {
                    _displayAbandonNum += 1;
                    _RefreshAbandon();
                });
            }
        }

        if (_score >= DrawEliminationData.WinScore)
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
            DrawEliminationEffect.PlayEffect("fx_ui_Sgame_clear_2", fromPos);
            DrawEliminationEffect.FlyStraightEffect("fx_ui_Sgame_trail_2", fromPos, missPos, () =>
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
        // WwiseEventManager.SendEvent(WwiseGameEvent.Custom, "SgameGetPoint");
        DrawEliminationProcesser.Finish(_score, _gameTime, _abandonNum);
    }

    private void _OnCustomerLeave(DrawEliminationCustomer customer)
    {
        // 还要扣分（配的负数）
        _score = Mathf.Max(0, _score + DrawEliminationHelper.GetCustomerScore(-1));
        _RefreshScore();
        // 离开的时候也要说话
        DrawEliminationTalk.Goes(DrawEliminationTalkType.T4_CustomerLeave);
        _PlayChangeCustom(customer);
    }

    private async void _PlayChangeCustom(DrawEliminationCustomer customer)
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
        var abandonMax = DrawEliminationData.AbandonMax;
        Txt_abandon.text = $"<color=#FF6E6E>{_displayAbandonNum}</color><size=48>/{abandonMax}</size>";
        Progress_abandon.SetProgress((float)_displayAbandonNum / abandonMax, ease);

        if (_abandonNum >= abandonMax && _displayAbandonNum == _abandonNum)
        {
            _HandleResult();
        }
    }

    private void _RefreshScore(bool ease = true)
    {
        var winScore = DrawEliminationData.WinScore;
        var to = Mathf.Min(winScore, _score);
        Progress_aim.SetProgress((float)_score / winScore, ease);
        if (ease)
        {
            var from = _displayScore;
            DrawEliminationHelper.TweenTo(from, to, 1f, _SetScoreView);
        }
        else
        {
            _SetScoreView(to);
        }

        _displayScore = _score;
        if (_score >= DrawEliminationData.WinScore)
        {
            _HandleResult();
        }
    }

    private void _SetScoreView(int score)
    {
        var winScore = DrawEliminationData.WinScore;
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
}