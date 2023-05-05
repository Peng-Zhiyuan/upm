using UnityEngine;
using System.Threading.Tasks;

public partial class HairFragment : LagacyFragment,IUpdatable
{
    void Awake()
    {
        this.ScrollView_hair.onSetItemView = OnSetItemViewHandler;
    }
    public override void OnSwitchedFrom()
    {

    }

    public override void OnPageNavigatedTo(PageNavigateInfo info)
    {
        if (info.terminalOperation == NavigateOperation.Back)
        {
            RebuildScrollView();
        }
    }
    public override void OnSwitchedTo()
    {
        RebuildScrollView();
    }

    void OnSetItemViewHandler(object data, Transform viewTransform)
    {
        var hairColorRow = data as HairColorRow;
        var view = viewTransform.GetComponent<HairCellView>();
        view.Bind(hairColorRow);
        view.onClick = OnViewClicked;
    }

    public HeroInfo DisplayHero
    {
        get
        {
            return HeroDisplayer.Hero;
        }
    }

    private void RefreshSelected(int id)
    {
        var viewList = this.ScrollView_hair.GetViewList<HairCellView>();
        for (int i = 0; i < viewList.Count; i++)
        {
            if (viewList[i] == null || viewList[i].hairRow == null) continue;
            viewList[i].Selected = viewList[i].hairRow.Id == id;
        }
    }

   

    public bool HasItem
    {
        get
        {
            if (HeroDressData.Instance.CurrentAvatarInfo.Item3 == 0) return true;
            var item3 = Database.Stuff.itemDatabase.GetFirstItemInfoOfRowId(HeroDressData.Instance.CurrentAvatarInfo.Item3);
            if (item3 != null && item3.val > 0)
            {
                return true;
            }
            return false;
        }
    }

    public bool UsedItem
    {
        get
        {
            return this.DisplayHero.AvatarInfo.Item3 == HeroDressData.Instance.CurrentAvatarInfo.Item3 && (this.DisplayHero.AvatarInfo.Item4 > Clock.ToTimestampS(Clock.Now) || this.DisplayHero.AvatarInfo.Item4 == 0);
        }
    }

    private float tick = 0;

    public void OnUpdate()
    {
        if (this == null) return;
         tick += Time.deltaTime;
        if (tick >= 1)
        {
            var id = HeroDressData.Instance.CurrentAvatarInfo.Item3;
            var remainTimeS = HeroDressData.Instance.RemainHairTimeS(id);
            if (remainTimeS <= 0)
            {
                this.HairRemainTime.text = "00:00:00";
                UpdateManager.Stuff.Remove(this);
            }
            else 
            {
                this.HairRemainTime.text = System.TimeSpan.FromSeconds(remainTimeS).ToStringHour();
                tick = 0;

            }

        }
    }
    void OnDisable()
    {
        UpdateManager.Stuff.Remove(this);
    }

    void OnDestroy()
    {
        UpdateManager.Stuff.Remove(this);
    }

