using System.Threading.Tasks;
using UnityEngine;
using Sirenix.OdinInspector;

public enum LightSpace
{
    Local,
    Woreld
}

public enum SKIN_TYPE
{
    HAIR,
    FACE,
    BODY,
    CLOTH,
    BROW,
}

[ExecuteAlways]
public class RoleRender : IRoleRender
{
    public static Color defaultLightColor = new Color(1, 1, 1, 0);
    public static Color defaultHDRColor = Color.yellow * 1.5f;
    private AttachBone attachBone;
    private GameObject selfGameObject;

    void Awake()
    {
        selfGameObject = this.gameObject;
        logicRoot = InitTrans("logicRoot");
        //absRoot = InitTrans("absRoot");
        // this.transform.Get2AddComponent<RoleRenderEffect>();
        attachBone = this.transform.GetComponent<AttachBone>();
        if (attachBone != null)
        {
            attachBone.Init();
        }
        this.RefreshRenderList();
#if UNITY_EDITOR
        var animator = this.transform.GetComponent<Animator>();
        bindAnimator = (animator == null);
#endif
        //this.Init();
    }

    private void OnEnable()
    {
        RefreshRender();
    }

    bool rendering = false;

    public async Task RefreshRender()
    {
        if (rendering) return;
        rendering = true;
        if (this.gameObject.name.Contains("_low")
            && DeveloperLocalSettings.GraphicQuality != DeveloperLocalSettings.Quality.High)
        {
            this.UseLowQuality = true;
            this.UseDither = false;
        }
        else
        {
            this.UseLowQuality = false;
            this.UseDither = false;
        }
        if (DeveloperLocalSettings.GraphicQuality != DeveloperLocalSettings.Quality.High)
        {
            this.RenderShadow(false);
            await this.RefreshSimpleShadow(true, "FX_UI_Indicator_Shadow.prefab");
        }
        else
        {
            this.RenderShadow(true);
            if (mSimpleShadow != null)
            {
                mSimpleShadow.SetActive(false);
            }
        }
        rendering = false;
    }

#if UNITY_EDITOR
    private bool bindAnimator = false;
#endif
    public override void Init()
    {
        ResetMaterials();
        ResetProperties();
    }

    Transform InitTrans(string n)
    {
        var trans = transform.Find(n);
        if (trans == null)
        {
            var go = new GameObject(n);
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.AngleAxis(180, Vector3.up);
            trans = go.transform;
        }
        return trans;
    }

    public async void SwitchFocusArea(bool _switch, Transform head)
    {
        Debug.LogWarning("SwitchFocusArea:" + _switch);
        if (_switch == showFocusArea)
            return;
        showFocusArea = _switch;
        if (_switch)
        {
            await ShowFocusAreaTrans(true, "FX_UI_Indicator_Blue90_In.prefab");
        }
        else
        {
            ShowFocusAreaTrans(false, "FX_UI_Indicator_Blue90_Out.prefab");
        }
        if (!mArea)
        {
            var obj = await GameObjectPoolUtil.ReuseAddressableObjectAsync<RecycledGameObject>(BucketManager.Stuff.Battle, "FX_UI_Indicator_Blue90_Loop.prefab");
            mArea = obj.gameObject;
            if (logicRoot != null)
            {
                mArea.transform.parent = logicRoot;
                mArea.transform.localScale = Vector3.one * 1.3f;
                mArea.transform.localPosition = Vector3.zero;
                mArea.transform.localRotation = Quaternion.identity;
            }
        }
        /*if (!mHeadCircle)
        {
            var obj = await GameObjectPoolUtil.ReuseAddressableObjectAsync<RecycledGameObject>( "FX_UI_Indicator_Blue90_Loop_3.prefab");
            mHeadCircle = obj.gameObject;
            if (logicRoot != null)
            {
                mHeadCircle.transform.parent = logicRoot;
                mHeadCircle.transform.localScale = Vector3.one;
                mHeadCircle.transform.localPosition = new Vector3(-1.2f, 0, 0);
                mHeadCircle.transform.localRotation = Quaternion.identity;
            }

            if (HeadBone == null)
            {
                HeadBone = head;
            }
        }
        
        mHeadCircle.SetActive(showFocusArea);
*/
        mArea.SetActive(showFocusArea);
    }

    public async void ShowHeadTag(bool _switch, Transform head)
    {
        if (_switch == showHeadTag)
            return;
        showHeadTag = _switch;
        if (!mHeadCircle)
        {
            var obj = await GameObjectPoolUtil.ReuseAddressableObjectAsync<RecycledGameObject>(BucketManager.Stuff.Battle, "FX_UI_Indicator_Blue90_Loop_3.prefab");
            mHeadCircle = obj.gameObject;
            if (logicRoot != null)
            {
                mHeadCircle.transform.parent = logicRoot;
                mHeadCircle.transform.localScale = Vector3.one;
                mHeadCircle.transform.localPosition = new Vector3(-1.2f, 0, 0);
                mHeadCircle.transform.localRotation = Quaternion.identity;
            }
            if (HeadBone == null)
            {
                HeadBone = head;
            }
        }
        mHeadCircle.SetActive(_switch);
    }

