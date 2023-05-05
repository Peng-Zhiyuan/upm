using System;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public partial class HeroStarPop : Page
{
    public HeroStarCluster StarCluster;
    
    private HeroInfo _heroInfo;
    private bool _isMax;
    private GameObject _smallEffect;
    private GameObject _bigEffect;
    private JLocker _locker;

    public override void OnForwardTo(PageNavigateInfo info)
    {
        if (info.param is HeroInfo heroInfo)
        {
            _heroInfo = heroInfo;

            _RefreshData();
            _InitView();
            _RefreshView();
            _BindReminders();
            StarCluster.SetInfo(_heroInfo);
        }
    }

    public override void OnButton(string msg)
    {
        switch (msg)
        {
            case "up":
                _StarUp();
                break;
            case "cancel":
                UIEngine.Stuff.Back();
                break;
        }
    }

    private async void Awake()
    {
        StarCluster.onSelect = _OnSelected;
        StarCluster.onHighlight = _OnHighlight;
        // 特效效果
        var bucket = BucketManager.Stuff.Main;
        _bigEffect = await bucket.GetOrAquireAsync<GameObject>("fx_ui_herostarpage_1.prefab");
        _smallEffect = await bucket.GetOrAquireAsync<GameObject>("fx_ui_herostarpage_2.prefab");
        // others
        _locker = new JLocker(1000);
    }

    private async void _StarUp()
    {
        if (_isMax) 
        {
            ToastManager.ShowLocalize("M4_max_already");
            return;
        }
        if (!ItemUtil.IsEnough(_heroInfo.StarConfig.soulSubs))
        {
            ToastManager.ShowLocalize("M4_not_enough");
            return;
        }

        if (_locker.IsOn)
        {
            ToastManager.ShowLocalize("common_clickCooldown");
            return;
        }

        _locker.On();
        var prevPower = _heroInfo.Power;
        await HeroApi.StarUpAsync(_heroInfo.HeroId);
        _locker.Off();
        // 上报战力变化
        HeroProxy.HandlePowerChange(_heroInfo, prevPower);
        // 标记需要刷新
        HeroNotifier.Invoke(HeroNotifyEnum.Star);
        // 更新红点逻辑
        HeroReminderProxy.UpdateReminder_StarChanged(_heroInfo);
        // 容错处理
        if (this == null) return;
        // 刷新视图
        _RefreshData();
        _RefreshView();
        StarCluster.HighlightStar(_heroInfo.StarId);
        List_abilities.Unlock(_heroInfo.StarConfig);
        // 播放升星声音
        WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_rewardHeroRisingStar");
    }

    private void _RefreshData()
    {
        _isMax = _heroInfo.NextStarConfig == null;
    }

    private void _InitView()
    {
        List_abilities.SetInfo(_heroInfo);
    }
    
    private void _BindReminders()
    {
        Reminder.Bind($"{HeroReminderConst.Hero_heroStarUpPrefix}{_heroInfo.HeroId}", Button_confirm);
    }

    private void _RefreshView()
    {
        Cost_starUp.gameObject.SetActive(!_isMax);
        if (!_isMax)
        {
            Cost_starUp.SetRequire(_heroInfo.StarConfig.soulSubs.First());
        }
        Label_up.SetLocalizer(_isMax ? "M4_level_max" : "M4_star_up");
    }

    private void _OnSelected(int starId)
    {
        // 永远让选中的居中
        var contentLength = Node_starCluster.content.sizeDelta.y;
        var maskLength = Node_starCluster.GetComponent<RectTransform>().sizeDelta.y;
        var starY = StarCluster.GetStarY(starId);
        var contentY = contentLength - starY - maskLength / 2;
        contentY = Math.Max(0, Math.Min(contentY, contentLength - maskLength));
        Node_starCluster.content.DOAnchorPosY(contentY, .2f);
        
        // 展示选中信息
        Node_currentStar.SetInfo(_heroInfo,StaticData.HeroStarTable.TryGet(starId));
    }

    private async void _OnHighlight(HeroStarSpot star)
    {
        // 还需要播放一个升星特效
        var effect = _heroInfo.StarConfig.Starlevel == 0 ? _bigEffect : _smallEffect;
        var effectInstance = PageBucket.Pool.Reuse<RecycledGameObject>(effect);
        var effectTransform = effectInstance.transform;
        effectTransform.SetParent(transform);
        effectTransform.position = star.transform.position;
        effectInstance.SetLocalScale(Vector3.one);
        // 到时间就销毁
        await Task.Delay(1000);
        effectInstance.Recycle();
    }
}