using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Crystal;
using Sirenix.OdinInspector;
using System.Runtime.CompilerServices;
using System.Linq;

public class UIEngine : ResourcesObject<UIEngine>
{
    [HideInInspector] public PageStack pageStack = new PageStack();


    Dictionary<string, Floating> displayingFloatingDic = new Dictionary<string, Floating>();

    public PageContainer prefab_pageContainer;

    static public Action<PageNavigateInfo> OnNavigated;
    static public Action<PageNavigateInfo> OnBeforeNavigatTo;

    public static Action<Floating, bool> FloatingDisplayChanged;
    public static Action<Page> OnNewPageCreated;

    public static Action RequestBlock;
    public static Action RequestRemoveBlock;

    public static Action RequestInvisibleBlock;
    public static Action RequestRemoveInvisibleBlock;

    public static Func<Task> RequestTransactionBlock;
    public static Func<Task> RequestRemoveTransactionBlock;
    //public static Action<Page> PageDisplayAnim;


    public static Func<string, GameObject> OnLoadPrefabImediately;
    public static Func<string, Task<GameObject>> OnLoadPrefabAsync;

    public Transform transform_safeAreaDebugDisplay;
    public RectTransform transofrm_safeArea;

    public static event Action<GameObject> FloatingCreated;

    public SafeArea safeArea;

    [ShowInInspector]
    public SafeArea.SimDevice SimlationSafeAreDevice
    {
        set { SafeArea.Sim = value; }
        get { return SafeArea.Sim; }
    }

    [ShowInInspector]
    public bool IsShowSafeAreaDebug
    {
        set { this.transform_safeAreaDebugDisplay.gameObject.SetActive(value); }
        get { return this.transform_safeAreaDebugDisplay.gameObject.activeSelf; }
    }

    public void Awake()
    {
        GameObject.DontDestroyOnLoad(this.gameObject);
        this.transform_safeAreaDebugDisplay.gameObject.SetActive(false);

        this.safeArea = this.GetComponentInChildren<SafeArea>();
    }

    //public bool IsCanvasEnabled
    //{
    //    get { return this.Canvas.enabled; }
    //    set { this.Canvas.enabled = value; }
    //}

    public bool IsPageLayerEnabled
    {
        get
        {
            var pageRoot = this.LayerToTransform(UILayer.PageLayer);
            return pageRoot.activeSelf;
        }
        set
        {
            var pageRoot = this.LayerToTransform(UILayer.PageLayer);
            pageRoot.SetActive(value);
        }
    }

    private Canvas _canvas;

    public Canvas Canvas
    {
        get
        {
            if (_canvas == null)
            {
                _canvas = this.gameObject.GetComponent<Canvas>();
            }

            return _canvas;
        }
    }

    public event Action CanvasSizeChnaged;

    private void OnRectTransformDimensionsChange()
    {
        CanvasSizeChnaged?.Invoke();
    }

    private RectTransform _canvasTransform;

    public RectTransform CanvasTransform
    {
        get
        {
            if (_canvasTransform == null)
            {
                _canvasTransform = Canvas.GetComponent<RectTransform>();
            }

            return _canvasTransform;
        }
    }

    private Camera _uiCamera;

    public Camera UICamera
    {
        get
        {
            if (_uiCamera == null)
            {
                if (Canvas.renderMode == RenderMode.ScreenSpaceCamera
                    || Canvas.renderMode == RenderMode.WorldSpace)
                {
                    _uiCamera = Canvas.worldCamera;
                }
                else if (Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    _uiCamera = null;
                }
            }

            return _uiCamera;
        }
    }

    PageContainer ReusePageContainer()
    {
        var container = BucketManager.Stuff.Main.Pool.Reuse<PageContainer>(this.prefab_pageContainer.gameObject);
        return container;
    }

