using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Linq;
using UnityEngine.UI;

[ServiceDescription("驱动 UIEngine")]
public class UIEngineService : Service
{
    public override void OnCreate()
    {
        UIEngine.OnLoadPrefabAsync += OnLoadPrefabAsync;
        UIEngine.OnLoadPrefabImediately += OnLoadPrefabImediately;

        UIEngine.OnNewPageCreated += this.OnNewPageCreated;
        UIEngine.FloatingCreated += this.OnFloatingCreated;
        UIEngine.OnNavigated += OnNavigatedHandler;

        // 驱动 UIEngine
        UIEngine.RequestBlock = this.Block;
        UIEngine.RequestRemoveBlock = this.RemoveBlock;

        UIEngine.FloatingDisplayChanged = OnFloatingDisplayChnaged;

        UIEngine.RequestTransactionBlock = OnTransactionBlock;
        UIEngine.RequestRemoveTransactionBlock = OnRemoveTransactionBlock;

        UIEngine.RequestInvisibleBlock = this.InvisibelBlock;
        UIEngine.RequestRemoveInvisibleBlock = this.RemoveInvisibleBlock;
    }

    async Task OnTransactionBlock()
    {
        //BlockManager.Stuff.AddBlock("UIEngine.Transaction", BlockLevel.Transaction);
        //var f = UIEngine.Stuff.FindFloating<BlockFloating>();
        //await f.WaitTansactionStayState();
        await BlockManager.Stuff.TransactionInAsync("UIEngine.Transaction");
    }

    async Task OnRemoveTransactionBlock()
    {
        BlockManager.Stuff.RemoveBlock("UIEngine.Transaction");
    }

    void InvisibelBlock()
    {
        var key = "UIEngine.Invisible";
        BlockManager.Stuff.AddBlock(key, BlockLevel.Invisible);
    }

    void RemoveInvisibleBlock()
    {
        var key = "UIEngine.Invisible";
        BlockManager.Stuff.RemoveBlock(key);
    }

    void Block()
    {
        var key = "UIEngine";
        BlockManager.Stuff.AddBlock(key, BlockLevel.Visible);
    }

    void RemoveBlock()
    {
        var key = "UIEngine";
        BlockManager.Stuff.RemoveBlock(key);
    }

    static void OnFloatingDisplayChnaged(Floating floating, bool isShow)
    {
        var floatingName = floating.name;
        if(isShow)
        {
            //     //var row = ClientConfig.AudioUiPrefabTable.TryGet(floatingName);
            //     //if (row != null)
            //     //{
            //     //    var eventList = row.openEventName;
            //     //    WwiseManager.TryPostEventList(eventList);
            //     //}
            //     WwiseUtil.TryPostTimeLineOpenEvent(floatingName);
            // }
            // else
            // {
            //     //var row = ClientConfig.AudioUiPrefabTable.TryGet(floatingName);
            //     //if (row != null)
            //     //{
            //     //    var eventList = row.closeEventName;
            //     //    WwiseManager.TryPostEventList(eventList);
            //     //}
            //     WwiseUtil.TryPostUIPrefabCloseEvent(floatingName);
            //WwiseEventManager.OnUiOpen(floatingName);
            WwiseEventManager.SendEvent(TransformTable.UiOpen, floatingName);
        }
        else
        {
            //WwiseEventManager.OnUiClose(floatingName);
            WwiseEventManager.SendEvent(TransformTable.UiClose, floatingName);
        }
    }

    public void OnNavigatedHandler(PageNavigateInfo info)
    {

        GuideManagerV2.Stuff.Notify("navigated");
        
    }

    public void OnFloatingCreated(GameObject floating)
    {
        // 给所有 Button 组件添加 ButtonSe 组件
        var buttonList = floating.GetComponentsInChildren<Button>(true);
        foreach (var button in buttonList)
        {
            if (button.GetComponent<ButtonExtra>() == null)
            {
                button.gameObject.AddComponent<ButtonExtra>();
            }
        }

        var textList = floating.GetComponentsInChildren<Text>(true);
        foreach (var text in textList)
        {
            if (text.GetComponent<FontLocalizer>() == null)
            {
                text.gameObject.AddComponent<FontLocalizer>();
            }
        }

        // 如果没有动画器，添加默认动画器
        var root = floating.transform.Find("root");
        if(root != null)
        {
            var animator = floating.GetComponent<Animator>();
            if (animator == null)
            {
                animator = floating.gameObject.AddComponent<Animator>();
                var controller = BucketManager.Stuff.Main.Get<RuntimeAnimatorController>("PopWindowAnimator.controller");
                if (controller != null)
                {
                    animator.runtimeAnimatorController = controller;
                    animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                }
            }
        }

    }

    public void OnNewPageCreated(Page page)
    {
        // 给所有 Button 组件添加 ButtonSe 组件
        var buttonList = page.GetComponentsInChildren<Button>();
        foreach (var button in buttonList)
        {
            if (button.GetComponent<ButtonExtra>() == null)
            {
                button.gameObject.AddComponent<ButtonExtra>();
            }
        }

        var textList = page.GetComponentsInChildren<Text>();
        foreach (var text in textList)
        {
            if (text.GetComponent<FontLocalizer>() == null)
            {
                text.gameObject.AddComponent<FontLocalizer>();
            }
        }

        // 如果没有动画器，添加默认动画器
        if(page.Overlay)
        {
            var animator = page.GetComponent<Animator>();
            if (animator == null)
            {
                animator = page.gameObject.AddComponent<Animator>();
                var controller = BucketManager.Stuff.Main.Get<RuntimeAnimatorController>("PageDisplayAnimator.controller");
                if (controller != null)
                {
                    animator.runtimeAnimatorController = controller;
                    animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                }
            }

        }



    }


    async Task<GameObject> OnLoadPrefabAsync(string nameWithExt)
    {
        GameObject prefab = null;

        // 尝试从 Addressble 系统中加载
        if (prefab == null)
        {
            var bucket = BucketManager.Stuff.Main;
            prefab = await bucket.GetOrAquireAsync<GameObject>(nameWithExt, true);
        }

        // 如果没有则尝试从 Resources 系统中加载
        if (prefab == null)
        {
            var noExtName = Path.GetFileNameWithoutExtension(nameWithExt);
            prefab = Resources.Load<GameObject>(noExtName);
        }

        if (prefab == null)
        {
            throw new Exception($"[UIEngineService] load {nameWithExt} fail");
        }

        return prefab;
    }

    GameObject OnLoadPrefabImediately(string nameWithExt)
    {
        // 尝试从中心资源缓存器获得
        //var prefab = AssetCacher.Stuff.Get<GameObject>(nameWithExt, true);
        var bucket = BucketManager.Stuff.Main;
        var prefab = bucket.Get<GameObject>(nameWithExt, true);

        // 尝试从 Addresable 缓存系统中获取
        //if(prefab == null)
        //{
        //    prefab = AddressableRes.Get<GameObject>(nameWithExt, true);
        //}

        // 如果没有则尝试从 Resources 系统中加载
        if (prefab == null)
        {
            var noExtName = Path.GetFileNameWithoutExtension(nameWithExt);
            prefab = Resources.Load<GameObject>(noExtName);
        }

        if (prefab == null)
        {
            throw new Exception($"[UIEngineService] get {nameWithExt} fail");
        }

        return prefab;
    }
}