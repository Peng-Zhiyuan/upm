using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

//[ExecuteInEditMode]
public enum RenderGroup
{
    OnlyWeapon,
    OnlyBody,
    All
}
public class IRoleRender : MonoBehaviour
{
    public Dictionary<Renderer, Material[]> cacheMats = new Dictionary<Renderer, Material[]>();

    public Transform FaceNode
    {
        get { return _faceNode; }
    }

    protected Transform _faceNode = null;

    protected void RenderShadow(bool show)
    {
        for (int i = 0; i < this._allRenders.Count; i++)
        { 
            if (this._allRenders[i] == null) continue;
            this._allRenders[i].shadowCastingMode = show? UnityEngine.Rendering.ShadowCastingMode.On:UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }

    public void RefreshRenderList(bool bForce = false)
    {
        //Debug.Log("RefreshRenderList");
        if (bForce || _matPropBlock == null)
        {
            _matPropBlock = new MaterialPropertyBlock();
            _allRenders.Clear();
            _bodyRenders.Clear();
            _weaponRenders.Clear();
            _boneDic.Clear();
            _renderDic.Clear();
            _faceNode = null;
            var rendersArray = transform.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < rendersArray.Length; i++)
            {
                var render = rendersArray[i];
                if (render.GetMaterials() == null) continue;
                var mats = render.GetMaterials();
                if (!cacheMats.ContainsKey(render))
                {
                    cacheMats[render] = mats;
                }
                /* if (faceNode == null
                     && mats.Length > 0)
                 {
                     for (int j = 0; j < mats.Length; j++)
                     {
                         var mat = mats[j];
                         if (mat == null)
                         {
                             continue;
                         }
                         if (mat.shader != null
                             && mat.shader.name.EndsWith("Face"))
                         {
 
                             faceNode = render.transform;
                         }
                     }
                 }*/
                if (render.name.Contains(weaponHead)
                    || render.name.Contains(weaponTag))
                {
                    _weaponRenders.Add(render);
                }
                else
                {
                    _bodyRenders.Add(render);
                }
                _allRenders.Add(render);
                _boneDic[render.name] = render.transform;
                CollectRender(render.name.ToLower(), render);
            }
            if (_renderDic.ContainsKey(faceTag))
            {
                _faceNode = _renderDic[faceTag].transform;
            }
            this.RefreshCachedBones(this.gameObject);
            // foreach (string key in _renderDic.Keys)
            // {
            //     Debug.LogWarning("key:"+key+"   val:"+ _renderDic[key].name);
            // }
        }
    }
    protected const string weaponTag = "_weapon_";
    protected const string weaponHead = "wp_";
    protected const string faceTag = "face";
    protected const string bodyTag = "body";
    protected const string hairTag = "hair";
    protected const string clothTag = "cloth";
    protected const string browTag = "brow";

    private void CollectRender(string renderName, Renderer render)
    {
        if (renderName.Contains(faceTag))
        {
            _renderDic[faceTag] = render;
            _renderDic[browTag] = render;
        }
        else if (renderName.Contains(bodyTag))
        {
            _renderDic[bodyTag] = render;
        }
        else if (renderName.Contains(hairTag))
        {
            _renderDic[hairTag] = render;
          
        }
        else if (renderName.Contains(clothTag))
        {
            _renderDic[clothTag] = render;
        }
    }

    public Dictionary<string, Transform> m_Bones = new Dictionary<string, Transform>();
    public Dictionary<string, Transform> m_BoneSlots = new Dictionary<string, Transform>();

    private bool _bWeaponShow = true;

    public bool WeaponShow
    {
        get { return _bWeaponShow; }
    }

    public void HideWeapon()
    {
        foreach (var VARIABLE in m_BoneSlots)
        {
            VARIABLE.Value.gameObject.SetActive(false);
        }
        _bWeaponShow = false;
    }

    public bool WeaponState { get; set; }

