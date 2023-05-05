using System.Threading.Tasks;
using UnityEngine;

public partial class CsRolePreviewRoot : MonoBehaviour
{
    // private Bucket Bucket => BuketManager.Stuff.Main;
    string lastModelAddressName = "";

    public GameObject HeroGo { get; set; }

    public async Task SetData(int roleId, int avatarId)
    {
        await this.SetHero(roleId, avatarId);
    }

    public async Task SetData(int roleId, (int, int, int, long) avatarInfo)
    {
        await this.SetHero(roleId, avatarInfo);
    }


    public void SetLookAt(CustomCameraController camera)
    {
        if (this.HeroGo == null) return;
        camera.m_Target = this.HeroGo.transform;
    }

    public void OnDisable()
    {
        if (this.HeroGo != null)
        {
            //this.HeroGo.SetActive(false);
            //UnityEngine.GameObject.Destroy(this.HeroGo);
            //this.HeroGo = null;
        }
    }

    public void SetLightOpen(bool val)
    {
        this.Directional_Light.gameObject.SetActive(val);
    }
    private GameObject particle = null;
    private async Task SetHero(int roleId, int avatarId = 0)
    {
        string avatarName = RoleHelper.GetAvatarName(roleId, avatarId);
        await this.SetHeroModel(avatarName, StaticData.HeroTable[roleId].showWeapon > 0);
    }


    private async Task SetHero(int roleId, (int, int, int, long) avatarInfo)
    {
        string avatarName = RoleHelper.GetAvatarName(roleId, avatarInfo.Item1);
        await this.SetHeroModel(avatarName, StaticData.HeroTable[roleId].showWeapon > 0);
        if (this.HeroGo == null) return;
        var roleRender = this.HeroGo.GetComponent<RoleRender>();
        await roleRender.SwitchHeroSkin(avatarInfo);
        if (this.Root.childCount > 0)
        {
            for (int i = this.Root.childCount - 1; i >= 0; i--)
            {
                var child = this.Root.GetChild(i);
                if (this.HeroGo != child.gameObject)
                {
                    UnityEngine.GameObject.DestroyImmediate(child.gameObject);
                }
      
            }
        }
    }

    HDRColorInfo hdrColor = new HDRColorInfo();
    public async Task SetHeroModel(string modelAddress, bool showWeapon = false, int modelScale = 100)
    {
       
        if(this.particle != null) this.particle.SetActive(false);
        string newModelAddressName = $"{modelAddress}.prefab";
        if (newModelAddressName == lastModelAddressName && this.HeroGo != null)
        {
           // Debug.LogError("model already loaded!");
            return;
        }
        if (this.HeroGo != null)
        {
         
            UnityEngine.GameObject.DestroyImmediate(this.HeroGo);
            this.HeroGo = null;
        }
       /* if (this.Root.childCount > 0)
        {
            for (int i = this.Root.childCount - 1; i >= 0; i--)
            {
                var child = this.Root.GetChild(i);
                //Debug.LogError("destroy child:"+ child.name);
                UnityEngine.GameObject.DestroyImmediate(child.gameObject);
            }
        }*/

        var bucket = BucketManager.Instance.GetBucket(UIEngine.LatestNavigatePageName);
        if (!string.IsNullOrEmpty(lastModelAddressName))
        {
            bucket.Release(lastModelAddressName);
            lastModelAddressName = null;
        }
        lastModelAddressName = newModelAddressName;

        var heroPrefab = await bucket.GetOrAquireAsync(newModelAddressName) as UnityEngine.GameObject;
        if (heroPrefab == null) return;

       
        if (particle == null)
        {
            string particleAddressName = "fx_refresh_1.prefab";
            var particlePrefab = await bucket.GetOrAquireAsync(particleAddressName) as UnityEngine.GameObject;
            if (this == null) return;
            this.particle = UnityEngine.GameObject.Instantiate(particlePrefab) as UnityEngine.GameObject;
            this.particle.transform.CustomSetParent(this.transform);
        }

        this.particle.SetActive(true);
       
        this.HeroGo = UnityEngine.GameObject.Instantiate(heroPrefab) as UnityEngine.GameObject;

        if (this.HeroGo == null) return;

        if (this.transform.parent != null)
        {
            this.HeroGo.transform.SetParent(this.Root);
        }
        float factor = Mathf.Pow(2, 0.5f);
        hdrColor.HDRColor = new Color(191, 138, 3, 0) * factor / 255f;
        var roleFresnelAnim = this.HeroGo.GetOrAddComponent<RoleFresnelAnim>();
        roleFresnelAnim.PlayAnim(0.5f, hdrColor);

        if (!showWeapon)
        {
            var roleRender = this.HeroGo.GetComponent<RoleRender>();
            if (roleRender != null)
            {
                roleRender.HideWeapon();
            }
            else
            {
                Debug.LogError("Role render is null，please attach rolerender conponent！");
            }
        }
        this.HeroGo.transform.localPosition = UnityEngine.Vector3.zero;
        this.HeroGo.transform.localScale = new UnityEngine.Vector3(modelScale / 100f, modelScale / 100f, modelScale / 100f);
        LagacyUtil.SetRootLayer(this.HeroGo, ELayerMask.UI);
        



       
    }

}