    public static async Task<Page> ReusePageAsync(string pageName)
    {
        // 尝试复用一个 page 对像
        var prefabName = $"{pageName}.prefab";
        var prefab = await LoadPrefabAsync(prefabName);
        var page = BucketManager.Stuff.Main.Pool.Reuse<Page>(prefab);
        page.GameObject.name = pageName;
        page.Active = false;
        if (page.IsVirgin)
        {
            OnNewPageCreated?.Invoke(page);
        }

        return page;
    }

    async Task<Page> TakeOrCreatePageWithContainerAsync(string pageName)
    {
        var page = await ReusePageAsync(pageName);
        var container = this.ReusePageContainer();
        var parent = LayerToTransform(UILayer.PageLayer);
        container.transform.SetParent(parent.transform, false);
        container.Page = page;
        container.gameObject.SetActive(false);
        container.Active = false;
        return page;
    }

    public async Task<Page> ReplaceAsync(string pageName, object param = null, bool useTransaction = false, [CallerFilePath] string callerFilePath = null)
    {
        var info = new PageNavigateInfo();
        info.thirdBack = true;
        info.fourthForward = pageName;
        info.param = param;
        info.useTransaction = useTransaction;
        info.callerFilePath = callerFilePath;
        await this.ProcessNavigationV2Async(info);
        var to = info.to;
        return to.Page;
    }


    public async Task<Page> ForwardAsync(string pageName, object param = null, bool useTransaction = false, [CallerFilePath] string callerFilePath = null)
    {
        var info = new PageNavigateInfo();
        info.param = param;
        info.fourthForward = pageName;
        info.useTransaction = useTransaction;
        info.callerFilePath = callerFilePath;
        await this.ProcessNavigationV2Async(info);
        var to = info.to;
        return to.Page;
    }

    public async Task RemoveFromStackAsync(string name, [CallerFilePath] string callerFilePath = null)
    {
        if (Top == null)
        {
            return;
        }

        if (Top.container.pageName == name)
        {
            await BackAsync(null, false, callerFilePath);
        }
        else
        {
            var page = pageStack.Remove(name);
            if (page != null)
            {
                //pagePool.Put (name, page);
                page.Page.RecyclePageAndContainer();
            }

            await this.RecaculateActiveAsync();
        }
    }

    Dictionary<string, List<Action>> hookTargetPageNameToOperationDic = new Dictionary<string, List<Action>>();

    public void HookBack(string targetPageName, Action handler)
    {
        var list = DictionaryUtil.GetOrCreateList(this.hookTargetPageNameToOperationDic, targetPageName);
        list.Add(handler);
    }

    public Task WaitPageBackAsync(string targetPageName)
    {
        var tcs = new TaskCompletionSource<bool>();
        HookBack(targetPageName, () => { tcs.SetResult(true); });
        return tcs.Task;
    }

    public void TryInvokeBackHook(string targetPageName)
    {
        if (hookTargetPageNameToOperationDic.ContainsKey(targetPageName))
        {
            var handlerList = hookTargetPageNameToOperationDic[targetPageName];
            var copyList = new List<Action>();
            copyList.AddRange(handlerList);
            handlerList.Clear();
            foreach (var one in copyList)
            {
                one.Invoke();
            }
        }
    }

    public Page FindPage(string name)
    {
        var targetPage = pageStack.Find(name);
        return targetPage?.Page;
    }

    public Page FindPageByTag(string tag)
    {
        var targetPage = pageStack.FindByTag(tag);
        return targetPage?.Page;
    }

    public async Task<Page> BackToAsync(string name, object param = null, [CallerFilePath] string callerFilePath = null)
    {
        var targetPage = pageStack.Find(name);
        if (targetPage == null)
        {
            // page not in stack, can't pop to
            throw new Exception($"[UIEngine] page {name} not in stack, can't pop to. \n called from: {callerFilePath}");
        }

        var info = new PageNavigateInfo();
        info.firstBackTo = name;
        info.param = param;
        info.callerFilePath = callerFilePath;
        await this.ProcessNavigationV2Async(info);
        var to = info.to.Page;
        return to;
    }

