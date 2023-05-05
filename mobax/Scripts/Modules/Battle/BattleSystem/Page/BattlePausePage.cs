using System;
using System.Diagnostics.PerformanceData;
using System.Threading.Tasks;
using BattleEngine.Logic;
using Modules.Battle.BattleSystem.Page;
using UnityEngine;
using UnityEngine.UI;

public partial class BattlePausePage : Page
{


    void BackToGame()
    {
        UIEngine.Stuff.Back();
        BattleDataManager.Instance.TimeScale = 1;
        BattleEngine.Logic.BattleManager.Instance.BtnPause(false);
        CameraManager.Instance.UpdateEnable = true;
        
        WwiseEventManager.SendEvent(TransformTable.Custom, "se_resume");
    }

    protected override async Task LogicBackAsync()
    {
        this.BackToGame();
    }
    public override async Task OnDismissAsync()
    {
    }

    public void CloseClick()
    {
        BackToGame();
    }

    public override void OnForwardTo(PageNavigateInfo info)
    {
        WwiseEventManager.SendEvent(TransformTable.Custom, "se_pause");
        BattleEngine.Logic.BattleManager.Instance.BtnPause(true);
        BattleDataManager.Instance.TimeScale = 0;
        CameraManager.Instance.UpdateEnable = false;
        
        ButtonUtil.SetClick(this.CommonCloseButton, () =>
            {
                BackToGame();
            }
        );
        
        ButtonUtil.SetClick(this.Continue, () =>
            {
                BackToGame();
            }
        );
        
        ButtonUtil.SetClick(this.QuitGame, () =>
            {
                QuitGameButton();
            }
        );
        
        ButtonUtil.SetClick(this.BattleSettingBtn, () =>
            {
                UIEngine.Stuff.ForwardOrBackTo("BattleSettingPage");
            }
        );
        
        

        RefreshList();
    }

    int cur_num = 0;
    int def_num = 0;
    int atk_num = 0;
    private int suggest_score = 0;
    private void RefreshList()
    {
        var list = BattleManager.Instance.ActorMgr.GetAtkLst();
        cur_num = 0;
        def_num = 0;
        atk_num = 0;
        foreach (var VARIABLE in list)
        {
            if (!VARIABLE.IsMain)
                continue;
            
            cur_num += VARIABLE.mData.battleItemInfo.battlePlayerRecord.OP_CureValue;
            def_num += VARIABLE.mData.battleItemInfo.battlePlayerRecord.ReceiveDamageValue;
            atk_num += VARIABLE.mData.battleItemInfo.battlePlayerRecord.OP_AttackValue;
        }
        
        foreach (var VARIABLE in list)
        {
            if (!VARIABLE.IsMain)
                continue;
            
            var go = Instantiate(CharacterItem, ListContent.transform);
            SetData(go.transform, VARIABLE);
        }
        
        Instantiate(Line, ListContent.transform);
        
        list = BattleManager.Instance.ActorMgr.GetDefLst();
        cur_num = 0;
        def_num = 0;
        atk_num = 0;
        
        foreach (var VARIABLE in list)
        {
            cur_num += VARIABLE.mData.battleItemInfo.battlePlayerRecord.OP_CureValue;
            def_num += VARIABLE.mData.battleItemInfo.battlePlayerRecord.ReceiveDamageValue;
            atk_num += VARIABLE.mData.battleItemInfo.battlePlayerRecord.OP_AttackValue;
        }
        
        foreach (var VARIABLE in list)
        {
            var go = Instantiate(CharacterItem, ListContent.transform);
            SetData(go.transform, VARIABLE);
        }

        var stage_info = StaticData.StageTable.TryGet(Battle.Instance.CopyId);
        if (stage_info == null)
            return;
        this.ChapterDes.text = LocalizationManager.Stuff.GetText(stage_info.desLv);
        UiUtil.ShowRollText(this.ChapterName, LocalizationManager.Stuff.GetText(stage_info.Name));
        suggest_score = stage_info.suggestCombat;
        
        TimeSpan ts = TimeSpan.FromMilliseconds((int)((BattleTimeManager.Instance.BattleTime - BattleTimeManager.Instance.CurrentBattleTime) * 1000));
        this.ChapterTime.text = ts.ToMillionSecond();
    }