    private (int, int, int, long) cacheAvatarInfo = (0, 0, 0, 0);

    public async Task SwitchHeroSkin((int, int, int, long) avatarInfo)
    {
        cacheAvatarInfo = avatarInfo;
        this.ResetSkin();
        if (avatarInfo.Item2 > 0)
        {
            string key = StaticData.ClothColorTable[avatarInfo.Item2].Modl;
            await this.SwitchSkin(SKIN_TYPE.CLOTH, key.ToLower());
        }
        if (avatarInfo.Item3 > 0
            && avatarInfo.Item4 > Clock.ToTimestampS(Clock.Now))
        {
            string key = StaticData.HairColorTable[avatarInfo.Item3].Modl;
            await this.SwitchSkin(SKIN_TYPE.HAIR, key.ToLower());
            await this.SwitchSkin(SKIN_TYPE.BROW, key.ToLower(), true);
        }
    }

    public async Task ResetHeroSkin()
    {
        await this.SwitchHeroSkin(cacheAvatarInfo);
    }

    public void HideHeadTag()
    {
        ShowHeadTag(false, null);
    }

    public async void SwitchTargetSub(bool _switch, Transform head, float scale = 1f)
    {
        if (!mHeadSub)
        {
            mHeadSub = new GameObject("SubTag");
            if (logicRoot != null)
            {
                mHeadSub.transform.parent = logicRoot;
                mHeadSub.transform.localScale = Vector3.one;
                mHeadSub.transform.localPosition = Vector3.zero;
                mHeadSub.transform.localRotation = Quaternion.identity;
            }
            var obj = await GameObjectPoolUtil.ReuseAddressableObjectAsync<RecycledGameObject>(BucketManager.Stuff.Battle, "FX_UI_Indicator_Blue90_Loop_3_2.prefab");
            obj.gameObject.transform.parent = mHeadSub.transform;
            obj.gameObject.transform.localPosition = Vector3.zero;
            obj.gameObject.transform.localRotation = Quaternion.identity;
            if (HeadBone == null)
            {
                HeadBone = head;
            }
        }
        mHeadSub.SetActive(_switch);
        if (_switch)
        {
            mHeadSub.transform.localScale = Vector3.one * scale;
        }
    }

    private string GetOriginMatName(string matInstanceName)
    {
        int index = matInstanceName.IndexOf(" (");
        if (index > 0)
        {
            return matInstanceName.Substring(0, index);
        }
        return matInstanceName;
    }

    public async Task SwitchSkin(SKIN_TYPE skinType, string matKey, bool exactMatch = false)
    {
        string skinTag = null;
        switch (skinType)
        {
            case SKIN_TYPE.HAIR:
                skinTag = hairTag;
                break;
            case SKIN_TYPE.FACE:
                skinTag = faceTag;
                break;
            case SKIN_TYPE.BODY:
                skinTag = bodyTag;
                break;
            case SKIN_TYPE.CLOTH:
                skinTag = clothTag;
                break;
            case SKIN_TYPE.BROW:
                skinTag = browTag;
                break;
        }
        if (skinTag == null)
        {
            throw new GameException(ExceptionFlag.None, $"invalid skin type skin type", "ROLE_RENDER_SKIN_ERROR");
        }
        Bucket bucket = null;
        if (UIEngine.LatestNavigatePageName != null)
        {
            bucket = BucketManager.Stuff.GetBucket(UIEngine.LatestNavigatePageName);
        }
        else bucket = BucketManager.Stuff.Main;
        var sharedMaterials = this._renderDic[skinTag].GetMaterials();
        if (exactMatch)
        {
            for (int i = 0; i < sharedMaterials.Length; i++)
            {
                var mat = sharedMaterials[i];
                if (mat.name.Contains(skinTag))
                {
                    string matName = GetOriginMatName(mat.name);
                    string newMatName = $"{matName}_{matKey}.mat";
                    var new_mat = await bucket.GetOrAquireAsync<Material>(newMatName);
                    sharedMaterials[i] = new_mat;
                    break;
                }
            }
        }
        else
        {
            for (int i = 0; i < sharedMaterials.Length; i++)
            {
                var mat = sharedMaterials[i];
                string matName = GetOriginMatName(mat.name);
                string newMatName = $"{matName}_{matKey}.mat";
                //Debug.LogError("newMatName:"+ newMatName);
                var new_mat = await bucket.GetOrAquireAsync<Material>(newMatName);
                sharedMaterials[i] = new_mat;
            }
        }
        this._renderDic[skinTag].SetMaterials(sharedMaterials);
    }

