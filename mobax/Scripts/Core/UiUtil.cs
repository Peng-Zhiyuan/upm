using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using System.Threading.Tasks;
using Spine.Unity;
using UnityEngine.U2D;
using CustomLitJson;
using Facebook.Unity;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

public static class UiUtil
{
    public static async Task BackToMainGroupThenForward<T>() where T : Page
    {
        var info = new PageNavigateInfo {secondBackToTag = "main", fourthForward = typeof(T).Name,};
        await UIEngine.Stuff.ProcessNavigationV2Async(info);
    }

    /// <summary>
    /// 回到主页面组，然后替换页面
    /// </summary>
    public static async Task<Page> BackToMainGroupThenReplaceAsync(string replacedToPageName, object param = null,
        bool useTransaction = false)
    {
        var info = new PageNavigateInfo();
        info.secondBackToTag = "main";
        info.thirdBack = true;
        info.fourthForward = replacedToPageName;
        info.param = param;
        info.useTransaction = useTransaction;
        await UIEngine.Stuff.ProcessNavigationV2Async(info);
        var toPage = info.to;
        return toPage.Page;
    }

    /// <summary>
    /// 回到主页面组，然后替换页面
    /// </summary>
    public static async void BackToMainGroupThenReplace(string replacedToPageName, object param = null,
        bool useTransaction = false)
    {
        await BackToMainGroupThenReplaceAsync(replacedToPageName, param, useTransaction);
    }

    /// <summary>
    /// 回到主页面组，然后替换页面
    /// </summary>
    public static async Task<Page> BackToMainGroupThenReplaceAsync<T>(object param = null)
    {
        var type = typeof(T);
        var name = type.Name;
        var page = await BackToMainGroupThenReplaceAsync(name, param);
        return page;
    }

    /// <summary>
    /// 回到主页面组，然后替换页面
    /// </summary>
    public static async void BackToMainGroupThenReplace<T>(object param = null)
    {
        var type = typeof(T);
        var name = type.Name;
        await BackToMainGroupThenReplaceAsync(name, param);
    }

    public static void ShowReward(List<ItemInfo> addItemList)
    {
        if (addItemList.Count == 0)
        {
            return;
        }

        var f = UIEngine.Stuff.ShowFloatingImediatly<RewardFloating>(wwise: "ui_reward");
        addItemList = FilterItem(addItemList);
        f.AddItem(addItemList);
    }

    public static void ShowReward(List<ItemInfo> addItemList, Action onCloseCallBack)
    {
        addItemList = FilterItem(addItemList);
        if (addItemList.Count == 0)
        {
            onCloseCallBack?.Invoke();
            return;
        }

        var f = UIEngine.Stuff.ShowFloatingImediatly<RewardFloating>(wwise: "ui_reward");
        f.AddItem(addItemList);
        f.AddCloseListener(onCloseCallBack);
    }

    public static List<DatabaseTransaction> JsonDataListToDatabaseTransactionList(List<JsonData> jsonDataList)
    {
        var transactionList = new List<DatabaseTransaction>();
        if (jsonDataList == null) return transactionList;
        foreach (var jd in jsonDataList)
        {
            var transaction = JsonUtil.JsonDataToObject<DatabaseTransaction>(jd);
            transactionList.Add(transaction);
        }

        return transactionList;
    }

    public static List<ItemInfo> CacheToDisplayItemList(List<JsonData> jsonDataTransactionList)
    {
        if (jsonDataTransactionList == null)
        {
            return new List<ItemInfo>();
        }

        var transactionList = JsonDataListToDatabaseTransactionList(jsonDataTransactionList);
        var addedItemInfoList = DatabaseTransactionListToDisplayableItemInfoList(transactionList);
        return addedItemInfoList;
    }

    public static void ShowRewardByCache(List<JsonData> jsonDataTransactionList, Action onCloseCallBack = null)
    {
        var addedItemInfoList = CacheToDisplayItemList(jsonDataTransactionList);
        ShowReward(addedItemInfoList, onCloseCallBack);
    }

    public static void ShowRewardByCache(List<DatabaseTransaction> transactionList)
    {
        var addedItemInfoList = DatabaseTransactionListToDisplayableItemInfoList(transactionList);
        ShowReward(addedItemInfoList);
    }

    public static void CleanAndShowAllCachedReward()
    {
        var list = Database.Stuff.PopRecordedTransaction();
        ShowRewardByCache(list);
    }

