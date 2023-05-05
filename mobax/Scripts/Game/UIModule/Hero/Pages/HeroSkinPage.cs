using UnityEngine;
using System.Threading.Tasks;

public partial class HeroSkinPage : Page
{

    public PressComponent pressComponent;
    public Animation anim;
    public ModelViewUnit CurrentModelViewUnit
    {
        get 
        {
            return this.ModelViewUnit;
        }
    }
   /* public SkinScrollerView skinScrollView;
    public ClothScrollerView clothScrollView;*/
    void Awake()
    {

    }

    public HeroInfo DisplayHero
    {
        get
        {
            return HeroDisplayer.Hero;
        }
    }

    protected override async Task LogicBackAsync()
    {
        if (UIEngine.Stuff.pageStack.Find("HeroPage") != null)
        {
            Debug.LogError("LogicBackAsync");
            await UIEngine.Stuff.BackToAsync<HeroPage>(HeroDisplayer.Hero);
        }
        else 
        {
            await UIEngine.Stuff.BackAsync();
        }
    }

    public override async Task OnNavigatedToPreperAsync(PageNavigateInfo info)
    {
        if (info.terminalOperation == NavigateOperation.Forward)
        {
            SkinMenu.SelectedIndex = 0;
            HeroDressData.Instance.CurrentAvatarInfo = HeroDressData.Instance.GetAvatarInfo(this.DisplayHero);
            SwitchCameraMode();
        }
      
    }


    /*    public override void OnPush()
        {
            SkinMenu.SelectedIndex = 0;
        }*/
    public override void OnNavigatedTo(PageNavigateInfo info)
    {
        if (info.terminalOperation == NavigateOperation.Forward)
        {
            this.SwitchFragment();
        }
    }

   
    private void SwitchCameraMode()
    {
        ModelViewUnit.rolePreviewUnit.SwitchCameraMode(HeroDisplayer.Hero.HeroId, CameraViewMode.NORMAL, true);
        //ModelViewUnit.SetInfo(HeroDisplayer.hero, CameraViewMode.NORMAL);
    }

    

    //---------------------------------------------------cloth-------------------------------------------------//
   
    //---------------------------------------------------hair-------------------------------------------------//

    //-----------------------------------------------------------------------------------------------------------------------------//
    public async void ShowNext()
    {
        HeroDisplayer.Filter.SetHeroByOffsetWithSkinCheck(1);
        HeroDressData.Instance.CurrentAvatarInfo = DisplayHero.AvatarInfo;
        SwitchCameraMode();

        this.fragmentManager.CurrentFragment.OnSwitchedTo();
    }

    public async void  ShowPrev()
    {
        HeroDisplayer.Filter.SetHeroByOffsetWithSkinCheck(-1);
        HeroDressData.Instance.CurrentAvatarInfo = DisplayHero.AvatarInfo;
        SwitchCameraMode();
        this.fragmentManager.CurrentFragment.OnSwitchedTo();
    }

  
    private void SwitchFragment()
    {
        switch (SkinMenu.SelectedIndex)
        {
            case 0:
                this.fragmentManager.SwitchFragment<SkinFragment>();
                break;
            case 1:
                this.fragmentManager.SwitchFragment<ClothFragment>();
                break;
            case 2:
                this.fragmentManager.SwitchFragment<HairFragment>();
                break;
            default:
                break;

        }
    }

    public async void OnMenuButton(string msg)
    {
        switch (msg)
        {
            case "ClothSkin":
                SkinMenu.SelectedIndex = 0;
                break;
            case "ColorSkin":
                SkinMenu.SelectedIndex = 1;
                break;
            case "HairSkin":
                SkinMenu.SelectedIndex = 2;
                break;
            default:break;
        }
        this.SwitchFragment();

    }

    
}