    /// <summary>
    /// 某一个页面在页面堆栈中，应该可见吗？
    /// </summary>
    /// <returns></returns>
    public bool TestPageWillVisibilityInStack(Page page)
    {
        if (page == null)
        {
            return false;
        }

        for (var i = this.pageStack.ContainerList.Count - 1; i >= 0; i--)
        {
            var container = this.pageStack.ContainerList[i];
            if (container.Page == page)
            {
                return true;
            }

            if (!container.isOverlay)
            {
                return false;
            }
        }

        return false;
    }

    public void RecaculateActive()
    {
        var visible = true;
        for (var i = this.pageStack.ContainerList.Count - 1; i >= 0; i--)
        {
            var container = this.pageStack.ContainerList[i];
            container.Active = visible;
            if (container.Page != null)
            {
                container.Page.Active = visible;
            }

            if (container.Active)
            {
                //await container.RecreatePageIfNeedAsync();
            }
            else
            {
                container.DestoryPageIfNeed();
            }

            if (!container.isOverlay)
            {
                visible = false;
            }
        }
    }

    public async Task RecaculateActiveAsync()
    {
        var visible = true;
        for (var i = this.pageStack.ContainerList.Count - 1; i >= 0; i--)
        {
            var container = this.pageStack.ContainerList[i];
            container.Active = visible;
            if (container.Page != null)
            {
                container.Page.Active = visible;
            }

            if (container.Active)
            {
                await container.RecreatePageIfNeedAsync();
            }
            else
            {
                container.DestoryPageIfNeed();
            }

            if (!container.isOverlay)
            {
                visible = false;
            }
        }
    }

    public async void Back(object param = null)
    {
        await this.BackAsync(param);
    }

    public async Task BackAsync(object param = null, bool useTransaction = false, [CallerFilePath] string callerFilePath = null)
    {
        var info = new PageNavigateInfo();
        info.thirdBack = true;
        info.param = param;
        info.useTransaction = useTransaction;
        info.callerFilePath = callerFilePath;
        await this.ProcessNavigationV2Async(info);
        var from = info.from;
        var pageName = from.pageName;
        this.TryInvokeBackHook(pageName);
    }

    public Page Top
    {
        get { return pageStack.StackTryPeek()?.Page; }
    }

    public bool IsTop(string name)
    {
        var top = this.Top;
        var topName = top.Prefab.name;
        if (name == topName)
        {
            return true;
        }

        return false;
    }

    public int PagesCount
    {
        get { return pageStack.Count; }
    }

    public Floating FindFloating(string name)
    {
        Floating ret = null;
        displayingFloatingDic.TryGetValue(name, out ret);
        return ret;
    }

    public bool IsFloatingExists<T>()
    {
        var name = typeof(T).Name;
        var b = IsFloatingExists(name);
        return b;
    }

    public bool IsFloatingExists(string name)
    {
        var floating = FindFloating(name);
        if (floating != null)
        {
            return true;
        }

        return false;
    }

    Dictionary<UILayer, GameObject> layerToRootDic = new Dictionary<UILayer, GameObject>();

    public GameObject LayerToTransform(UILayer layer)
    {
        var root = DictionaryUtil.TryGet(layerToRootDic, layer, null);
        if (root != null)
        {
            return root;
        }

        var childName = layer.ToString();
        //var child = this.root.transform.Find(childName);
        var child = this.transofrm_safeArea.transform.Find(childName);
        if (child == null)
        {
            throw new Exception($"[UIEngine] layer `{childName}` not found");
        }

        layerToRootDic[layer] = child.gameObject;
        return child.gameObject;
    }

    GameObject LoadPrefab(string prefabName)
    {
        if (OnLoadPrefabImediately == null)
        {
            throw new Exception("[UIEngien] OnLoadPrefab handler not set yet!");
        }

        var prefab = OnLoadPrefabImediately.Invoke(prefabName);
        if (prefab == null)
        {
            throw new Exception($"[UIEngien] OnLoadPrefab {prefabName} get null");
        }

        return prefab;
    }