    public void ResetSkinByType(SKIN_TYPE skinType)
    {
        string skinTag = null;
        switch (skinType)
        {
            case SKIN_TYPE.HAIR:
                skinTag = hairTag;
                break;
            case SKIN_TYPE.FACE:
                skinTag = faceTag;
                break;
            case SKIN_TYPE.BODY:
                skinTag = bodyTag;
                break;
            case SKIN_TYPE.CLOTH:
                skinTag = clothTag;
                break;
            case SKIN_TYPE.BROW:
                skinTag = faceTag;
                break;
            default: break;
        }
        if (skinTag == null)
        {
            throw new GameException(ExceptionFlag.None, $"invalid skin type skin type", "ROLE_RENDER_SKIN_ERROR");
        }
        if (!this._renderDic.ContainsKey(skinTag))
        {
            Debug.LogWarning("not find skin node:" + skinTag);
            return;
        }
        var render = this._renderDic[skinTag];
        if (render != null
            && this.cacheMats[render].Length > 0)
        {
            render.SetMaterials(this.cacheMats[render]);
        }
    }

    private void ResetSkin()
    {
        this.ResetSkinByType(SKIN_TYPE.HAIR);
        this.ResetSkinByType(SKIN_TYPE.CLOTH);
        this.ResetSkinByType(SKIN_TYPE.FACE);
        this.ResetSkinByType(SKIN_TYPE.BODY);
        this.ResetSkinByType(SKIN_TYPE.BROW);
    }

    private async Task RefreshSimpleShadow(bool show, string effect)
    {
        if (mSimpleShadow == null && show)
        {
            var obj = await GameObjectPoolUtil.ReuseAddressableObjectAsync<RecycledGameObject>(BucketManager.Stuff.Battle, effect);
            if (obj == null) return;
            mSimpleShadow = obj.gameObject;
            if (logicRoot != null)
            {
                mSimpleShadow.transform.parent = logicRoot;
                mSimpleShadow.transform.localScale = Vector3.one * 1.2f;
                mSimpleShadow.transform.localPosition = Vector3.zero;
                mSimpleShadow.transform.localRotation = Quaternion.identity;
            }
        }
        if (mSimpleShadow != null)
        {
            mSimpleShadow.SetActive(show);
        }
    }

    private async Task ShowFocusAreaTrans(bool bIn, string effect)
    {
        GameObject area = null;
        if (bIn)
        {
            await Task.Delay(100);
            if (mAreaIn == null)
            {
                var obj = await GameObjectPoolUtil.ReuseAddressableObjectAsync<RecycledGameObject>(BucketManager.Stuff.Battle, effect);
                mAreaIn = obj.gameObject;
            }
            area = mAreaIn;
            SetCustomLightAtten(2);
        }
        else
        {
            if (mAreaOut == null)
            {
                var obj = await GameObjectPoolUtil.ReuseAddressableObjectAsync<RecycledGameObject>(BucketManager.Stuff.Battle, effect);
                mAreaOut = obj.gameObject;
            }
            area = mAreaOut;
        }
        if (area == null)
            return;
        area.SetActive(true);
        if (logicRoot != null)
        {
            area.transform.parent = logicRoot;
            area.transform.localScale = Vector3.one * 1.3f;
            area.transform.localPosition = Vector3.zero;
            area.transform.localRotation = Quaternion.identity;
        }
        await Task.Delay(500);
        if (bIn)
        {
            SetCustomLightAtten(0);
        }
        if (this != null) area.SetActive(false);
    }

    public void SetWeaponLayer(ELayerMask eLayer)
    {
        Transform tr = GetBoneTrans(this.attachBone.Part.gameObject.name);
        tr.gameObject.layer = (int)eLayer;
        LagacyUtil.SetRootLayer(tr.gameObject, eLayer);
    }

    public void DisolveWeapon()
    {
        this.PlayEffect(ROLE_EFFECT_TYPE.WEAPON_DISSOLVED);
    }

