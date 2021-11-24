using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class UIEngine : ResourcesObject<UIEngine> 
{
    [HideInInspector]
    public PageStack pageStack = new PageStack ();
    GameObject root;

    //GameObject pageLayer;

    Dictionary<string, Floating> displayingFloatingDic = new Dictionary<string, Floating> ();

    public List<Floating> displayingFloatings{
        get{
            return new List<Floating>(displayingFloatingDic.Values);
        }
    }

    public PageContainer prefab_pageContainer;

    static public Action<Page> TopPageChanged;

    public Action<Floating, bool> FloatingDisplayChanged;
    public static Func<string, Task> BeforeForward;
    public static Action<Page> AfterForward;
    public static Action<string> RequestBlock;
    public static Action<string> RequestUnblock;
    public static Action<GameObject> VirginViewCreated;
    public static Func<string, GameObject> OnLoadPrefab;
    public static Func<string, Task<GameObject>> OnLoadPrefabAsync;

    public void Awake () 
    {
        root = this.gameObject;
        //root = GameObject.Find ("UIEngine.Canvas");
        //if (root == null)   
        //{
        //    var prefab = Resources.Load<GameObject> ("UIEngine.Canvas");
        //    root = GameObject.Instantiate (prefab);
        //}
        //root.name = "UIEngine.Canvas";
        GameObject.DontDestroyOnLoad (root);

        //pageLayer = root.transform.Find ("PageLayer").gameObject;

        //top = GameObject.Find("UIEngine.Top");

        //Canvas canvas = root.GetComponent<Canvas> ();
        //canvas.renderMode = RenderMode.ScreenSpaceCamera;
        //canvas.worldCamera = GameObject.Find("UICamera").GetComponent<Camera>();
        // if(hasDefaultPage)
        // {
        //     Debug.Log($"[{nameof(UIEngine)}] forward default page: {defaultPage}");
        //     this.Forward(defaultPage);
        // }
        //GameObject.DontDestroyOnLoad (this.gameObject);
    }


    private Canvas _canvas;
    public Canvas Canvas 
    {
        get 
        {
            if (_canvas == null) 
            {
                _canvas = root.GetComponent<Canvas> ();
            }
            return _canvas;

        }
    }

    private RectTransform _canvasTransform;
    public RectTransform CanvasTransform 
    {
        get 
        {
            if (_canvasTransform == null) 
            {
                _canvasTransform = Canvas.GetComponent<RectTransform> ();
            }
            return _canvasTransform;

        }
    }

    PageContainer ReusePageContainer()
    {
        var container = GameObjectPool.Stuff.Reuse<PageContainer>(this.prefab_pageContainer.gameObject);
        return container;
    }

    private async Task<Page> TakeOrCreatePageAsync (string pageName) 
    {
        // 尝试复用一个 page 对像
        var prefabName = $"{pageName}.prefab";

        //var prefab = await EasyAssetManager.Stuff.LoadAssetInBackgroundThreadAsync<GameObject>(prefabName);
        var prefab = await this.LoadPrefabAsync(prefabName);
        var page = GameObjectPool.Stuff.Reuse<Page>(prefab);

        if(page.IsVirgin){
            //LocalizationExtension.PrefabToLocalFont(page.GameObject);
            VirginViewCreated?.Invoke(page.GameObject);
        }

        //var containerPrefabName = $"PageContainer.prefab";
        //var containerPrefab = this.LoadPrefab(containerPrefabName);
        var container = this.ReusePageContainer();

        page.GameObject.name = pageName;

        var parent = LayerToTransform(UILayer.PageLayer);

        container.name = "PageContainer - " + pageName;

        container.transform.SetParent(parent.transform, false);
        container.Page = page;
        page.ContainerActive = false;
        page.Active = false;
        // page.transform.localScale = Vector2.one;
        // page.rectTransform.offsetMin = Vector2.zero;
        // page.rectTransform.offsetMax = Vector2.zero;
        return page;
    }

    public async Task<Page> ReplaceAsync (string pageName, object param = null) 
    {
        var hasPage = pageStack.Find (pageName);
        if (hasPage != null) 
        {
            throw new Exception ("page: " + pageName + " already in stack, can't replace, try use Backto");
        }
        // var fromPage = pageStack.Peek ();
        var page = await TakeOrCreatePageAsync(pageName);

        var oldPage = pageStack.Peek();

        PageNavigateInfo naviInof = new PageNavigateInfo();
        naviInof.param = param;
        naviInof.isBack = false;
        naviInof.lastPageType = oldPage.GetType();
        await page.OnPreper(naviInof);

        await pageStack.RelpaceTopAsync(page, param);

        RepositionMask ();
        pageStack.Log("Replace");
        return page;
    }

    public async void Forward (string pageName, object param = null) 
    {
        await ForwardAsync(pageName, param);
    }


    public async Task<Page> ForwardAsync (string pageName, object param = null) 
    {
        var blockKey = $"UIEngine.ForwardAsync: {pageName}";
        try
        {
            //BlockManager.Show(blockKey, BlockLevel.Network);
            RequestBlock?.Invoke(blockKey);
            if(BeforeForward != null){
                await BeforeForward.Invoke(pageName);
            }
            var oldPage = pageStack.Find (pageName);
            if (oldPage != null) 
            {
                throw new Exception ("page: " + pageName + " already in stack, can't navigate, try use BackTo");
            }
            var fromPage = pageStack.Peek ();
            var page = await TakeOrCreatePageAsync(pageName);
            
            var from = pageStack.Peek();
            PageNavigateInfo naviInof = new PageNavigateInfo();
            naviInof.param = param;
            naviInof.isBack = false;
            if(from != null){
                naviInof.lastPageType = from.GetType();
            }
            else{
                naviInof.lastPageType = null;
            }
            await page.OnPreper(naviInof);

            //page.Active = true;
            pageStack.Push (page, param);
            RepositionMask ();
            pageStack.Log("Forward");
            return page;
        }
        finally
        {
            AfterForward?.Invoke(pageStack.Peek());
            //BlockManager.Hide(blockKey);
            RequestUnblock?.Invoke(blockKey);
        }
    }

    public async Task RemoveFromStackAsync(string name)
    {
        if (Top == null) 
        {
            return;
        }
        if (Top.GameObject.name == name) 
        {
            BackInBackground ();
        } 
        else 
        {
            var page = await pageStack.RemoveAsync(name);
            if (page != null) 
            {
                //pagePool.Put (name, page);
                page.RecyclePageAndContainer();
            }
            pageStack.Log("RemoveFromStack");
        }
       
    }

    public Page FindPage (string name)
    {
        var targetPage = pageStack.Find (name);
        return targetPage;
    }

    public async Task<Page> BackToAsync(string name, object param = null)
    {
        var targetPage = pageStack.Find (name);
        if (targetPage == null)
        {
            // page not in stack, can't pop to
            return null;
        }
        var popedList = await pageStack.PopUtilAsync(targetPage, param);

        foreach (var p in popedList) 
        {
            p.Poped?.Invoke();
            p.RecyclePageAndContainer();
        }
        RepositionMask ();
        pageStack.Log("BackTo");
        return targetPage;
    }

    public async void Back()
    {
        await BackAsync();
    }

    public async Task BackAsync () 
    {
        //BlockManager.Show("UIEngine.Back");
        var blockKey = "UIEngine.Back";
        RequestBlock?.Invoke(blockKey);
        try
        {
            var page = await pageStack.PopAsync();

            if (page != null)
            {
                page.Poped?.Invoke();
                page.RecyclePageAndContainer();
            }
            if (pageStack.Count == 0)
            {
                Debug.Log("All pages poped!");
            }
            RepositionMask();
            pageStack.Log("Back");

        }
        finally
        {
            //BlockManager.Hide("UIEngine.Back");
            RequestUnblock?.Invoke(blockKey);
        }
    }

    public async void BackInBackground () 
    {
        await BackAsync();
    }

    public Page Top 
    {
        get 
        {
            return pageStack.Peek ();
        }
    }

    public bool IsTop(string name)
    {
        var top = this.Top;
        var topName = top.Prefab.name;
        if(name == topName)
        {
            return true;
        }
        return false;
    }

    public int PagesCount 
    {
        get 
        {
            return pageStack.Count;
        }
    }

    public Floating FindFloating (string name) 
    {
        Floating ret = null;
        displayingFloatingDic.TryGetValue(name, out ret);
        return ret;
    }

    public bool IsFloatingExists<T> () 
    {
        var name = typeof(T).Name;
        var b = IsFloatingExists(name);
        return b;
    }

    public bool IsFloatingExists (string name) 
    {
        var floating = FindFloating(name);
        if(floating != null)
        {
            return true;
        }
        return false;
    }

    Dictionary<UILayer, GameObject> layerToRootDic = new Dictionary<UILayer, GameObject>();

    GameObject LayerToTransform(UILayer layer)
    {
        var root = DictionaryUtil.TryGet(layerToRootDic, layer, null);
        if(root != null)
        {
            return root;
        }
        var childName = layer.ToString();
        var child = this.root.transform.Find(childName);
        layerToRootDic[layer] = child.gameObject;
        return child.gameObject;
    }

    public Floating ShowFloatingIfNotShown(string name, Page containerPage = null, UILayer layer = UILayer.FloatingLayer)
    {
        var old = this.FindFloating(name);
        if (old != null)
        {
            return old;
        }
        var floating = ShowFloating(name, containerPage, layer);
        return floating;
    }


    GameObject LoadPrefab(string prefabName)
    {

        if (OnLoadPrefab == null)
        {
            throw new Exception("[UIEngien] OnLoadPrefab handler not set yet!");
        }

        var prefab = OnLoadPrefab.Invoke(prefabName);

        if (prefab == null)
        {
            throw new Exception($"[UIEngien] OnLoadPrefab {prefabName} get null");
        }

        return prefab;


    }

    async Task<GameObject> LoadPrefabAsync(string prefabName)
    {
        if (OnLoadPrefabAsync == null)
        {
            throw new Exception("[UIEngien] OnLoadPrefabAsync handler not set yet!");
        }

        var task = OnLoadPrefabAsync.Invoke(prefabName);
        var prefab = await task;

        if(prefab == null)
        {
            throw new Exception($"[UIEngien] OnLoadPrefabAsync {prefabName} get null");
        }
        return prefab;
    }

    public Floating ShowFloating (string name, Page containerPage = null, UILayer layer = UILayer.FloatingLayer, object param = null) 
    {
        Debug.Log($"[UIEngine] Show Floating {name}");
        // 如果已经在显示了，线隐藏再重新显示
        var old = this.FindFloating(name);
        if(old != null)
        {
            RemoveFloating(name);
        }


        var prefabName = $"{name}.prefab";
        var prefab = LoadPrefab(prefabName);
        var floating = GameObjectPool.Stuff.Reuse<Floating>(prefab, false, typeof(Floating));
        if(floating == null)
        {
            throw new Exception($"[UIEngine] can't create floaitng from prefab Floatings/{name}");
        }

        if(floating.IsVirgin){
            //LocalizationExtension.PrefabToLocalFont(floating.GameObject);
            VirginViewCreated?.Invoke(floating.GameObject);
        }

        GameObject root = null;
        if(containerPage == null)
        {
            root = this.LayerToTransform(layer);
        }
        else
        {
            root = containerPage.PageContainer.floatingRoot.gameObject;
        }

        floating.GameObject.name = name;
        floating.Transform.SetParent (root.transform, false);

        displayingFloatingDic[name] = floating;
    
        floating.OnShow(param);
        FloatingDisplayChanged?.Invoke(floating, true);
        floating.GameObject.SetActive(true);
        //floating.OnAfterShow();
        return floating;
    }

    public void RemoveFloating (string name) 
    {
        // Debug.Log($"[UIEngine] Remove Floating {name}");
        Floating floating;
        displayingFloatingDic.TryGetValue(name, out floating);
        if(floating == null)
        {
            // 并没有在显示，什么都不干
            return;
        }

        displayingFloatingDic.Remove(name);
        floating.Recycle();
        floating.OnHide();

        FloatingDisplayChanged?.Invoke(floating, false);
    }
   


    public void RepositionMask () 
    {
        // var firstPage = pageStack.Peek ();
        // if (firstPage != null && firstPage.Overlay) 
        // {
        //     var control = ShowFloating ("MaskFloating", null, UILayer.Middle);
           
        //     if (control != null) 
        //     {
        //         control.transform.SetParent(null,false);
        //         control.transform.SetParent (firstPage.transform.parent, false);
        //         var index = firstPage.transform.GetSiblingIndex ();
        //         control.transform.SetSiblingIndex (index);
        //     }
        // } 
        // else 
        // {
        //     HideFloating ("MaskFloating");
        // }
    }

    public async Task<Page> ForwardOrBackToAsync (string name, object param = null)
    {
        var oldPage = FindPage (name);
        if (oldPage != null) 
        {
            var page = await BackToAsync(name, param);
            return page;
        } 
        else 
        {
            var page = await ForwardAsync (name, param);
            return page;
        }
    }

    public void GlobleBack () 
    {
        var top = UIEngine.Stuff.Top;
        var dealed = top.OnGlobleBack ();
        if (!dealed) 
        {
            UIEngine.Stuff.BackInBackground ();
        }
    }

   
}

public enum UILayer
{
    /// <summary>
    /// 显示页面所使用的层
    /// </summary>
    PageLayer,

    /// <summary>
    /// 普通 Floaitng 层，显示在所有 Page 前面，转场在这一层
    /// </summary>
    FloatingLayer,

    /// <summary>
    /// 教程 Floating 层，会显示在其他Floating上面
    /// </summary>
    GuideLayer,

    /// <summary>
    /// 过场，遮盖任何全局 Floating
    /// </summary>
    TransitionLayer,

    /// <summary>
    /// 对话框层，对用户来说总是显示在最前面
    /// </summary>
    DialogLayer,

    /// <summary>
    /// 贴在屏幕上，用于显示调试信息
    /// </summary>
    ScreenLayer,
}