    async static Task<GameObject> LoadPrefabAsync(string prefabName)
    {
        if (OnLoadPrefabAsync == null)
        {
            throw new Exception("[UIEngien] OnLoadPrefabAsync handler not set yet!");
        }

        var task = OnLoadPrefabAsync.Invoke(prefabName);
        var prefab = await task;
        if (prefab == null)
        {
            throw new Exception($"[UIEngien] OnLoadPrefabAsync {prefabName} get null");
        }

        return prefab;
    }


    public async Task PreLoadFloatingAsync(string name, Page containerPage = null, UILayer layer = UILayer.UseDefault,
        object param = null)
    {
        var prefabName = $"{name}.prefab";
        await LoadPrefabAsync(prefabName);
    }


    public async Task PreLoadPageAsync(string pageName)
    {
        if (this.FindPage(pageName)) return;
        var prefabName = $"{pageName}.prefab";
        await LoadPrefabAsync(prefabName);
        return;
    }

    public void MoveFloatingLayer(Floating f, Page containerPage = null)
    {
        GameObject root = null;
        if (containerPage == null)
        {
            root = this.LayerToTransform(UILayer.NormalFloatingLayer);
        }
        else
        {
            root = containerPage.container.floatingRoot.gameObject;
        }

        f.transform.SetParent(root.transform, false);
    }


    UILayer DecideLayer(Floating prefab, UILayer callerSepecify)
    {
        if (callerSepecify == UILayer.UseDefault)
        {
            var ret = prefab.defaultLayer;
            return ret;
        }
        else
        {
            return callerSepecify;
        }
    }

    private Floating GetFloating(GameObject prefab, Page containerPage = null, UILayer layer = UILayer.UseDefault,
        object param = null)
    {
        var name = prefab.name;
        var old = this.FindFloating(name);
        if (old != null)
        {
            return old;
        }


        var floating =
            BucketManager.Stuff.Main.Pool.Reuse<Floating>(prefab, false, typeof(Floating), UIEngine.Stuff.transform);

        if (floating.IsVirgin)
        {
            FloatingCreated?.Invoke(floating.GameObject);
        }

        var finalLayer = DecideLayer(floating, layer);
        GameObject root;
        if (containerPage == null)
        {
            root = this.LayerToTransform(finalLayer);
        }
        else
        {
            root = containerPage.container.floatingRoot.gameObject;
        }

        floating.GameObject.name = name;
        floating.Transform.SetParent(root.transform, false);
        displayingFloatingDic[name] = floating;
        return floating;
    }

    public async Task<Floating> ShowFloatingAsync(string name, Page containerPage = null,
        UILayer layer = UILayer.UseDefault, object param = null, string wwise = "")
    {
        var prefabName = $"{name}.prefab";
        var prefab = await LoadPrefabAsync(prefabName);

        var floating = this.GetFloating(prefab, containerPage, layer, param);
        await floating.OnShowPreperAsync();
        floating.OnShow(param);

        FloatingDisplayChanged?.Invoke(floating, true);
        floating.GameObject.SetActive(true);
        if (!string.IsNullOrEmpty(wwise))
        {
            WwiseEventManager.SendEvent(TransformTable.UiControls, wwise);
        }

        floating.OnEnter();
        return floating;
    }

    public Floating ShowFloatingImediatly(string name, Page containerPage = null, UILayer layer = UILayer.UseDefault,
        object param = null, string wwise = "")
    {
        var prefabName = $"{name}.prefab";
        var prefab = LoadPrefab(prefabName);
        var floating = this.GetFloating(prefab, containerPage, layer, param);
        floating.GameObject.SetActive(true);
        if (!string.IsNullOrEmpty(wwise))
        {
            WwiseEventManager.SendEvent(TransformTable.UiControls, wwise);
        }

        return floating;
    }