    public void ShowWeapon()
    {
        if (_bWeaponShow)
            return;
        foreach (var VARIABLE in m_BoneSlots)
        {
            VARIABLE.Value.gameObject.SetActive(true);
        }
        _bWeaponShow = true;
    }

    private void RefreshCachedBones(GameObject rootBone)
    {
        Transform tmp_boneRoot = rootBone.transform;
        /*if (rootBone.name == "Bip001")
            tmp_boneRoot = rootBone.transform;
        else
            tmp_boneRoot = rootBone.transform.Find("Rig_Char003/Bip001");

        if (tmp_boneRoot == null)
        {
            tmp_boneRoot = rootBone.transform.Find("Bip001");
        }

        if (tmp_boneRoot == null)
            return;*/
        //Debug.Log("Refresh Cache bones!" + rootBone.name);
        m_Bones.Clear();
        var tmp_bones = ListPool.ListPoolN<Transform>.Get();
        tmp_boneRoot.GetComponentsInChildren(tmp_bones);
        m_Bones.Add("root", tmp_boneRoot);
        foreach (var tmp_bone in tmp_bones)
        {
            var boneName = tmp_bone.name.ToLower();
            if (boneName.StartsWith("bip")
                || boneName.StartsWith("bone")
                || boneName.StartsWith("b_")
                || boneName.StartsWith("body_"))
            {
                if (m_Bones.ContainsKey(boneName))
                {
                    //LogMgr.LogErrorFormat("Creature -> RefreshCachedBones : {0} is existed", tmp_bone.name);
                    continue;
                }
                m_Bones.Add(boneName, tmp_bone);
            }
            else
            {
                if ((boneName.Contains(weaponTag) || boneName.Contains(weaponHead))
                    && !m_BoneSlots.ContainsKey(boneName))
                {
                    m_BoneSlots.Add(boneName, tmp_bone);
                }
            }
        }
        ListPool.ListPoolN<Transform>.Release(tmp_bones);
    }

    public virtual void Init() { }
    public virtual void Reborn() { }

    public virtual Transform GetBoneTrans(string name)
    {
        return null;
    }

    public virtual void SetMpTexture(string _name, Texture2D _value)
    {
        if (_matPropBlock == null) return;
        _matPropBlock.SetTexture(_name, _value);

    }

    public virtual Texture2D GetMpTexture(string _name)
    {
        if (_matPropBlock == null) return null;
        var tex = _matPropBlock.GetTexture(_name);
        return tex as Texture2D;
    }

    public virtual void SetMpColor(string _name, Color _value)
    {
        if (_matPropBlock == null) return;
        _matPropBlock.SetColor(_name, _value);
    }

    private void SetPropertyBlock(MaterialPropertyBlock propertyBlock, RenderGroup renderGroup = RenderGroup.All)
    {
        switch (renderGroup)
        {
            case RenderGroup.OnlyWeapon:

                for (int i = _weaponRenders.Count - 1; i >= 0; i--)
                {
                    if (_weaponRenders[i] != null)
                    {
                        _weaponRenders[i].SetPropertyBlock(propertyBlock);
                    }
                    else
                    {
                        _weaponRenders.RemoveAt(i);
                    }
                }
                break;
            case RenderGroup.OnlyBody:
                for (int i = _bodyRenders.Count - 1; i >= 0; i--)
                {
                    if (_bodyRenders[i] != null)
                    {
                        _bodyRenders[i].SetPropertyBlock(propertyBlock);
                    }
                    else
                    {
                        _bodyRenders.RemoveAt(i);
                    }
                }
                break;
            case RenderGroup.All:
                for (int i = _allRenders.Count - 1; i >= 0; i--)
                {
                    if (_allRenders[i] != null)
                    {
                        _allRenders[i].SetPropertyBlock(propertyBlock);
                    }
                    else
                    {
                        _allRenders.RemoveAt(i);
                    }
                }
                break;
            default:break;
        }
       
    }

    private void OnDestroy()
    {
        this.cacheMats.Clear();
    }