    public static async Task SetGrey(Image image)
    {
        var address = "UIGrey.mat";
        //this.greyMaterial = await AddressableRes.AquireAsync<Material>(address);
        var bucket = BucketManager.Stuff.Battle;
        var mat = await bucket.GetOrAquireAsync<Material>(address);
        if (mat != null)
        {
            image.material = mat;
        }
    }

    // pzy:
    // 本票不显示，临时逻辑
    // 应当统一使用 bag 判断
    static bool IsDisplayableItem(int id)
    {
        var itype = StaticDataUtil.GetIType(id);
        if (itype == 4)
        {
            // 门票
            return false;
        }

        return true;
    }

    public static List<ItemInfo> DatabaseTransactionListToDisplayableItemInfoList(
        List<DatabaseTransaction> transactionList)
    {
        var ret = new List<ItemInfo>();
        foreach (var one in transactionList)
        {
            var rowId = one.id;
            var dataType = StaticDataUtil.GetServerDataModel(rowId);
            if (dataType == ServerDataModel.Item)
            {
                // 月卡
                if (rowId == 9401
                    || rowId == 9402)
                {
                    continue;
                }

                var transactionType = one.t;
                if (transactionType == TransactionType.Add
                    || transactionType == TransactionType.New)
                {
                    var r = one.r;
                    if (r != null
                        && r.IsArray)
                    {
                        var list = one.r.ToList();
                        foreach (var jd in list)
                        {
                            var info = JsonUtil.JsonDataToObject<ItemInfo>(jd);
                            var isDisplayable = IsDisplayableItem(info.id);
                            if (isDisplayable)
                            {
                                ret.Add(info);
                            }
                        }
                    }
                    else
                    {
                        var isDisplayable = IsDisplayableItem(one.id);
                        if (!isDisplayable)
                        {
                            continue;
                        }

                        var itemInfo = new ItemInfo {id = one.id, _id = one._id, val = one.v.ToInt()};
                        ret.Add(itemInfo);
                    }
                }
            }
        }

        ret = CombineItemInfoList(ret);
        return FilterItem(ret);
    }

    static List<ItemInfo> FilterItem(List<ItemInfo> itemInfos)
    {
        return itemInfos.FindAll(val =>
            {
                var iType = StaticDataUtil.GetIType(val.id);
                return iType != IType.Plot;
            }
        );
    }

    static List<VirtualItem> CombineVritualList(List<VirtualItem> virtualItemList)
    {
        var dic = new Dictionary<int, VirtualItem>();
        foreach (var one in virtualItemList)
        {
            var id = one.id;
            var item = DictionaryUtil.TryGet(dic, id, null);
            if (item != null)
            {
                item.val += one.val;
            }
            else
            {
                dic[id] = one;
            }
        }

        var ret = new List<VirtualItem>();
        foreach (var one in dic.Values)
        {
            ret.Add(one);
        }

        return ret;
    }

    // pzy:
    // item 是物品实例，实例本身就是不同的，没有合并一说，在此处作为参数不合理，应当是 virtual item
    static List<ItemInfo> CombineItemInfoList(List<ItemInfo> itemInfoList)
    {
        var dic = new Dictionary<int, ItemInfo>();
        var ret = new List<ItemInfo>();
        foreach (var one in itemInfoList)
        {
            // 拼图的不能被拆
            if (one.bag == IType.Puzzle)
            {
                ret.Add(one);
                continue;
            }

            var id = one.id;
            var item = DictionaryUtil.TryGet(dic, id, null);
            if (item != null)
            {
                item.val += one.val;
            }
            else
            {
                dic[id] = one;
            }
        }

        foreach (var one in dic.Values)
        {
            ret.Add(one);
        }

        return ret;
    }

    public async static Task ConversationAsync(int storyChatId)
    {
        var page = await UIEngine.Stuff.ForwardAsync<ConversationPage>(storyChatId);
        await page.WaitCompleteAsync();
    }