    /// <summary>
    /// 移除 Floating
    /// 操作并不会立刻完成，因为包含异步操作
    /// </summary>
    /// <param name="name"></param>
    public async Task RemoveFloatingAsync(string name)
    {
        Floating floating;
        displayingFloatingDic.TryGetValue(name, out floating);
        if (floating == null)
        {
            // 并没有在显示，什么都不干
            return;
        }

        displayingFloatingDic.Remove(name);

        WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_smallWindowClose");
        floating.OnExit();
        await floating.WaitExitingCompelteAsync();

        floating.OnHide();
        floating.Recycle();
        
        FloatingDisplayChanged?.Invoke(floating, false);
    }

    /// <summary>
    /// 立即移除 Floating
    /// 不会触发异步操作
    /// </summary>
    /// <param name="name"></param>
    public void RemoveFloatingImediately(string name)
    {
        Floating floating;
        displayingFloatingDic.TryGetValue(name, out floating);
        if (floating == null)
        {
            // 并没有在显示，什么都不干
            return;
        }

        displayingFloatingDic.Remove(name);
        WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_smallWindowClose");
        floating.OnExit();

        floating.OnHide();
        floating.Recycle();
        
        FloatingDisplayChanged?.Invoke(floating, false);
    }

    public void RepositionMask()
    {
        var firstPage = pageStack.StackTryPeek();
        if (firstPage?.Page.Overlay == true)
        {
            var control = this.ShowFloatingImediatly("MaskFloating", null, UILayer.PageLayer)
                .GetComponent<MaskFloating>();
            if (control != null)
            {
                control.rectTransform.SetParent(null, false);
                control.rectTransform.SetParent(firstPage.Page.rectTransform.parent, false);
                var index = firstPage.Page.rectTransform.GetSiblingIndex();
                control.rectTransform.SetSiblingIndex(index);

                control.IsMaskTransparent = firstPage.Page.maskIsTranparent;
                control.clickToBack = firstPage.Page.clickMaskToBack;
            }
        }
        else
        {
            var floating = this.FindFloating("MaskFloating");
            if (floating != null)
            {
                floating.Remove();
            }
        }
    }

    public async Task<Page> ForwardOrBackToAsync(string name, object param = null,
        [CallerFilePath] string callerFilePath = null)
    {
        var oldPage = FindPage(name);
        if (oldPage != null)
        {
            var page = await BackToAsync(name, param, callerFilePath);
            return page;
        }
        else
        {
            var page = await ForwardAsync(name, param, false, callerFilePath);
            return page;
        }
    }

    bool isGlobalBackProcessing;

    /// <summary>
    /// 全局返回，同时只能进行一次全局处理
    /// </summary>
    public async void TryGlobalBack()
    {
        if (isGlobalBackProcessing)
        {
            Debug.Log("[UIEngine] already in GlobalBacking. skip.");
            return;
        }

        if (isNavigating)
        {
            Debug.Log("[UIEngine] already in navigating. skip.");
            return;
        }

        isGlobalBackProcessing = true;

        try
        {
            var target = this.FindGlobackBackTarget();
            if (target == null)
            {
                Debug.Log("[UIEngine] globalBack no target found.");
                return;
            }

            var hasLogicBack = target.hasLogicBack;
            Debug.Log("[UIEngine] globalBack, target: " + target.name + ", hasLogicBack: " + hasLogicBack);
            if (!hasLogicBack)
            {
                return;
            }

            await target.TryLogicBack();
        }
        finally
        {
            isGlobalBackProcessing = false;
        }
    }

    public UIEngineElement FindGlobackBackTarget()
    {
        // 搜索 floating
        var list = Enum.GetValues(typeof(UILayer));
        for (int i = list.Length - 1; i >= 0; i--)
        {
            var intV = (int) list.GetValue(i);
            var enmV = (UILayer) intV;
            if (enmV == UILayer.PageLayer)
            {
                // 不对页面进行搜索
                continue;
            }

            if (enmV == UILayer.UseDefault)
            {
                // 此值仅做语义站位处理
                continue;
            }

            var root = LayerToTransform(enmV).transform;
            var childCount = root.childCount;
            for (int j = childCount - 1; j >= 0; j--)
            {
                var transform = root.GetChild(j);
                var f = transform.GetComponent<Floating>();
                if (f == null)
                {
                    continue;
                }

                if (f.globalBackTarget)
                {
                    return f;
                }
            }
        }

        var topPage = this.Top;
        return topPage;
    }

