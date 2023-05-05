using System.Collections;
using UnityEngine;
using System.Threading.Tasks;
using System;
using Sirenix.OdinInspector;
using DG.Tweening;
using System.Collections.Generic;

public class Page : UIEngineElement
{
    public string pageTag;
    private Bucket bucket;

    public Bucket PageBucket
    {
        get
        {
            if (this.bucket == null)
            {
                this.bucket = BucketManager.Stuff.GetBucket(this.GetType().Name);
            }

            return this.bucket;
        }
    }


    bool _active;

    /// <summary>
    /// 用来代替 Unity 的原生事件 OnEnable / OnDisable
    /// 因为游戏对象的 Active 属性可能因为某些原因在一个 UIEngine 操作里会被反复设置多次，会多次触发底层事件
    /// 而此状态由 UIEngine 最终设置，以免除被影响
    /// </summary>
    public bool Active
    {
        get { return _active; }
        set
        {
            if (value == _active)
            {
                return;
            }

            _active = value;
            if (value)
            {
                this.OnPreActive();
                this.OnActive();
            }
            else
            {
                this.OnPreDeactive();
                this.OnDeactive();
            }
        }
    }


    [NonSerialized] public PageContainer container;


    [Tooltip("叠加窗口，使其后面的窗口也保持可见")] public bool Overlay;


    [Tooltip("遮罩是是透明的")] [ShowIf(nameof(Overlay))]
    public bool maskIsTranparent = false;

    [Tooltip("点击遮罩返回")] [ShowIf(nameof(Overlay))]
    public bool clickMaskToBack = true;


    public PageOptimization optimization = PageOptimization.OneTime;

    [Tooltip("语义：当此页面在最上时，禁止整体 block。此功能并不由 UIEngine 内部实现")]
    public bool noBlock = false;


    public void OnPreNavigatedTo(PageNavigateInfo info)
    {
        this.fragmentManager.OnPageNavigatedTo(info);
    }

    // 当被导航到这个窗口时发生
    public virtual void OnNavigatedTo(PageNavigateInfo navigateInfo)
    {
    }

    // 当从这个窗口导航到别的窗口时发生
    public virtual void OnNavigatedFrom(PageNavigateInfo navigateInfo)
    {
    }

    // 当被导航到这个窗口时发生
    public virtual void OnNavigatedToComplete(PageNavigateInfo navigateInfo)
    {
    }

    /// <summary>
    /// 为处理 wwise 事件额外添加，之后考虑整合
    /// </summary>
    void OnPreActive()
    {
        var pageName = this.name;
        //Debug.Log("[Page] OnPreActive:" + pageName);

        //WwiseEventManager.OnUiOpen(pageName);
        WwiseEventManager.SendEvent(TransformTable.UiOpen, pageName);
    }

    /// <summary>
    /// 为处理 wwise 事件额外添加，之后考虑整合
    /// </summary>
    void OnPreDeactive()
    {
        var pageName = this.name;

        //WwiseEventManager.OnUiClose(pageName);
        WwiseEventManager.SendEvent(TransformTable.UiClose, pageName);
    }

    public override void OnEnter()
    {
        base.OnEnter();

        if (Overlay)
        {
            WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_smallWindowOpen");
        }
        else
        {
            WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_windowOpen");
        }
    }

    public override void OnExit()
    {
        base.OnExit();

        if (Overlay)
        {
            WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_smallWindowClose");
        }
        else
        {
            WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_windowClose");
        }
    }

    public virtual void OnPush()
    {
    }

    public virtual void OnPop()
    {
    }

    public virtual async Task OnForwardToPreperAsync(PageNavigateInfo info)
    {
    }

    public virtual async Task OnNavigatedToPreperAsync(PageNavigateInfo info)
    {
    }

    public virtual async Task OnNavigatedFromPreperAsync(PageNavigateInfo info)
    {
    }

    bool ShouldRecycle
    {
        get
        {
            var ret = this.optimization == PageOptimization.None;
            return ret;
        }
    }

    public void RecyclePageAndContainer()
    {
        this.Active = false;
        var shouldRecycle = this.ShouldRecycle;
        if (shouldRecycle)
        {
            this.Recycle();
        }
        else
        {
            this.PageBucket.ReleaseAll();
            this.container.DestoryPage();
            /*
            var pageName = this.gameObject.name;
            Debug.LogError($"{pageName} is oneTime, not recyle");
            GameObject.Destroy(this.gameObject);
            MonoBehaviour.Destroy(this);
            BuketManager.Stuff.Main.Release($"{this.name}.prefab");
            */
        }

        this.container.Recycle();
        this.container.ReleaseAllFloatings();
    }


    public virtual void OnForwardTo(PageNavigateInfo info)
    {
    }

    public virtual void OnBackTo(PageNavigateInfo info)
    {
    }


    protected virtual void OnActive()
    {
    }

    protected virtual void OnDeactive()
    {
    }

    public virtual void OnButton(string msg)
    {
    }

    public void OnBack()
    {
        UIEngine.Stuff.Back();
    }

    public virtual async Task OnDismissAsync()
    {
        //var tcs = new TaskCompletionSource<bool>();
        //tcs.SetResult(true);
        //return tcs.Task;

        // xinwusai
        // 带有uispine界面的使用DoFade会出现白影
        var group = this.transform.gameObject.GetOrAddComponent<CanvasGroup>();
        group.DOFade(0, 0.2f);
        await Task.Delay(200);
        if (group != null)
        {
            group.alpha = 1f;
        }
    }


    protected override async Task LogicBackAsync()
    {
        await UIEngine.Stuff.BackAsync();
    }


    public virtual void OnAnimatorEvent(string eventName)
    {
        Debug.Log($"{this.name} receive animator event : {eventName}");
        this.InvokeOnetimeListner(eventName);
    }

    public Task WaitEventAsync(string eventName)
    {
        var tcs = new TaskCompletionSource<bool>();
        RegisterOnetimeListner(eventName, () => { tcs.SetResult(true); });
        return tcs.Task;
    }

    public void RegisterOnetimeListner(string eventName, Action action)
    {
        var list = DictionaryUtil.GetOrCreateList(eventNameToOncetimeListnerDic, eventName);
        list.Add(action);
    }


    static Dictionary<string, List<Action>> eventNameToOncetimeListnerDic = new Dictionary<string, List<Action>>();

    public void InvokeOnetimeListner(string pageName)
    {
        var b = eventNameToOncetimeListnerDic.TryGetValue(pageName, out var list);
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
}