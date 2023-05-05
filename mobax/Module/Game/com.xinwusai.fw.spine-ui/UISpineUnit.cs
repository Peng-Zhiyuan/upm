using System;
using System.Threading.Tasks;
using DG.Tweening;
using JetBrains.Annotations;
using SpineRegulate;
using UnityEngine;

public partial class UISpineUnit : MonoBehaviour
{
    private Bucket Bucket => BucketManager.Stuff.GetBucket(UIEngine.LatestNavigatePageName); // BuketManager.Stuff.Tool;

    private SpineRegulateAppointScriptObject _transAppointData;
    private SpineRegulateTemplateScriptObject _transTemplateData;
    public bool shareMaterial = true;
    public bool isNeedTransPageAsset = true;

    public ESpineTemplateType TemplateType => null == _transTemplateData ? default : _transTemplateData.templateType;

    public void Destroy()
    {
        SkeletonGraphic.skeletonDataAsset = null;
        SkeletonGraphic.Initialize(true);
    }

    private void Awake()
    {
        this.InitLocalPosition();
        this.InitMaterial();
    }

    public void ResetTrans()
    {
        this.rectTransform().localPosition = Vector3.zero;
        this.rectTransform().localScale = Vector3.one;
    }

    public async void OnEnable()
    {
    }

    void InitMaterial()
    {
        if (shareMaterial) return;
        SkeletonGraphic.material = new Material(SkeletonGraphic.material);
    }

    async void InitLocalPosition()
    {
        var page = this.transform.GetComponentInParent<Page>();
        if (this.Bucket == null)
        {
            return;
        }

        if (page == null) return;
        string pathName = page.name.Replace("(Clone)", "");
        var posData =
            await this.Bucket.GetOrAquireAsync<SpineRegulateUIPosScriptObject>($"Trans_{pathName}.asset", true);
        if (posData == null)
        {
            Debug.LogWarning("not find:" + $"Trans_{page.name}.asset");
            // 尝试找当前unSpineUnit组件名去找
            return;
        }

        // 如果await完后，就被销毁了 （比如说，迅速切页，可能就会导致这个问题）， 那么就要判断this为空
        if (null == this) return;
        if (!isNeedTransPageAsset)
        {
            return;
        }

        this.rectTransform().localPosition = new Vector3(posData.offsetX, posData.offsetY);
        this.rectTransform().localScale = Vector3.one * posData.scale;
    }

    /// <summary>
    /// 绑定指定类型  不推荐
    /// </summary>
    /// <param name="pageName"></param>
    /// <param name="heroId"></param>
    /// <param name="actionName"></param>
    /// <param name="delay"></param>
    /// <param name="materialName"></param>
    // [Obsolete]
    public async Task Bind(string pageName, int heroId, string actionName = "idle", float delay = 0,
        string materialName = "")
    {
        var heroRow = StaticData.HeroTable.TryGet(heroId);
        if (heroRow == null)
        {
            Debug.LogWarning($"[Error] : HeroTable Can Not Find HeroId ----> {heroId}");
            return;
        }

        if (string.IsNullOrEmpty(heroRow.heroLive))
        {
            Debug.LogWarning($"[Error] : HeroTable Can Not Find HeroLive-> ----> {heroId}");
            this.SetActive(false);
            return;
        }

        var address = $"{heroRow.heroLive}.asset";
        var skeletonGraphic = this.SkeletonGraphic;
        await this.InitAppointData(pageName, heroRow.heroLive);
        if (this == null) return;
        this.RefreshAppointTrans();

        var material = await GetMaterial(materialName);
        UiUtil.SetSkeletonInBackground(skeletonGraphic, () => address, null, actionName, delay, material: material);
    }

    /// <summary>
    /// 使用模板数据，推荐类型
    /// </summary>
    /// <param name="heroId"></param>
    /// <param name="type"></param>
    /// <param name="actionName"></param>
    /// <param name="delay"></param>
    /// <param name="needResetTrans">是否修改trans</param>
    /// <param name="needRevertX"></param>
    public async Task Set(int heroId, ESpineTemplateType type = ESpineTemplateType.Model, string actionName = "idle",
        float delay = 0, bool needResetTrans = true, bool needRevertX = false)
    {
        var heroRow = StaticData.HeroTable.TryGet(heroId);
        if (heroRow == null)
        {
            Debug.LogWarning($"[Error] : HeroTable Can Not Find HeroId ----> {heroId}");
            return;
        }

        if (string.IsNullOrEmpty(heroRow.heroLive))
        {
            Debug.LogWarning($"[Error] : HeroTable Can Not Find HeroLive-> ----> {heroId}");
            this.SetActive(false);
            return;
        }

        await Bind(heroRow.heroLive, type, actionName, delay, needRevertX: needRevertX);
    }

    public async Task Bind(string spineName, ESpineTemplateType type = ESpineTemplateType.Model,
        string actionName = "idle", float delay = 0, bool needResetTrans = true, bool needRevertX = false)
    {
        var address = $"{spineName}.asset";
        var skeletonGraphic = this.SkeletonGraphic;

        await InitTemplateData(spineName, type);
        if (needResetTrans)
        {
            RefreshTemplateTrans(type, needRevertX);
        }

        UiUtil.SetSkeletonInBackground(skeletonGraphic, () => address, null, actionName, delay);
        this.SetActive(true);
    }

    [ItemCanBeNull]
    async Task<Material> GetMaterial(string materialName)
    {
        Material material = null;
        if (string.IsNullOrEmpty(materialName))
        {
            material = await this.Bucket.GetOrAquireAsync<Material>("SkeletonGraphicDefaultMask.mat");
        }
        else
        {
            material = await this.Bucket.GetOrAquireAsync<Material>($"{materialName}.mat");
        }

        return material;
    }