    public void ShowUI()
    {
        this.gameObject.SetActive(true);
    }

    public void HideUI()
    {
        this.gameObject.SetActive(false);
    }

    async Task FillInInOutInfo(PageNavigateInfo info)
    {
        var from = this.Top;
        info.from = from?.container;
        var targetIndex = this.pageStack.Count - 1;
        var backTo = info.firstBackTo;
        if (backTo != null)
        {
            here:
            var pageList = this.pageStack.ContainerList;
            var container = pageList[targetIndex];
            var name = container.pageName;
            if (name != backTo)
            {
                targetIndex--;
                if (targetIndex == -1)
                {
                    throw new Exception($"[UIEngine] not found page `{backTo}` in operation BackTo");
                }

                goto here;
            }
        }

        var backToTag = info.secondBackToTag;
        if (backToTag != null)
        {
            here:
            var pageList = this.pageStack.ContainerList;
            var container = pageList[targetIndex];
            var tag = container.pageTag;
            if (tag != backToTag)
            {
                targetIndex--;
                if (targetIndex == -1)
                {
                    throw new Exception($"[UIEngine] not found page taged `{backToTag}` in operation BackToTag");
                }

                goto here;
            }
        }

        var back = info.thirdBack;
        if (back)
        {
            targetIndex--;
        }

        var forward = info.fourthForward;
        if (forward != null)
        {
            var list = this.pageStack.ContainerList;
            for (int i = 0; i <= targetIndex; i++)
            {
                var container = list[i];
                if (container.pageName == forward)
                {
                    throw new Exception($"[UIEngine] page {forward} already in stack, can't forward");
                }
            }

            var toPage = await this.TakeOrCreatePageWithContainerAsync(forward);
            info.terminalOperation = NavigateOperation.Forward;
            info.to = toPage.container;
            info.forwardNewPageContainer = toPage.container;
            return;
        }
        else
        {
            //var finalContianer = this.pageStack.ContainerList[targetIndex];
            var finalContianer = ListUtil.TryGet(this.pageStack.ContainerList, targetIndex);
            info.to = finalContianer;
            info.terminalOperation = NavigateOperation.Back;
            return;
        }
    }

    (List<PageContainer> popedList, List<PageContainer> pushedList) OperatePageStack(PageNavigateInfo info)
    {
        var popedList = new List<PageContainer>();
        var pushedList = new List<PageContainer>();
        var backTo = info.firstBackTo;
        if (backTo != null)
        {
            var page = this.pageStack.FromTopFindName(backTo);
            var thePopedList = this.pageStack.PopUtil(page);
            popedList.AddRange(thePopedList);
        }

        var backToTag = info.secondBackToTag;
        if (backToTag != null)
        {
            var page = this.pageStack.FromTopFindTag(backToTag);
            var thePopedList = this.pageStack.PopUtil(page);
            popedList.AddRange(thePopedList);
        }

        var back = info.thirdBack;
        if (back)
        {
            var page = this.pageStack.Pop();
            popedList.Add(page);
        }

        var forward = info.fourthForward;
        if (forward != null)
        {
            var page = info.forwardNewPageContainer;
            this.pageStack.Push(page);
            pushedList.Add(page);
        }

        return (popedList, pushedList);
    }

    static public string LatestNavigatePageName
    {
        get { return latestNavigateInfo != null ? latestNavigateInfo.to.pageName : null; }
    }

    static private PageNavigateInfo latestNavigateInfo = null;

    public bool isNavigating;
    PageNavigateInfo navigatingInfo;