    public void ShowWeapon(bool show)
    {
        if (show)
        {
            if (attachBone != null)
            {
                Transform tr = GetBoneTrans(this.attachBone.Part.gameObject.name);
                tr.gameObject.SetActive(true);
                foreach (var trans in tr.GetComponentsInChildren<Transform>(true))
                {
                    trans.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            if (attachBone != null)
            {
                Transform tr = GetBoneTrans(this.attachBone.Part.gameObject.name);
                foreach (var trans in tr.GetComponentsInChildren<Transform>(true))
                {
                    trans.gameObject.SetActive(false);
                }
            }
        }
    }
    /*
    public float Hurt()
    {
        return PlayEffect(ROLE_EFFECT_TYPE.HURT, Vector3.zero);
        //StartCoroutine(HurtEffect());
    }
 

    public void ChangeColor(Color color, float time)
    {
        //if (Time.time - _changeColorTick < _CHANGE_COLOR_CD)
        //{
        //    return;
        //}
        //_changeColorTick = Time.time;
        //StartCoroutine(ChangeColorEffect(color,time));
    }
     */
    //private float _changeColorTick = 0f;
    //private float _CHANGE_COLOR_CD = 0.5f;

    private void RefreshProperties()
    {
        this.RefreshRenderList();
        UseFresnelEffect = this.useFresnelEffect;
        FresnelColor = this.fresnelColor;
        FresnelStart = this.fresnelStart;
        CustomLightAtten = this.customLightAtten;
        CustomLightColor = this.customLightColor;
        CustomLightSpace = this.customLightSpace;
        LightRotateY = this.lightRotateY;
        UseFaceMask = this.useFaceMask;
        UseFlowMap = this.useFlowMap;
        this.ApplyMPBlock();
        UseWeaponDissolveEffect = this.useWeaponDissolveEffect;
        WeaponDissolveColor = this.weaponDissolveColor;
        WeaponDissolveRatio = this.weaponDissolveRatio;
        WeaponDissolveRange = this.weaponDissolveRange;
        WeaponDissolveDir = this.weaponDissolveDir;
        this.ApplyMPBlock(RenderGroup.OnlyWeapon);
    }

    public void OnDidApplyAnimationProperties()
    {
        Debug.Log("OnDidApplyAnimationProperties");
        RefreshProperties();
    }

    //------------------------------------UseLowQuality--------------------------------------//
    [SerializeField, SetProperty("UseLowQuality")]
    //[SerializeField]
    private bool useLowQuality;
    //private bool useLowQualityCache;

    public bool UseLowQuality
    {
        set
        {
            //if (useLowQualityCache != value)
            //{
            SetMpFloat("_UseLowQuality", value ? 1 : 0);
            this.SwitchKeyword("_LOW_QUALITY", value);
            //}
            // useLowQualityCache = value;
            useLowQuality = value;
        }
    }

    //------------------------------------UseDither--------------------------------------//
    [SerializeField, SetProperty("UseDither")]
    //[SerializeField]
    private bool useDither;
    private bool useDitherCache;
    public bool UseDither
    {
        set
        {
            if (useDitherCache != value)
            {
                SetMpFloat("_UseDither", value ? 1 : 0);
                this.SwitchKeyword("_USE_DITHER", value);
            }
            this.useDither = value;
            this.useDitherCache = value;
        }
    }

    /*  public void SetWeaponFresnel(bool value, Color c, float fresnelStartVal = 0, bool force = false)
      {
          if (openFresnel == value
              && fresnelColor == c
              && fresnelStart == fresnelStartVal
              && !force) return;
          this.SwitchKeyword("_FRESNEL_EFFECT", value);
          SetMpColor("_FresnelColor", c);
          SetMpFloat("_FresnelStart", fresnelStartVal);
          openFresnel = value;
          fresnelColor = c;
          fresnelStart = fresnelStartVal;
      }*/

    //---------------------------------UseFresneEffect-------------------------------------//  
    [SerializeField]
    //[SerializeField, SetProperty("UseFresnelEffect")]
    private bool useFresnelEffect = false;
    //private bool useFresnelEffectCache;
    public bool UseFresnelEffect
    {
        set
        {
            //if (useFresnelEffectCache != value)
            {
                this.SwitchKeyword("_FRESNEL_EFFECT", value);
                SetMpFloat("_FresnelEffect", value ? 1 : 0);
            }
            this.useFresnelEffect = value;
            //this.useFresnelEffectCache = value;
        }
    }

    [SerializeField, ColorUsageAttribute(true, true)]
    //[SerializeField, SetProperty("FresnelColor")]
    private Color fresnelColor = defaultHDRColor;
    private Color fresnelColorCache;

    public Color FresnelColor
    {
        set
        {
            if (this.fresnelColorCache != value)
            {
                SetMpColor("_FresnelColor", value);
            }
            this.fresnelColor = value;
            this.fresnelColorCache = value;
        }
    }

    [SerializeField]
    private float fresnelStart = 0;
    private float fresnelStartCache;
    public float FresnelStart
    {
        set
        {
            if (this.fresnelStartCache != value)
            {
                SetMpFloat("_FresnelStart", value);
            }
            this.fresnelStart = value;
            this.fresnelStartCache = value;
        }
    }

    public void SetFresnel(bool useFresnelEffect, Color fresnelColor, float fresnelStart = 0)
    {
        UseFresnelEffect = useFresnelEffect;
        FresnelColor = fresnelColor;
        FresnelStart = fresnelStart;
        this.ApplyMPBlock();
    }

    public void SetBodyFresnel(bool useFresnelEffect, Color fresnelColor, float fresnelStart = 0)
    {
        this.SwitchKeyword("_FRESNEL_EFFECT", useFresnelEffect, RenderGroup.OnlyBody);
        SetMpFloat("_FresnelEffect", useFresnelEffect ? 1 : 0);
        FresnelColor = fresnelColor;
        FresnelStart = fresnelStart;
        this.ApplyMPBlock(RenderGroup.OnlyBody);
    }

    //---------------------------------CustomLightAtten-------------------------------------//
    //[SerializeField, SetProperty("CustomLightAtten")]
    [SerializeField]
    private float customLightAtten = 0;
    private float customLightAttenCache;
    public float CustomLightAtten
    {
        set
        {
            if (this.customLightAttenCache != value)
            {
                SetMpFloat("_CustomLightAtten", value);
            }
            this.customLightAtten = value;
            this.customLightAttenCache = value;
        }
    }

    public void SetCustomLightAtten(float customLightAtten)
    {
        CustomLightAtten = customLightAtten;
        this.ApplyMPBlock();
    }

    //---------------------------------CustomLightColor-------------------------------------//
    //[SerializeField, SetProperty("CustomLightColor")]
    [SerializeField]
    private Color customLightColor = defaultLightColor;
    private Color customLightColorCache;
    public Color CustomLightColor
    {
        set
        {
            if (this.customLightColorCache != value)
            {
                SetMpColor("_CustomLightColor", value);
            }
            this.customLightColor = value;
            this.customLightColorCache = value;
        }
    }

    public void SetCustomLightColor(Color customLightColor)
    {
        CustomLightColor = customLightColor;
        this.ApplyMPBlock();
    }

    //-----------------------------------UseDissolveEffect-----------------------------------//
    //[SerializeField, SetProperty("UseDissolveEffect")]
    [SerializeField]
    private bool useWeaponDissolveEffect = false;

    private bool useWeaponDissolveEffectCache;
    public bool UseWeaponDissolveEffect
    {
        set
        {
            if (useWeaponDissolveEffectCache != value)
            {
                this.SwitchKeyword("_DISSOLVE_EFFECT", value, RenderGroup.OnlyWeapon);
                SetMpFloat("_DissolveEffect", value ? 1 : 0);
            }
            this.useWeaponDissolveEffect = value;
            this.useWeaponDissolveEffectCache = value;
        }
    }

    //[ColorUsageAttribute(true, true)]
    //[SerializeField, SetProperty("DissolveColor")]
    [SerializeField, ColorUsageAttribute(true, true)]
    private Color weaponDissolveColor = defaultHDRColor;
    private Color weaponDissolveColorCache;
    public Color WeaponDissolveColor
    {
        set
        {
            if (this.weaponDissolveColorCache != value)
            {
                SetMpColor("_DissolveColor", value);
            }
            this.weaponDissolveColor = value;
            this.weaponDissolveColorCache = value;
        }
    }

    [SerializeField]
    private float weaponDissolveRange = 0.5f;
    private float weaponDissolveRangeCache;
    public float WeaponDissolveRange
    {
        set
        {
            if (this.weaponDissolveRangeCache != value)
            {
                SetMpFloat("_DissolveRange", value);
            }
            this.weaponDissolveRange = value;
            this.weaponDissolveRangeCache = value;
        }
    }

    /*

        [SerializeField, SetProperty("DissolveEffect")]
        private float dissolveEffect= 0;
        private float dissolveEffectCache;
        public float DissolveEffect
        {
            set
            {
                if (this.dissolveEffectCache != value)
                {
                    SetMpFloat("_DissolveEffect", value, true);
                }
                this.dissolveEffect = value;
                this.dissolveEffectCache = value;
            }
        }*/

    //[SerializeField, SetProperty("DissolveRatio")]
    [SerializeField]
    private float weaponDissolveRatio = 0;
    private float weaponDissolveRatioCache;
    public float WeaponDissolveRatio
    {
        set
        {
            if (this.weaponDissolveRatioCache != value)
            {
                SetMpFloat("_DissolveRatio", value);
            }
            this.weaponDissolveRatio = value;
            this.weaponDissolveRatioCache = value;
        }
    }

    //[SerializeField, SetProperty("DissolveDir")]
    [SerializeField]
    private float weaponDissolveDir = 1;
    private float weaponDissolveDirCache;
    public float WeaponDissolveDir
    {
        set
        {
            if (this.weaponDissolveDirCache != value)
            {
                SetMpFloat("_DissolveDir", value);
            }
            this.weaponDissolveDir = value;
            this.weaponDissolveDirCache = value;
        }
    }

    public void SetWeaponDissolve(float dissolveRatio, Color dissolveColor, float disolveRange = 0.5f, bool dissolveDir = true)
    {
        UseWeaponDissolveEffect = dissolveRatio > 0;
        WeaponDissolveColor = dissolveColor; // new Color32(255, 255, 0, 255);
        //DissolveEffect = dissolveEffect > 0? 1:0;
        WeaponDissolveRatio = dissolveRatio;
        WeaponDissolveDir = dissolveDir ? 1 : 0;
        WeaponDissolveRange = disolveRange;
        this.ApplyMPBlock();
    }

    //--------------------------------------------------------------------------------------//
    //[SerializeField, SetProperty("CustomLightSpace")]
    [SerializeField]
    private LightSpace customLightSpace = LightSpace.Local;
    private LightSpace customLightSpaceCache;
    public LightSpace CustomLightSpace
    {
        set
        {
            if (this.customLightSpaceCache != value)
            {
                SetCustomLightDir(value, lightRotateY);
            }
            this.customLightSpace = value;
            this.customLightSpaceCache = value;
        }
    }

    //[SerializeField, SetProperty("LightRotateY")]
    [SerializeField]
    private float lightRotateY = 0;
    private float lightRotateYCache;
    public float LightRotateY
    {
        set
        {
            if (this.lightRotateYCache != value)
            {
                SetCustomLightDir(customLightSpace, value);
            }
            this.lightRotateY = value;
            this.lightRotateYCache = value;
        }
    }

    /*[SerializeField, SetProperty("CustomLightDir")]
    private Vector4 customLightDir = Vector4.zero;
    private Vector4 customLightDirCache;
    public Vector4 CustomLightDir
    {
        set
        {
            if (this.customLightDirCache != value)
            {
                SetMpVector("_CustomLightDir", value);
            }
            this.customLightDir = value;
            this.customLightDirCache = value;
        }
    }*/

    private void SetCustomLightDir(LightSpace lightSpace, float lightRotateY)
    {
        Vector4 lightDirection;
        if (lightSpace == LightSpace.Local)
        {
            lightDirection = Quaternion.Euler(0, lightRotateY * 30, 0) * this.transform.right;
        }
        else
        {
            lightDirection = Quaternion.Euler(0, lightRotateY * 30, 0) * Vector3.right;
        }
        lightDirection.w = lightRotateY > 0 ? 1 : 0;
        SetMpVector("_CustomLightDir", lightDirection);
        this.ApplyMPBlock();
    }

    //--------------------------------------------------------------------------------------//
    /*
    private bool useMapCap = false;
    private Texture2D _matCapTex2d = null;
    private Color _matCapColor;
    private float _matCapPower;
    private float _matCapAlphaPower;
    private float _matCapMultiply;
    private float _matCapAdd;

    public void SetMatCap(bool value, Texture2D tex2d, Color matCapColor, float matCapPower, float matCapAlphaPower, float matCapMultiply, float matCapAdd, bool force = false)
    {
        if (useMapCap == value
            && _matCapTex2d == tex2d
            && _matCapColor == matCapColor
            && _matCapPower == matCapPower
            && _matCapAlphaPower == matCapAlphaPower
            && _matCapMultiply == matCapMultiply
            && _matCapAdd == matCapAdd
            && !force) return;
        SwitchKeyword("_USE_MATCAP", value);
        SetMpFloat("_UseMatcapEffect", value ? 1 : 0);
        if (tex2d != null) SetMpTexture("_MatCap", tex2d);
        SetMpColor("_MatCapColor", matCapColor);
        SetMpFloat("_MatCapPower", matCapPower);
        SetMpFloat("_MatCapAlphaPower", matCapAlphaPower);
        SetMpFloat("_MatCapMultiply", matCapMultiply);
        SetMpFloat("_MatCapAdd", matCapAdd);
        useMapCap = value;
    }
    */
    //---------------------------------------------UseFaceMask-----------------------------------------//
    //[SerializeField, SetProperty("UseFaceMask")]
    [SerializeField]
    private float useFaceMask = 0;
    private float useFaceMaskCache;
    public float UseFaceMask
    {
        set
        {
            if (useFaceMaskCache != value)
            {
                SetMpFloat("_UseMask", value);
            }
            this.useFaceMask = value;
            this.useFaceMaskCache = value;
        }
    }

    public void SetMask(float useFaceMask)
    {
        UseFaceMask = useFaceMask;
        this.ApplyMPBlock();
    }

    [ShowInInspector]
    public void ResetProperties()
    {
        this.RefreshRenderList();
        UseFresnelEffect = false;
        FresnelColor = defaultHDRColor;
        FresnelStart = 0;
        CustomLightAtten = 0;
        CustomLightColor = defaultLightColor;
        CustomLightSpace = LightSpace.Local;
        LightRotateY = 0;
        UseFaceMask = 0;
        UseFlowMap = false;
        UseWeaponDissolveEffect = false;
        WeaponDissolveColor = defaultHDRColor;
        WeaponDissolveRatio = 0;
        WeaponDissolveRange = 0.5f;
        WeaponDissolveDir = 1;
        ApplyMPBlock();
    }

    /*
        private bool eyeRotateSpecular = false;

        public void SetEyeRotateSpecular(bool value, bool force = false)
        {
            if (eyeRotateSpecular == value
                && !force) return;
            SetMpFloat("_RotateSpecular", value ? 1 : 0);
            eyeRotateSpecular = value;
        }

        private bool eyeQuiverSpecular = false;

        public void SetEyeQuiverSpecular(bool value, bool force = false)
        {
            if (eyeQuiverSpecular == value
                && !force) return;
            SetMpFloat("_QuiverSpecular", value ? 1 : 0);
            eyeQuiverSpecular = value;
        }

        private bool eyeTearfulSpecular = false;

        public void SetTearfulSpecular(bool value, bool force = false)
        {
            if (eyeTearfulSpecular == value
                && !force) return;
            SetMpFloat("_TearfulSpecular", value ? 1 : 0);
            eyeTearfulSpecular = value;
        }*/
    //---------------------------------------------UseFlowMap-----------------------------------------//
    //[SerializeField, SetProperty("UseFaceMask")]
    [SerializeField]
    private bool useFlowMap = false;
    private bool useFlowMapCache;
    public bool UseFlowMap
    {
        set
        {
            if (useFlowMapCache != value)
            {
                this.SwitchKeyword("_USE_FLOW_MAP", value);
            }
            this.useFlowMap = value;
            this.useFlowMapCache = value;
        }
    }

    public override Transform GetBoneTrans(string _name)
    {
        if (_name == "root"
            || _name == "Root")
        {
            return this.transform;
        }
        if (m_Bones.ContainsKey(_name))
        {
            return m_Bones[_name];
        }
        if (m_BoneSlots.ContainsKey(_name))
        {
            return m_BoneSlots[_name];
        }
        if (_boneDic.ContainsKey(_name))
        {
            return _boneDic[_name];
        }
        GameObject findTrans = this.transform.FindInChildren(_name);
        if (findTrans == null)
        {
            return null;
        }
        _boneDic.Add(_name, findTrans.transform);
        return findTrans.transform;
    }

    //private bool once = true;
    private const string _FaceFront = "_FaceFront";
    private const string _FaceUp = "_FaceUp";
    private const string _RoleFront = "_RoleFront";
    private const string _RoleUp = "_RoleUp";
    private const string _CameraDistance = "_CameraDistance";
    private const string _RolePosition = "_RolePosition";

    void Update()
    {
        //var scaleMatirx = Matrix4x4.Scale(faceNode.transform.Matrix4x4Scale)
        if (_faceNode != null)
        {
            Vector4 faceFront = this._faceNode.up;
            Vector4 faceUp = -this._faceNode.right;
#if UNITY_EDITOR
            if (bindAnimator)
            {
                faceFront = this._faceNode.forward;
                faceUp = this._faceNode.up;
            }
#endif
            this.SetMpVector(_FaceFront, faceFront);
            this.SetMpVector(_FaceUp, faceUp);
        }
        Vector4 roleFont = this.transform.forward;
        Vector4 roleUp = this.transform.up;
        this.SetMpVector(_RoleFront, roleFont);
        this.SetMpVector(_RoleUp, roleUp);
        /*if (useDither && Camera.main != null)
        {
            var distance = Vector3.Distance(this.transform.position, Camera.main.transform.position);
            this.SetMpFloat(_CameraDistance, distance);
        }*/
        /*
         if (Camera.main != null)
         {
             float distance = Vector3.Distance(Camera.main.GetPosition(), this.transform.position + Vector3.up);
             this.SetMpFloat("_CameraDistance", distance);
         }
         else 
         {
             this.SetMpFloat("_CameraDistance", 10);
         }
        */
        Vector4 pos = this.transform.position + Vector3.up * 2.5f * this.transform.localScale.y;
        this.SetMpVector(_RolePosition, pos);
        this.ApplyMPBlock();
        if (HeadBone != null)
        {
            if (mHeadCircle != null
                && mHeadCircle.activeSelf)
            {
                var pos1 = this.transform.position;
                pos1.y = HeadBone.position.y + 1f;
                mHeadCircle.transform.position = pos1;
            }
            if (mHeadSub != null
                && mHeadSub.activeSelf)
            {
                var pos1 = this.transform.position;
                pos1.y = HeadBone.position.y + 1f;
                mHeadSub.transform.position = pos1;
            }
        }
#if UNITY_EDITOR
        /*  if (Input.GetKey(KeyCode.T))
          {
              this.SwitchSkin(SKIN_TYPE.CLOTH, "1502055_cloth_fire_01");
              this.SwitchSkin(SKIN_TYPE.FACE, "1502055_face_fire");
          }
          if (Input.GetKey(KeyCode.R))
          {
              this.ResetSkin();
          }*/
#endif
    }

    public float PlayEffect(ROLE_EFFECT_TYPE type, Vector3 ? _arg = null)
    {
        var _max = 0f;
        IRoleEffect roleEffect = null;
        switch (type)
        {
            case ROLE_EFFECT_TYPE.DEAD:
            {
                roleEffect = this.gameObject.GetOrAddComponent<ReFragDead>();
                break;
            }
            case ROLE_EFFECT_TYPE.HURT:
            {
                roleEffect = this.gameObject.GetOrAddComponent<ReHurt>();
                break;
            }
            case ROLE_EFFECT_TYPE.WEAPON_DISSOLVED:
            {
                roleEffect = this.gameObject.GetOrAddComponent<ReWeaponDissolved>();
                break;
            }
            default:
            {
                Debug.LogError("未识别的:" + type);
                break;
            }
        }
        if (roleEffect != null)
        {
            _max = roleEffect.PlayEffect(_arg);
        }
        return _max;
    }

    public float Awak()
    {
        return PlayEffect(ROLE_EFFECT_TYPE.AWAKE);
    }

    public float Dead()
    {
        if (mSimpleShadow != null)
        {
            mSimpleShadow.SetActive(false);
        }
        return PlayEffect(ROLE_EFFECT_TYPE.DEAD);
    }

    public float WeaponDissolved()
    {
        return PlayEffect(ROLE_EFFECT_TYPE.WEAPON_DISSOLVED);
    }

    /*    public float DeadFly(Vector3 _force)
        {
            return PlayEffect(ROLE_EFFECT_TYPE.DEAD_FLY, _force);
        }*/

    /*
        public void Reborn(ROLE_EFFECT_TYPE t)
        {
            if (effectList != null)
            {
                for (int i = 0; i < effectList.Length; i++)
                {
                    effectList[i].Reset(t);
                }
            }
    
            StopAllCoroutines();
            subRender.Reborn();
        }
    
        public void UpdateHpOffset(float _offset)
        {
        }
    */
    public void SetForward(Vector3 _forward)
    {
        logicRoot.transform.forward = _forward;
    }

    public void ShowAttackRange(Vector3 tpos, float radius, Vector3[] targets = null)
    {
        if (attackRanger)
        {
            attackRanger.SetOwnerSacle(transform.localScale.x);
            attackRanger.Show(tpos, radius, targets);
        }
    }

    public void CloseAttackRange()
    {
        if (attackRanger)
        {
            attackRanger.Close();
        }
    }

    /// <summary>
    /// 显示表情
    /// </summary>
    public void ShowBS(string bs)
    {
        Animation anim = this.gameObject.GetComponentInChildren<Animation>();
        if (anim == null)
            return;
        AnimationClip clip = anim.GetClip(bs);
        if (clip == null)
            return;

        //Debug.LogError(bs + temp_anim.GetClipCount());
        anim.Play(bs);
    }

    //public IRoleRender subRender;
    GameObject mArea;
    GameObject mAreaIn;
    GameObject mAreaOut;
    GameObject mHeadCircle;
    private GameObject mHeadSub;
    private Transform HeadBone;
    private bool showFocusArea = false;
    private bool showHeadTag = false;
    GameObject mSimpleShadow;
    Transform logicRoot;
    //Transform absRoot;

    AttackRanger attackRanger;
    //public IRoleEffect[] effectList = null;

    public void ShowSkinnedMeshRender(string skinName = "")
    {
        if (string.IsNullOrEmpty(skinName))
        {
            for (int i = 0; i < this._allRenders.Count; i++)
            {
                _allRenders[i].enabled = true;
            }
        }
        else
        {
            for (int i = 0; i < this._allRenders.Count; i++)
            {
                if (skinName == this._allRenders[i].gameObject.name)
                {
                    this._allRenders[i].enabled = true;
                }
            }
        }
    }

    public void HideSkinnedMeshRender(string skinName = "")
    {
        if (string.IsNullOrEmpty(skinName))
        {
            for (int i = 0; i < this._allRenders.Count; i++)
            {
                this._allRenders[i].enabled = false;
            }
        }
        else
        {
            for (int i = 0; i < this._allRenders.Count; i++)
            {
                if (skinName == this._allRenders[i].gameObject.name)
                {
                    this._allRenders[i].enabled = false;
                }
            }
        }
    }

    public void OnValidate()
    {
        if (selfGameObject != null
            && selfGameObject.activeSelf)
        {
            RefreshProperties();
        }
    }
}