    private async Task UpdateHairInfo(HairColorRow hairColorRow)
    {
        HeroDressData.Instance.CurrentAvatarInfo = DisplayHero.AvatarInfo;

        HeroDressData.Instance.CurrentAvatarInfo.Item3 = hairColorRow.Id;
        if (HeroDressData.Instance.CurrentAvatarInfo.Item3 > 0)
        {
            long tick = Clock.ToTimestampS(Clock.Now.AddSeconds(hairColorRow.Expire));
            HeroDressData.Instance.CurrentAvatarInfo.Item4 = tick;
        }
        else 
        {
            HeroDressData.Instance.CurrentAvatarInfo.Item4 = 0;
        }
       

        this.RefreshSelected(HeroDressData.Instance.CurrentAvatarInfo.Item3);
        this.HeroName.text = LocalizationManager.Stuff.GetText(this.DisplayHero.Name);
        this.tick = 0;
        var remainTimeS = HeroDressData.Instance.RemainHairTimeS(hairColorRow.Id);
        if (remainTimeS > 0)
        {
           
            TimeNode.SetActive(true);
            this.HairRemainTime.text = System.TimeSpan.FromSeconds(remainTimeS).ToStringHour();
             UpdateManager.Stuff.Add(this);
        }
        else 
        {
            TimeNode.SetActive(false);
        }
        this.State.SetActive(UsedItem);
        //bool canUsed = HasItem && !UsedItem;
        bool canUsed = HeroDressData.Instance.IsHairCanUse(hairColorRow.Id) && !UsedItem;
        this.UsedButton.SetActive(canUsed);
        this.UsedButtonDisabled.SetActive(!canUsed);

        if (HeroDressData.Instance.CurrentAvatarInfo.Item3 > 0)
        {
            SkinCostPanel.gameObject.SetActive(true);
            this.SkinName.text = LocalizationManager.Stuff.GetText(hairColorRow.Name);
        }
        else
        {
            SkinCostPanel.gameObject.SetActive(false);
            this.SkinName.text = LocalizationManager.Stuff.GetText("heroskin_default_hair");
        }

        HeroSkinPage skinPage = this.fragmentManager.view as HeroSkinPage;
        await skinPage.CurrentModelViewUnit.rolePreviewUnit.SetData(this.DisplayHero.HeroId, HeroDressData.Instance.CurrentAvatarInfo);
    }


    async void RebuildScrollView()
    {
        var dataList = HeroDressData.Instance.HairDic[HeroDisplayer.Hero.HeroId];
        this.ScrollView_hair.DataList = dataList;
        var id = HeroDressData.Instance.CurrentAvatarInfo.Item4 > Clock.ToTimestampS(Clock.Now)?HeroDressData.Instance.CurrentAvatarInfo.Item3: 0;
        int selIndex = dataList.FindIndex((row) =>
        {
            return row.Id == id;
        });
        selIndex = Mathf.Max(0, selIndex);
        await this.UpdateHairInfo(dataList[selIndex]);
        
    }
    async void OnViewClicked(HairCellView view)
    {
       // HeroDressData.Instance.CurrentAvatarInfo.Item3 = view.hairRow.Id;
       
      //  HeroSkinPage skinPage = this.fragmentManager.view as HeroSkinPage;
       // await skinPage.CurrentModelViewUnit.rolePreviewUnit.SetData(this.DisplayHero.HeroId, HeroDressData.Instance.CurrentAvatarInfo);
        UpdateHairInfo(view.hairRow);
    }

    public async void OnGet()
    {
        var id = HeroDressData.Instance.CurrentAvatarInfo.Item3;
        if (id == 0) return;
        var f = await UIEngine.Stuff.ShowFloatingAsync<ItemInfoFloating>();
        f.Set(id);
    }

    public async void OnUse()
    {
        var id = HeroDressData.Instance.CurrentAvatarInfo.Item3;
        bool canUsed = HeroDressData.Instance.IsHairCanUse(id) && !UsedItem;
        if (canUsed)
        {
            var skinConfirmPage = await UIEngine.Stuff.ForwardOrBackToAsync<SkinConfirmPage>();
            bool result = await skinConfirmPage.AskConfirmAsync();
            if (!result) return;
            string info = HeroDressData.Instance.AvatarInfoToString(HeroDressData.Instance.CurrentAvatarInfo);
            TrackManager.CustomReport("Skin", info);
            await DressApi.RequestSwitchAvatarAsync(DisplayHero.HeroId, info);
            var data = HeroDressData.Instance.HairDic[DisplayHero.HeroId];
         
            int selIndex = data.FindIndex((row) => { if (row.Id == id) return true; else return false; });
            this.UpdateHairInfo(data[selIndex]);
            ToastManager.ShowLocalize("heroskin_dress_suc");
        }
    }
    public async void OnBuy()
    {
        await UIEngine.Stuff.ForwardOrBackToAsync<SkinBuyPage>();
    }

}