    public async Task ProcessNavigationV2Async(PageNavigateInfo info)
    {
        if (isNavigating)
        {
            //Debug.LogError("[UIEngine] already in navigating. if called in OnNvagitedTo(), try use OnNavigatedToComplete() instead.");
            throw new Exception(
                $"[UIEngine] already in navigating. if called in OnNvagitedTo(), try use OnNavigatedToComplete() instead. \n navigating: {navigatingInfo} \n     call from: {navigatingInfo.callerFilePath} \n rejected: {info} \n      call from: {info.callerFilePath}");
        }

        isNavigating = true;
        navigatingInfo = info;

        RequestInvisibleBlock?.Invoke();


        // 转场进入
        if (info.useTransaction)
        {
            await RequestTransactionBlock();
        }

        try
        {
            RequestBlock?.Invoke();

            // 装填管线中参数
            await FillInInOutInfo(info);
            latestNavigateInfo = info;

            var to = info.to;
            var from = info.from;

           

            // 目标页面准备
            if (to != null && to.Page != null)
            {
                await to.Page?.OnNavigatedToPreperAsync(info);
            }

            // 导航离开事件
            if (from != null)
            {
                await from.Page?.OnNavigatedFromPreperAsync(info);
                from.Page?.OnNavigatedFrom(info);
            }



            RequestRemoveBlock?.Invoke();

            // 修改页面栈，但是不修改可见性
            var stackChnagedInfo = OperatePageStack(info);

            var infoRemovedCommon = UIEngineUtil.RemoveCommonName(stackChnagedInfo.popedList, stackChnagedInfo.pushedList);

            // 对于出栈的页面，应当播放退出动画
            {
                var taskList = new List<Task>();
                foreach (var container in infoRemovedCommon.popedList)
                {
                    container?.Page?.OnExit();
                    var task = container?.Page?.WaitExitingCompelteAsync();
                    if (task != null)
                    {
                        taskList.Add(task);
                    }
                }

                await Task.WhenAll(taskList);
            }

            //GuideManagerV2.Stuff.Notify($"{pageName}_display_finished");

            if (info.preChangeVisibility != null)
            {
                await info.preChangeVisibility.Invoke();
            }

            // 刷新页面的可见性
            await this.RecaculateActiveAsync();
            //this.RecaculateActive();

            // 重新计算 Mask 位置
            RepositionMask();

            // 导航到事件
            UIEngine.OnBeforeNavigatTo?.Invoke(info);
            if (to != null)
            {
                to.Page?.OnPreNavigatedTo(info);
                to.Page?.OnNavigatedTo(info);

                var oper = info.terminalOperation;
                if (oper == NavigateOperation.Forward)
                {
                    to.Page?.OnForwardTo(info);
                }
                else if (oper == NavigateOperation.Back)
                {
                    to.Page?.OnBackTo(info);
                }

                to.Page.ForceRefresh();
            }

            //if(info.preShowPageHandler != null)
            //{
            //    await info.preShowPageHandler.Invoke();
            //}

            // 转场退出
            if (info.useTransaction)
            {
                await RequestRemoveTransactionBlock();
            }

            // 对于入栈的页面，应当播放进入动画
            {
                var taskList = new List<Task>();
                foreach (var container in infoRemovedCommon.pushedList)
                {
                    container?.Page?.OnEnter();
                    var task = container?.Page?.WaitEnteringCompelteAsync();
                    if (task != null)
                    {
                        taskList.Add(task);
                    }
                }

                await Task.WhenAll(taskList);
            }

            // 回收出栈的页面
            var popedList = stackChnagedInfo.popedList;
            foreach (var one in popedList)
            {
                one?.Page.RecyclePageAndContainer();
            }

            // 导航已完成事件
            UIEngine.OnNavigated?.Invoke(info);
            InvokeOnetimeListner(info.to?.pageName);
            var str = this.pageStack.GetStackPrint();
            Debug.Log($"[UIEngine] {from?.pageName ?? "null"} -> {to?.pageName ?? "null"}    |    {str}"); 
        }
        finally
        {
            isNavigating = false;
            navigatingInfo = null;
            RequestRemoveInvisibleBlock?.Invoke();
            RequestRemoveBlock?.Invoke();
            if (info.useTransaction)
            {
                await RequestRemoveTransactionBlock();
            }
        }

        var to2 = info.to;
        if (to2 != null)
        {
            to2.Page?.OnNavigatedToComplete(info);
        }
    }