    public static void SetAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null) return;
        var c = graphic.color;
        c.a = alpha;
        graphic.color = c;
    }

    static void TweenAlphaTo1(Graphic graphic, float alpha = 1)
    {
        graphic.DOFade(alpha, 0.2f);
    }

    public static void SetSpriteBlank(Image image)
    {
        image.sprite = null;
        SetAlpha(image, 0);
    }

    /// <summary>
    /// 在后台设置 Image 的 sprite，会妥善且优雅的处理资源获取过程。
    /// 注意：如果无法显示，会将 alpha 设置为0，如果能显示图片，最终会将 alpha 设置为 1。
    /// </summary>
    /// <param name="image"></param>
    /// <param name="requestAddressHandler"></param>
    public static async void SetSpriteInBackground(Image image, Func<string> requestAddressHandler,
        Action<bool> setDefaultCallback = null, float alpha = 1, Bucket bucket = null, bool ignoreAlpha = false,
        bool emptyVis = true)
    {
        if (requestAddressHandler == null)
        {
            image.sprite = null;
            image.DOKill(false);
            SetAlpha(image, 0);
            return;
        }

        var address = requestAddressHandler.Invoke();
        if (bucket == null)
        {
            bucket = BucketManager.Stuff.GetBucket(UIEngine.LatestNavigatePageName);
            // bucket = BuketManager.Stuff.Main;
        }

        // if (UIEngine.LatestNavigatePageName != null)
        // {
        //     bucket = BuketManager.Stuff.GetBucket(UIEngine.LatestNavigatePageName);
        // }
        var alreadyLoaded = bucket.IsAddressAquired(address);
        if (alreadyLoaded)
        {
            var sprite = bucket.Get<Sprite>(address);
            image.sprite = sprite;
            image.DOKill(false);
            SetAlpha(image, alpha);
            if (setDefaultCallback != null)
            {
                if (sprite == null)
                {
                    setDefaultCallback.Invoke(true);
                }
                else
                {
                    setDefaultCallback.Invoke(false);
                }
            }
        }
        else
        {
            if (!emptyVis)
            {
                image.SetActive(false);
            }
            else
            {
                image.sprite = null;
                image.DOKill(false);
                SetAlpha(image, 0);
            }

            var sprite = await bucket.GetOrAquireAsync<Sprite>(address, true);
            if (image == null)
            {
                return;
            }

            var postAddress = requestAddressHandler.Invoke();
            if (address != postAddress)
            {
                return;
            }

            image.sprite = sprite;

            if (!emptyVis)
            {
                image.SetActive(true);
            }
            else
            {
                if (!ignoreAlpha)
                    TweenAlphaTo1(image, alpha);
                else
                {
                    var c = image.color;
                    c.a = 1;
                    image.color = c;
                }
            }

            if (setDefaultCallback != null)
            {
                if (sprite == null)
                {
                    setDefaultCallback.Invoke(true);
                }
                else
                {
                    setDefaultCallback.Invoke(false);
                }
            }
        }
    }

    /// <summary>
    /// 在后台设置 Image 的 Atlas sprite，会妥善且优雅的处理资源获取过程。
    /// 注意：如果无法显示，会将 alpha 设置为0，如果能显示图片，最终会将 alpha 设置为 1。
    /// </summary>
    /// <param name="image"></param>
    /// <param name="requestSpriteHandler"></param>
    public static async void SetAtlasSpriteInBackground(Image image, string atlasAddress,
        Func<string> requestSpriteHandler, Action<bool> setDefaultStyleHandler = null, float alpha = 1)
    {
        if (requestSpriteHandler == null)
        {
            image.sprite = null;
            image.DOKill(false);
            SetAlpha(image, 0);
            return;
        }

        var spriteName = requestSpriteHandler.Invoke();
        var bucket = BucketManager.Stuff.GetBucket(UIEngine.LatestNavigatePageName);
        var alreadyLoaded = bucket.IsAddressAquired(atlasAddress);
        if (alreadyLoaded)
        {
            var atlas = bucket.Get<SpriteAtlas>(atlasAddress);
            image.sprite = atlas.GetSprite(spriteName);
            image.DOKill(false);
            SetAlpha(image, alpha);
            if (setDefaultStyleHandler != null)
            {
                if (atlas == null)
                {
                    setDefaultStyleHandler.Invoke(true);
                }
                else
                {
                    setDefaultStyleHandler.Invoke(false);
                }
            }
        }
        else
        {
            image.sprite = null;
            image.DOKill(false);
            SetAlpha(image, 0);
            var atlas = await bucket.GetOrAquireAsync<SpriteAtlas>(atlasAddress, true);
            //var sprite = await bucket.GetOrAquireAsync<Sprite>(address, true);
            if (image == null)
            {
                return;
            }

            var postAddress = atlasAddress;
            if (atlasAddress != postAddress)
            {
                return;
            }

            if (atlas != null)
            {
                image.sprite = atlas.GetSprite(spriteName);
            }

            TweenAlphaTo1(image, alpha);
            if (setDefaultStyleHandler != null)
            {
                if (atlas == null)
                {
                    setDefaultStyleHandler.Invoke(true);
                }
                else
                {
                    setDefaultStyleHandler.Invoke(false);
                }
            }
        }
    }

    public static async void SetSkeleton(SkeletonGraphic skeletonGraphic, SkeletonDataAsset asset, string animationName,
        bool isLoop, float delay)
    {
        var oldAsset = skeletonGraphic.skeletonDataAsset;
        var oldLoop = skeletonGraphic.startingLoop;
        var oldAnimationName = skeletonGraphic.startingAnimation;
        if (oldAsset == asset
            && oldLoop == isLoop
            && oldAnimationName == animationName)
        {
            return;
        }

        skeletonGraphic.skeletonDataAsset = asset;
        skeletonGraphic.startingLoop = isLoop;
        if (string.IsNullOrEmpty(oldAnimationName))
        {
            oldAnimationName = "idle";
            skeletonGraphic.startingAnimation = oldAnimationName;
        }

        // skeletonGraphic.timeScale = 0.4f;
        if (skeletonGraphic.canvasRenderer == null)
        {
            Debug.LogError("skeletonGraphic.canvasRenderer is null");
            return;
        }

        // 不是同一个资源需要重新渲染
        if (oldAsset != asset)
        {
            skeletonGraphic.Initialize(true);

            // 轨道动画不存在
            if (skeletonGraphic.SkeletonData.FindAnimation(animationName) == null)
            {
                return;
            }

            skeletonGraphic.AnimationState.AddAnimation(0, animationName, true, 0);
            skeletonGraphic.startingAnimation = animationName;
        }
        else
        {
            // 轨道动画不存在
            if (skeletonGraphic.SkeletonData.FindAnimation(animationName) == null)
            {
                return;
            }

            // 设置动画混合
            // var stateData = skeletonGraphic.SkeletonDataAsset.GetAnimationStateData();
            // var oldAnimation = stateData.SkeletonData.FindAnimation(oldAnimationName);
            // var curAnimation = stateData.SkeletonData.FindAnimation(animationName);
            // if (oldAnimation != null && curAnimation != null)
            // {
            //     var mixTime = stateData.GetMix(oldAnimation, curAnimation);
            //     stateData.SetMix(oldAnimationName, animationName, mixTime);
            // }
            //
            // skeletonGraphic.AnimationState.AddAnimation(0, animationName, true, 0);
            skeletonGraphic.startingAnimation = animationName;
        }
    }

    /// <summary>
    /// 在后台设置 Image 的 sprite，会妥善且优雅的处理资源获取过程。
    /// 注意：如果无法显示，会将 alpha 设置为0，如果能显示图片，最终会将 alpha 设置为 1。
    /// </summary>
    /// <param name="image"></param>
    /// <param name="requestAddressHandler"></param>
    public static async void SetSkeletonInBackground(SkeletonGraphic skeletonGraphic,
        Func<string> requestAddressHandler, Action<bool> setDefaultStyleHandler = null, string actionName = "idle",
        float delay = 0, Material material = null)
    {
        if (requestAddressHandler == null)
        {
            skeletonGraphic.skeletonDataAsset = null;
            return;
        }
        
        if (material != null)
        {
            skeletonGraphic.material = material;
        }
        var address = requestAddressHandler.Invoke();
        var bucket = BucketManager.Stuff.GetBucket(UIEngine.LatestNavigatePageName);
        var alreadyLoaded = bucket.IsAddressAquired(address);
        if (alreadyLoaded)
        {
            // Debug.LogError("ALREADY_LOADED:" + address);
            var data = bucket.Get<SkeletonDataAsset>(address);
            SetSkeleton(skeletonGraphic, data, actionName, true, delay);
            SetAlpha(skeletonGraphic, 1);
            if (setDefaultStyleHandler != null)
            {
                if (data == null)
                {
                    setDefaultStyleHandler.Invoke(true);
                }
                else
                {
                    setDefaultStyleHandler.Invoke(false);
                }
            }
        }
        else
        {
            skeletonGraphic.skeletonDataAsset = null;
            SetAlpha(skeletonGraphic, 0);
            var data = await bucket.GetOrAquireAsync<SkeletonDataAsset>(address, true);
            if (data == null)
            {
                return;
            }

            var postAddress = requestAddressHandler.Invoke();
            if (address != postAddress)
            {
                return;
            }

            SetSkeleton(skeletonGraphic, data, actionName, true, delay);
            TweenAlphaTo1(skeletonGraphic);
            if (setDefaultStyleHandler != null)
            {
                if (data == null)
                {
                    setDefaultStyleHandler.Invoke(true);
                }
                else
                {
                    setDefaultStyleHandler.Invoke(false);
                }
            }
        }
    }

    /// <summary>
    /// 检查物品是否足够，如果不够弹窗并且抛出静默异常
    /// </summary>
    /// <param name="itenId"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static void CheckEnough(int itenId, int count)
    {
        var holdCount = Database.Stuff.itemDatabase.GetHoldCount(itenId);
        if (holdCount >= count)
        {
            return;
        }

        var row = StaticData.ItemTable[itenId];
        var name = row.Name;
        name = LocalizationManager.Stuff.GetText(name);
        Dialog.Confirm("", $" <color=#ffff00>{name}</color> not enouph");
        throw new GameException(ExceptionFlag.Silent, $"item {itenId} not enouph ({holdCount}/{count})");
    }

    public static void ScaleBling(Transform transform, float endScale, float recoveScale, float durT1, float durT2,
        Action callback, string name)
    {
        DOTween.Kill($"bling_{transform.name}_{name}");
        Sequence seq = DOTween.Sequence();
        seq.SetId($"bling_{transform.name}_{name}");
        seq.Append(transform.DOScale(endScale, durT1));
        seq.Append(transform.DOScale(recoveScale, durT2));
        seq.AppendCallback(delegate
            {
                if (callback != null)
                    callback.Invoke();
            }
        );
    }

    public static void ShowRollText(Text text, string des, bool bForce = false, bool bReset = false, float speed = 60f,
        int time = -1, Action endCallBack = null)
    {
        if (time == 0)
        {
            if (endCallBack != null)
            {
                endCallBack.Invoke();
            }

            return;
        }

        if (time > 0)
            time--;
        if (bReset)
            DOTween.Kill(text.name);
        text.text = des;
        var bg = text.transform;
        var bg_width = bg.transform.GetComponent<RectTransform>().sizeDelta.x;
        var pos = Vector3.zero;
        if (bg_width >= text.preferredWidth
            && !bForce)
        {
            if (bg.parent.GetComponent<Mask>() != null)
            {
                text.transform.localPosition = pos;
            }

            return;
        }

        if (bg.parent.GetComponent<Mask>() == null)
        {
            var image = new GameObject("mask");
            var img = image.AddComponent<Image>();
            var color = img.color;
            color.a = 0.01f;
            img.color = color;
            image.AddComponent<Mask>();
            image.transform.SetParent(text.transform.parent);
            image.transform.localPosition = text.transform.localPosition;
            image.transform.localScale = Vector3.one;
            var rect = image.transform.GetComponent<RectTransform>();
            rect.sizeDelta = text.transform.GetComponent<RectTransform>().sizeDelta;
            text.transform.SetParent(image.transform);
            //text.transform.localPosition = Vector3.zero;
        }

        pos.x = bg_width;
        if (bReset)
            text.transform.localPosition = Vector3.zero;
        else
            text.transform.localPosition = pos;
        var width = text.preferredWidth;
        var sequence = DOTween.Sequence();
        sequence.Append(
            text.transform.DOLocalMoveX(-width, (width + bg_width) / speed).SetEase(DG.Tweening.Ease.Linear));
        sequence.OnComplete(delegate { ShowRollText(text, des, bForce, bReset, speed, time, endCallBack); })
            .SetUpdate(true);
        if (bReset)
            sequence.SetId(text.name);
    }

    public static bool IsStageTriggerGift(int stageID)
    {
        List<GiftRow> lst = StaticData.GiftTable.ElementList;
        for (int i = 0; i < lst.Count; i++)
        {
            if (lst[i].Type == 1
                && lst[i].Param == stageID)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 检测弹出礼包
    /// </summary>
    public static async void CheckTriggerGift()
    {
        //如果这个引导没完成 那么统一不弹
        var isStepCompleted = true; //Database.Stuff.roleDatabase.Me.GetGuide("dispatch", -1) >= 1;
        if (GuideManagerV2.Stuff.IsExecutingForceGuide
            || GuideManagerV2.Stuff.IsExecutingTriggredGuide
            || !isStepCompleted)
        {
            return;
        }

        GiftInfo triggerGift = GiftTriggerManager.Instance.GetNewGiftInfo();
        if (triggerGift == null)
        {
            return;
        }

        var f = await UIEngine.Stuff.ShowFloatingAsync<ShopPopEventGiftFloating>();
        f.SetFloating(triggerGift, false);
        GiftTriggerManager.Instance.SetGiftCheckTime(Clock.Now);
    }

    //显示道具界面
    public static async Task ShowItemPanel()
    {
        var page = UIEngine.Stuff.FindPage<BattlePage>();
        if (page != null)
        {
            await page.ShowItemPanel();
        }
    }

    public static async void ShowBattleUI(List<EGuideBattleUIType> uiTypes)
    {
        var page = UIEngine.Stuff.FindPage<BattlePage>();
        if (page == null)
            return;

        page.SetUIVis(uiTypes, true);
    }

    public static async void HideBattleUI(List<EGuideBattleUIType> uiTypes)
    {
        var page = UIEngine.Stuff.FindPage<BattlePage>();
        if (page == null)
            return;

        page.SetUIVis(uiTypes, false);
    }

    public static async void ShowOrHideMainPageUI(List<EMainPageUIType> uiTypes, bool isShow)
    {
        var page = UIEngine.Stuff.FindPage<MainPage>();
        if (page != null)
        {
            foreach (var uiType in uiTypes)
            {
                page.ShowOrHideBtnUI(uiType, isShow);
            }
        }
    }

    //隐藏道具界面
    public static async Task HideItemPanel()
    {
        var page = UIEngine.Stuff.FindPage<BattlePage>();
        if (page != null)
        {
            await page.HideItemPanel();
        }
    }

    /** 是否点到目标物 */
    public static bool HitTest(Transform tf)
    {
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        PointerEventData p = new PointerEventData(EventSystem.current);
        p.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        EventSystem.current.RaycastAll(p, raycastResults);
        foreach (var raycastResult in raycastResults)
        {
            var node = raycastResult.gameObject.transform;
            while (node != null)
            {
                if (node == tf)
                {
                    return true;
                }

                node = node.parent;
            }
        }

        return false;
    }

    //隐藏悬浮界面
    public static void HideUI(Transform trans)
    {
        if (trans.gameObject.activeSelf
            && !UiUtil.HitTest(trans))
        {
            trans.SetActive(false);
        }
    }

    public static GameObject GetGuildObject(string name)
    {
        return GameObject.Find(name);
    }

    static bool IsMainGroupPage(string pageName)
    {
        if (pageName == nameof(HeroListPage))
        {
            return true;
        }
        else if (pageName == nameof(BagPage))
        {
            return true;
        }
        else if (pageName == nameof(MainPage))
        {
            return true;
        }
        else if (pageName == nameof(WorkshopComposePage))
        {
            return true;
        }
        else if (pageName == nameof(DrawCardPoolPage))
        {
            return true;
        }

        return false;
    }

    static bool IsStackHasTag(string tag)
    {
        var c = UIEngine.Stuff.pageStack.FindByTag(tag);
        if (c == null)
        {
            return false;
        }

        return true;
    }

    static bool IsPageInStack(string pageName)
    {
        var c = UIEngine.Stuff.pageStack.Find(pageName);
        if (c == null)
        {
            return false;
        }

        return true;
    }

    public static async void Navigate<T>(object param)
    {
        var pageName = typeof(T).Name;
        await NavigateAsync(pageName, param);
    }

    public static async Task NavigateAsync<T>(object param)
    {
        var pageName = typeof(T).Name;
        await NavigateAsync(pageName, param);
    }

    public static async Task NavigateAsync(string pageName, object param)
    {
        var isMainGroup = IsMainGroupPage(pageName);
        if (isMainGroup)
        {
            if (IsStackHasTag("main"))
            {
                await UiUtil.BackToMainGroupThenReplaceAsync(pageName, param);
                return;
            }
            else
            {
                await UIEngine.Stuff.ForwardAsync(pageName, param);
            }
        }
        else
        {
            if (IsPageInStack(pageName))
            {
                await UIEngine.Stuff.BackToAsync(pageName, param);
            }
            else
            {
                await UIEngine.Stuff.ForwardAsync(pageName, param);
            }
        }
    }


    public static void SetActive(GameObject go, bool vis)
    {
        var com = go.GetOrAddComponent<GameObjectExt>();
        com.SetActive(vis);
    }
}