    /// <summary>
    /// 切换动画管理
    /// </summary>
    /// <param name="heroId"></param>
    /// <param name="type"></param>
    /// <param name="easeDuration"></param>
    public async void ChangeTypeAndPlayAnimation(int heroId, ESpineTemplateType type = ESpineTemplateType.Model,
        float easeDuration = 0)
    {
        var heroRow = StaticData.HeroTable.TryGet(heroId);
        if (heroRow == null)
        {
            Debug.LogWarning($"[Error] : HeroTable Can Not Find HeroId ----> {heroId}");
            return;
        }

        var spineName = heroRow.heroLive;
    }

    public async void ChangeTypeAndPlayAnimation(ESpineTemplateType type = ESpineTemplateType.Model,
        float easeDuration = 0)
    {
        var spineName = _transTemplateData.spineName;
        var newTemplateData =
            await this.Bucket.GetOrAquireAsync<SpineRegulateTemplateScriptObject>(this.GetAddressName(spineName, type),
                true);
        var newPos = new Vector3(newTemplateData.offsetX, newTemplateData.offsetY, 0);
        var newScale = Vector3.one * newTemplateData.scale;
        if (easeDuration <= 0)
        {
            this.SkeletonGraphic.rectTransform.localPosition = newPos;
            this.SkeletonGraphic.rectTransform.localScale = newScale;
        }
        else
        {
            this.SkeletonGraphic.rectTransform.DOLocalMove(newPos, easeDuration);
            this.SkeletonGraphic.rectTransform.DOScale(newScale, easeDuration);
        }

        _transTemplateData = newTemplateData;
    }

    public void ShowAnimation(string anim, bool loop = false)
    {
        SkeletonGraphic.AnimationState.SetAnimation(0, anim, loop);
        if (!loop)
        {
            SkeletonGraphic.AnimationState.AddAnimation(0, "idle", true, 0);
        }
    }

    public void ShowIdle()
    {
        ShowAnimation("idle", true);
    }

    private string GetAddressName(string spineName, ESpineTemplateType type)
    {
        var addressName = "";
        if (type == ESpineTemplateType.Model)
        {
            addressName = $"Model_{spineName}.asset";
        }
        else if (type == ESpineTemplateType.HalfModel)
        {
            addressName = $"HalfModel_{spineName}.asset";
        }
        else if (type == ESpineTemplateType.FeatureCamera)
        {
            addressName = $"FeatureCamera_{spineName}.asset";
        }

        return addressName;
    }

    private async Task InitTemplateData(string spineName, ESpineTemplateType type)
    {
        this._transTemplateData =
            await this.Bucket.GetOrAquireAsync<SpineRegulateTemplateScriptObject>(this.GetAddressName(spineName, type),
                true);
    }

    private async Task InitAppointData(string pageName, string spineName)
    {
        if (this == null) return;
        this._transAppointData =
            await this.Bucket.GetOrAquireAsync<SpineRegulateAppointScriptObject>($"{pageName}_{spineName}.asset", true);
    }

    private void RefreshTemplateTrans(ESpineTemplateType type, bool needRevertX = false)
    {
        var transData = this._transTemplateData;
        if (transData == null)
        {
            this.SkeletonGraphic.rectTransform.localPosition = Vector3.zero;
            this.SkeletonGraphic.rectTransform.localScale = Vector3.one;
            return;
        }

        this.SkeletonGraphic.rectTransform.localPosition = new Vector3(transData.offsetX, transData.offsetY, 0);
        if (needRevertX)
        {
            this.SkeletonGraphic.rectTransform.localScale = new Vector3(-1, 1, 1) * transData.scale;
        }
        else
        {
            this.SkeletonGraphic.rectTransform.localScale = Vector3.one * transData.scale;
        }

        if (type == ESpineTemplateType.HalfModel
            && transData.openMask)
        {
            this.Mask.rectTransform.sizeDelta = transData.maskSize;
            this.Mask.softness = transData.softness;
        }
        else
        {
            this.Mask.rectTransform.sizeDelta = new Vector2(1080, 1920) * 2;
            this.Mask.softness = Vector2Int.zero;
        }
    }

    /// <summary>
    /// 更新spine位置
    /// </summary>
    private async void RefreshAppointTrans()
    {
        var transData = this._transAppointData;
        if (transData == null)
        {
            Debug.LogError("立绘坐标数据不存在");
            this.SkeletonGraphic.rectTransform.localPosition = Vector3.zero;
            this.SkeletonGraphic.rectTransform.localScale = Vector3.one;
            this.Mask.rectTransform.sizeDelta = new Vector2(1080, 1920);
            this.Mask.softness = Vector2Int.zero;
            return;
        }

        if (this == null
            || this.SkeletonGraphic == null) return;
        this.SkeletonGraphic.rectTransform.localPosition = new Vector3(transData.offsetX, transData.offsetY, 0);
        this.SkeletonGraphic.rectTransform.localScale = Vector3.one * transData.scale;
        if (transData.openMask)
        {
            this.Mask.rectTransform.sizeDelta = transData.maskSize;
            this.Mask.softness = transData.softness;
        }
        else
        {
            this.Mask.rectTransform.sizeDelta = new Vector2(1080, 1920) * 2;
            // this.Mask.rectTransform.sizeDelta =  new Vector2(1080, 1920);
            this.Mask.softness = Vector2Int.zero;
        }
    }

    public void RefreshSkeletonGraphicTrans(Vector3 position, Vector3 size)
    {
        this.SkeletonGraphic.rectTransform.localPosition = position;
        this.SkeletonGraphic.rectTransform.localScale = size;
    }
}