    // pzy:
    // 支持一次性监听

    static Dictionary<string, List<Action>> pageNameToOneTimeListnerDic = new Dictionary<string, List<Action>>();

    public static void InvokeOnetimeListner(string pageName)
    {
        if(pageName == null)
        {
            return;
        }
        var b = pageNameToOneTimeListnerDic.TryGetValue(pageName, out var list);
        if (!b)
        {
            return;
        }

        var copyList = new List<Action>(list);
        list.Clear();
        foreach (var l in copyList)
        {
            l.Invoke();
        }
    }

    public static void RegisterOnetimePageChangedListner(string pageName, Action action)
    {
        var list = DictionaryUtil.GetOrCreateList(pageNameToOneTimeListnerDic, pageName);
        list.Add(action);
    }


    void Update()
    {
#if UNITY_EDITOR
        // STF : 自己测试用,别传
        // if (Input.GetKeyDown(KeyCode.G))
        // {
        //     Debug.Log("gc");
        //     Resources.UnloadUnusedAssets();
        // }
        // if (Input.GetKeyDown(KeyCode.W))
        // {
        //     Debug.Log("resize");
        //     ScreenSizeHelper.ScreenSizeChange?.Invoke();
        // }
        //
        // if (Input.GetKeyDown(KeyCode.O))
        // {
        //     Debug.Log("open battle effect");
        //     var actors = BattleEngine.Logic.BattleManager.Instance.ActorMgr.GetAllActors();
        //     BattleEffect.Instance.StartFocus(actors[0].mData.UID);
        // }
        // if (Input.GetKeyDown(KeyCode.P))
        // {
        //     Debug.Log("open battle effect");
        //     BattleEffect.Instance.EndFocus();
        // }
#endif
        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    this.OnBackKey();
        //}
    }

    void OnBackKey()
    {
        // hook by sdk
        var sdkCanvas = FindObjectOfType<IggSdkCanvas>();
        var sdkHook = sdkCanvas.AnyPannelEnabled;
        if (sdkHook)
        {
            var highestPannel = sdkCanvas.HighestActivePannel;
            if (highestPannel.name == "AppConfPanel")
            {
                return;
            }

            highestPannel.gameObject.SetActive(false);
            return;
        }

        // 新手引导中禁用返回键
        if (GuideManagerV2.Stuff.IsExecutingForceGuide || GuideManagerV2.Stuff.IsExecutingTriggredGuide)
        {
            return;
        }

        this.TryGlobalBack();
    }



}

public enum UILayer
{
    /// <summary>
    /// 仅在参数中表示语义，使用默认值
    /// </summary>
    UseDefault,

    /// <summary>
    /// 显示页面所使用的层
    /// </summary>
    PageLayer,

    /// <summary>
    /// 普通 Floaitng 层，显示在所有 Page 前面
    /// </summary>
    NormalFloatingLayer,

    /// <summary>
    /// 教程 Floating 层，会显示在普通Floating上面
    /// </summary>
    GuideLayer,

    /// <summary>
    /// 全局阻塞层
    /// </summary>
    BlockLayer,

    /// <summary>
    /// 过场，遮盖任何全局 Floating
    /// </summary>
    TransitionLayer,

    /// <summary>
    /// 全局对话框层，对用户来说总是显示在最前面
    /// </summary>
    GlobalDialogLayer,

    /// <summary>
    /// 贴在屏幕上，用于显示调试信息
    /// </summary>
    ScreenLayer,
    
    /// <summary>
    /// 教程 Floating Block层，会显示在普通Floating上面
    /// </summary>
    GuideBlockLayer,
}

public enum NavigateOperation
{
    Forward,
    Back,
}