    public virtual void SetMpFloat(string _name, float _value)
    {
        if (_matPropBlock == null) return;
        _matPropBlock.SetFloat(_name, _value);
    }

    public virtual void SetMpVector(string _name, Vector4 _value)
    {
        if (_matPropBlock == null) return;
        _matPropBlock.SetVector(_name, _value);
    }

    public void ApplyMPBlock(RenderGroup renderGroup = RenderGroup.All)
    {
        if (_allRenders.Count > 0)
        {
            this.SetPropertyBlock(_matPropBlock, renderGroup);
        }
    }

    public void SwitchKeyword(string key, bool isSwitch, RenderGroup renderGroup = RenderGroup.All)
    {
        List<Renderer> renderlist = null;
        switch (renderGroup)
        {
           
            case RenderGroup.All:
                renderlist = _allRenders;
                break;
            case RenderGroup.OnlyWeapon:
                renderlist = _weaponRenders;
                break;
            case RenderGroup.OnlyBody:
                renderlist = _bodyRenders;
                break;
            default:
                renderlist = _allRenders;
                break;
        }
        if (isSwitch)
        {
            for (int i = 0; i < renderlist.Count; i++)
            {
                var mats = renderlist[i].GetMaterials();
                for (int j = 0; j < mats.Length; j++)
                {
                    if (mats[j] == null)
                    {
                        continue;
                    }
                    mats[j].EnableKeyword(key);
                }
            }
        }
        else
        {
            for (int i = 0; i < renderlist.Count; i++)
            {
                var mats = renderlist[i].GetMaterials();
                for (int j = 0; j < mats.Length; j++)
                {
                    if (mats[j] == null) continue;
                    mats[j].DisableKeyword(key);
                }
            }
        }
    }

    public void ResetMaterials()
    {
        for (int i = 0; i < _allRenders.Count; i++)
        {
            var render = _allRenders[i];
            if (cacheMats.ContainsKey(render))
            {
                render.SetMaterials(cacheMats[render]);
            }
        }
    }

    public async void SwitchMaterials(string matName)
    {
        await MatsPool.Instance.CacheMaterialAsync(matName);
        for (int i = 0; i < _allRenders.Count; i++)
        {
            var render = _allRenders[i];
            if (render is SkinnedMeshRenderer)
            {
                var mats = render.GetMaterials();
                Material[] new_mats = new Material[mats.Length];
                for (int j = 0; j < mats.Length; j++)
                {
                    if (mats[j].HasProperty("_Albedo"))
                    {
                        var matInstance = MatsPool.Instance.TakeMaterial(matName);
                        new_mats[j] = matInstance;
                        var albedo = mats[j].GetTexture("_Albedo");
                        new_mats[j].SetTexture("_Albedo", albedo);
                    }
                    else
                    {
                        var matInstance = MatsPool.Instance.TakeMaterial(matName);
                        new_mats[j] = matInstance;
                        //var albedo = mats[j].GetTexture("_BaseMap");
                        //new_mats[j].SetTexture("_Albedo", albedo);
                    }
                }
                render.SetMaterials(new_mats);
            }
        }
    }
    /*   public bool IsShaderPassEnabled(string  passName)
       {
           return _renders.material.GetShaderPassEnabled(passName);
       }
   
       public void SetShaderPassEnabled(string passName, bool val)
       {
            _renders.material.SetShaderPassEnabled(passName, val);
       }*/

    public virtual void SwitchPlayMode(bool _mode)
    {
        _inPlayMode = _mode;
    }

   // public virtual void PlayAni(string aniLogic) { }

    protected List<Renderer> _bodyRenders = new List<Renderer>();
    protected List<Renderer> _weaponRenders = new List<Renderer>();
    protected List<Renderer> _allRenders = new List<Renderer>();
    protected Dictionary<string, Renderer> _renderDic = new Dictionary<string, Renderer>();
    protected MaterialPropertyBlock _matPropBlock;
    protected bool _inPlayMode = true;
    protected Dictionary<string, Transform> _boneDic = new Dictionary<string, Transform>();
}