    private void SetData(Transform transform, Creature data)
    {
        if(data == null)
            return;

        var info = StaticData.HeroTable.TryGet(data.ConfigID);
        if(info == null)
            return;
        var name = LocalizationManager.Stuff.GetText(info.Name);
        var text = transform.Find("Name").GetComponent<Text>();
        UiUtil.ShowRollText(text, name);
        //transform.Find("NameBG/Name").GetComponent<Text>().text = LocalizationManager.Stuff.GetText(info.Name);
        transform.Find("Head/Level").GetComponent<Text>().text = string.Format("{0:00}", data.mData.battleItemInfo.lv);
        
        if (data.IsEnemy)
        {
            transform.Find("CombatIcon").SetActive(false);
            transform.Find("Score").SetActive(false);
        }
        else
        {
            if (data.IsMain)
            {
                var hero = HeroManager.Instance.GetHeroInfo(data.mData.ConfigID);
                if(hero != null)
                    transform.Find("Score").GetComponent<Text>().text = hero.Power.ToString(); 
            
                transform.Find("AssistPanelEnemy").gameObject.SetActive(false);
            }
            else
            {
                transform.Find("Score").GetComponent<Text>().text = suggest_score.ToString();
            }
        }
        
        UiUtil.SetSpriteInBackground(transform.Find("Head/HeadIcon").GetComponent<Image>(), () => info.Head + ".png", null, 1, null, true);

        var job = transform.Find("Head/Job").GetComponent<Image>();
        UiUtil.SetSpriteInBackground(job, () => "Icon_occ" + info.Job + ".png", null, 1, null, true);
        
        var Element = transform.Find("Head/Weak").GetComponent<Image>();
        UiUtil.SetSpriteInBackground(Element, () => "element_" + info.Element + ".png", null, 1, null, true);
        
        if(cur_num > 0)
        transform.Find("DataStatistics/CureBar").GetComponent<Image>().fillAmount = data.mData.battleItemInfo.battlePlayerRecord.OP_CureValue * 1f / cur_num;
        if(def_num > 0)
        transform.Find("DataStatistics/DefenceBar").GetComponent<Image>().fillAmount = data.mData.battleItemInfo.battlePlayerRecord.ReceiveDamageValue * 1f / def_num;
        if(atk_num > 0)
        transform.Find("DataStatistics/AttackBar").GetComponent<Image>().fillAmount = data.mData.battleItemInfo.battlePlayerRecord.OP_AttackValue * 1f / atk_num;
        
        transform.Find("DataStatistics/CureNum").GetComponent<Text>().text = data.mData.battleItemInfo.battlePlayerRecord.OP_CureValue.ToString();
        transform.Find("DataStatistics/DefenceNum").GetComponent<Text>().text = data.mData.battleItemInfo.battlePlayerRecord.ReceiveDamageValue.ToString();
        transform.Find("DataStatistics/AttackNum").GetComponent<Text>().text = data.mData.battleItemInfo.battlePlayerRecord.OP_AttackValue.ToString();

        var root = transform.Find("Buff/BuffRoot");
        var BufferBar = new BufferBar(root, "RoleBuffer.prefab");
        BufferBar.SetData(data.mData);

        var click = transform.Find("Click").GetComponent<Button>();
        ButtonUtil.SetClick(click, () =>
            {
                //UIEngine.Stuff.Forward("RoleDetailPage", data);
            }
        );
    }

    public async void QuitGameButton()
    {
        //先清理战斗界面
        var page = UIEngine.Stuff.FindPage("BattlePage");
        if(page != null)
            (page as BattlePage).EndGame();
            
        // 战斗页面进入这个页面前会暂停时间
        // 这里应当将选择回执给战斗页面让战斗页面来退出游戏
        // 然而现在没有很好的机制，加上跨越了 ts 和 c# 虚拟机
        // 暂时直接处理
        if (Battle.Instance.IsDreamEscapeMode)
        {
            if (!await Dialog.AskAsync("", LocalizationManager.Stuff.GetText("M4_memory_msg_exitbattle")))
            {
                UIEngine.Stuff.Back();
                BattleDataManager.Instance.TimeScale = 1;
                BattleEngine.Logic.BattleManager.Instance.BtnPause(false);
                return;
            }
            BattleDataManager.Instance.TimeScale = 1;
            await DreamscapeManager.Exit(false);
            await UIEngine.Stuff.RemoveFromStack<DreamscapePage>();
        }
        else
        {
            BattleDataManager.Instance.TimeScale = 1;
        }
        //UnityEngine.Time.timeScale = 1;
        if(!Battle.Instance.IsArenaMode)
            TrackManager.BattleExit(Battle.Instance.CopyId.ToString());
        else
        {
            ArenaMode mode = Battle.Instance.GameMode as ArenaMode;
            var opponentId = mode.param.targetUid;
            var row = ArenaUtil.GetArenaRowByScore(ArenaManager.Stuff.info.score);
                
            TrackManager.PvpExit(mode.param.targetUid, row.Id.ToString(), "0");
        }
        //await BattlePipline.DestroyBattleInstanceAsync();
        //await UIEngine.Stuff.ForwardOrBackToAsync<StageMainPage>();
        await BattlePipline.GoBack();
            
        //AudioManager.PlayBgmInBackground("Bgm_System_Inside.wav");
    }
}