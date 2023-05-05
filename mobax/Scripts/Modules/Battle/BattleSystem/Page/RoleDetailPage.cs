using System.Threading.Tasks;
using BattleEngine.Logic;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public partial class RoleDetailPage : Page
{
    private Creature Owner;
    public override async Task OnNavigatedToPreperAsync(PageNavigateInfo info)
    {
        if (info.terminalOperation == NavigateOperation.Forward)
        {
            this.Owner = info.param as Creature;

            Init();
        }
    }
    

    
    public override void OnForwardTo(PageNavigateInfo info)
    {
        ButtonUtil.SetClick(this.CloseButton, () =>
            {
                UIEngine.Stuff.Back();
            }
        );
    }

    private void Init()
    {
        if(Owner == null)
            return;

        var data = Owner;
        var info = StaticData.HeroTable.TryGet(data.ConfigID);
        if(info == null)
            return;
        
        RoleLevel.text = Owner.mData.battleItemInfo.lv.ToString();
        Name.text = LocalizationManager.Stuff.GetText(info.Name);
        
        if (data.IsMain)
        {
            var hero = HeroManager.Instance.GetHeroInfo(data.mData.ConfigID);
            if(hero != null)
                Power.text = hero.Power.ToString(); 
            
            VimRoot.SetActive(false);
            RoleHPBar_Enemy.SetActive(false);
            RoleHPBar.fillAmount = data.mData.CurrentHealth.Percent();
        }
        else
        {
            RoleHPBar.SetActive(false);
            RoleHPBar_Enemy.fillAmount = data.mData.CurrentHealth.Percent();
        }
        
        UiUtil.SetSpriteInBackground(RoleIcon,
            () => HeroHelper.GetResAddress(data.mData.ConfigID, HeroResType.HalfIcon),
            isDisplayDefault => { _roleIcon.gameObject.SetActive(!isDisplayDefault); }, 1, null, true);
        
        //UiUtil.SetSpriteInBackground(RoleIcon, () => info.Head+ ".png", null, 1, null, true);
        UiUtil.SetSpriteInBackground(RoleJob, () => "Icon_occ" + info.Job + ".png", null, 1, null, true);
        UiUtil.SetSpriteInBackground(RoleAttr, () => "element_" + info.Element + ".png", null, 1, null, true);

        RoleHpDes.text = data.mData.CurrentHealth.Value + "/" + data.mData.CurrentHealth.MaxValue;
        
        RoleVimDes.text = data.mData.CurrentVim.Value + "/" + data.mData.CurrentVim.MaxValue;
        RoleVimBar.fillAmount = data.mData.CurrentVim.Percent();
        
        //技能
        foreach (var VARIABLE in data.mData.SkillSlots)
        {
            if (VARIABLE.Value.SkillBaseConfig.skillType == (int)SKILL_TYPE.Passive
                || VARIABLE.Value.SkillBaseConfig.skillType == (int)SKILL_TYPE.ATK)
                continue;
            
            var go = Instantiate(SkillItem, SkillRoot.transform);
            go.transform.Find("Level").GetComponent<Text>().text = VARIABLE.Value.SkillBaseConfig.Level.ToString();
            UiUtil.SetSpriteInBackground(go.transform.Find("Icon").GetComponent<Image>(), () => $"{VARIABLE.Value.SkillBaseConfig.Icon}.png", null, 1, null, true);
            go.transform.Find("Name").GetComponent<Text>().text = LocalizationManager.Stuff.GetText(VARIABLE.Value.SkillBaseConfig.Name);
        }

        Creature target = SceneObjectManager.Instance.Find(data.mData.targetKey);
        if (target == null)
        {
            TargetRoot.SetActive(false);
            return;
        }
            
        
        info = StaticData.HeroTable.TryGet(target.ConfigID);
        if(info == null)
            return;
        
        TargetLevel.text = target.mData.battleItemInfo.lv.ToString();
        TargetName.text = LocalizationManager.Stuff.GetText(info.Name);
        
        UiUtil.SetSpriteInBackground(TargetIcon, () => info.Head+ ".png", null, 1, null, true);
        UiUtil.SetSpriteInBackground(TargetJob, () => "Icon_occ" + info.Job + ".png", null, 1, null, true);
        UiUtil.SetSpriteInBackground(TargetAttribute, () => "element_" + info.Element + ".png", null, 1, null, true);
    }
}