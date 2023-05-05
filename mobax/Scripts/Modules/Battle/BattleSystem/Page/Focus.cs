using System.Collections.Generic;
using System.Threading.Tasks;
using BattleEngine.Logic;
using BattleSystem.ProjectCore;
using UnityEngine;
using UnityEngine.UI;


/*public class FocusEnemyItem
{
    public GameObject Root;
    public GameObject SelectedImage;
    public Image Head;
    public Creature Data;
    public Focus Owner;
    //public int Index;
    public Image Hp;
    public Image Vim;
    public Button Confirm;
    public GameObject BuffTag;

    public FocusEnemyItem(Transform item, Focus focus, BattlePage page)
    {
        Owner = focus;
        Root = item.gameObject;
        SelectedImage = item.Find("Selected").gameObject;
        Head = item.Find("Mask/Head").GetComponent<Image>();
        Hp = item.Find("Hp").GetComponent<Image>();
        Vim = item.Find("Vim").GetComponent<Image>();
        BuffTag = item.Find("Buff").gameObject;
        //Index = index;
        Confirm = item.Find("Confirm").GetComponent<Button>();
        Confirm.SetActive(false);
        item.Find("Click").GetComponent<Button>().onClick.AddListener(delegate
        {
            //focus.Select(Int32.Parse(Root.name));

            if (Data.mData.IsDead)
                return;
            //Data.mData.Publish(new FocusPreTargetEvent() { targetUID = Data.mData.UID });
            var list = SceneObjectManager.Instance.GetAllPlayer();
            foreach (var VARIABLE in list)
            {
                if (VARIABLE.IsHero)
                    VARIABLE.mData.Publish(new FocusPreTargetEvent() {targetUID = Data.mData.UID});
            }

            CameraManager.Instance.FocusTarget = Data;

            Owner.CleanPreSelect();
            Confirm.SetActive(true);
            page.PreFocus(Data);
        });

        item.Find("Selected/RoleSelect").GetComponent<Button>().onClick.AddListener(delegate
        {
            focus.FocusFight(Data);
            focus.ShowFocus(false);
            //focus.foro.SetActive(false);
            //page.ShowFunctionButton(true);
        });

        item.Find("Confirm").GetComponent<Button>().onClick.AddListener(delegate
        {
            Owner.Page.ShowFocusVis(false);
            
            focus.FocusFight(Data);
            focus.ShowFocus(false);
            //focus.foro.SetActive(false);
            //page.ShowFunctionButton(true);
            Confirm.SetActive(false);
        });

        SetData(null);
    }
    
    public void SetData(Creature role)
    {
        Data = role;
        BuffTag.SetActive(Data != null && Data.mData.GetStagePassiveBuff() != null);
        Refresh();
    }

    public void Selected(bool sel)
    {
        SelectedImage.SetActive(sel);
    }

    public void Refresh()
    {
        if (Data == null)
        {
            Root.SetActive(false);
            return;
        }

        Root.SetActive(true);
        Selected(false);

        var info = StaticData.HeroTable.TryGet(Data.ConfigID);
        if (info != null)
        {
            UiUtil.SetSpriteInBackground(this.Head, () => info.Head + ".png");
        }

        Hp.fillAmount = Data.mData.CurrentHealth.Percent();
        Vim.fillAmount = Data.mData.CurrentVim.Percent();
    }
}*/

public partial class Focus : MonoBehaviour
{
    public List<FocusEnemyView> EnemyItems = new List<FocusEnemyView>();
    public BattlePage Page;
    public DefenceOrder DefenceRoot;
    public GameObject HideRoot;
    public GameObject HeroRoot;

    public void Init(BattlePage page)
    {
        Page = page;

        FocusEnemyView.focus = this;
        FocusEnemyView.page = page;
        DefFocusEnemyView.page = page;
        
        this.FocusEnemyItem.SetActive(false);
        //EnemyItems.Add(new FocusEnemyItem(this.FocusEnemyItem1.transform, this, page, 0));
        //EnemyItems.Add(new FocusEnemyItem(this.FocusEnemyItem2.transform, this, page, 1));
        //EnemyItems.Add(new FocusEnemyItem(this.FocusEnemyItem3.transform, this, page, 2));

        /*this.FocusButton.GetComponent<Button>().onClick.AddListener(delegate
        {
            if(SceneObjectManager.Instance.AllEnemyDie())
                return;
            
            this.FocusRoot.SetActive(true);
            this.OrderRoot.SetActive(false);
            page.ShowFunctionButton(true);
            SetDatas();
        });
        
        this.DefenceButton.GetComponent<Button>().onClick.AddListener(delegate
        {
            this.DefenceRoot.SetActive(true);
            this.OrderRoot.SetActive(false);
            this.DefenceRoot.SetDatas();
            this.Page.ShowOrderDefencePanels(false);
        });
        */
        
        /*this.FixedDefenceClick.GetComponent<Button>().onClick.AddListener(delegate
        {
            BattleManager.Instance.DefendFocusOnFiring(Page.FixedModeTarget.mData.UID);
            Page.ShowFocusVis(false);
            WwiseEventManager.SendEvent(WwiseGameEvent.Custom, "FocusDefenceLine");
            Page.ResetFocusCD();

            if(!Battle.Instance.FocusItemUse)
                return;
            if(BattleDataManager.Instance.UseItem(Battle.Instance.FocusItemID))
                GameEventCenter.Broadcast(GameEvent.FocusItemUseEvent);
            else
            {
                return;
            }
            BattleManager.Instance.BattleInfoRecord.SetItemUse(Battle.Instance.FocusItemID);
        });*/

        GameEventCenter.AddListener(GameEvent.ActorDie, this, this.RoleDie);
    }

    public void OnDestroy()
    {
        GameEventCenter.RemoveListener(GameEvent.ActorDie, this);
    }

    void RoleDie(object[] param)
    {
        if (this.EnemyList.gameObject.activeSelf)
            SetDatas();
    }

    public void SetDatas()
    {
        if(!FocusRoot.gameObject.activeSelf)
            return;
        
        foreach (var VARIABLE in EnemyItems)
        {
            if (VARIABLE == FocusEnemyView.LastSelected)
            {
                if (VARIABLE.data != null && VARIABLE.data.mData.IsDead)
                {
                    FocusEnemyView.HideLastSelect();
                }
            }
            VARIABLE.SetActive(false);
        }

        List<Creature> targets = new List<Creature>();
        var list = BattleManager.Instance.ActorMgr.GetCamp(1);
        foreach (var VARIABLE in list)
        {
            if (!VARIABLE.mData.IsDead)
            {
                if (VARIABLE.mData.GetStagePassiveBuff() != null)
                {
                   targets.Insert(0, VARIABLE); 
                }
                else
                {
                    targets.Add(VARIABLE);
                }
            }
        }
        int index = 0;
        foreach (var VARIABLE in targets)
        {
            if (EnemyItems.Count < (index + 1))
            {
                var go = GameObject.Instantiate(this.FocusEnemyItem.gameObject,
                    this.FocusEnemyItem.transform.parent);
                var item = go.GetComponent<FocusEnemyView>();
                item.HideSelect();
                EnemyItems.Add(item);
            }
            
            EnemyItems[index].SetData(VARIABLE);
            EnemyItems[index].SetActive(true);
            EnemyItems[index].name = $"{index}";
            index++;
        }

        if (Battle.Instance.mode.ModeType == BattleModeType.Fixed)
        {
            this.FixedModeRoot.SetActive(true);
            
            var info = StaticData.HeroTable.TryGet(Page.FixedModeTarget.ConfigID);
            if (info != null)
            {
                UiUtil.SetSpriteInBackground(this.FixedHeadIcon, () => info.Head + ".png");
            }
        }
        else
        {
            this.FixedModeRoot.SetActive(false);
        }
    }

    /*public void Select(int index)
    {
        for (int i = 0; i < EnemyItems.Count; i++)
        {
            EnemyItems[i].Selected(i == index);
        }
    }*/

    private int CD = 15;

    public async void FocusFight(Creature role)
    {
        //CloseFocusList();

        BattleEngine.Logic.BattleManager.Instance.AtkerFocusOnFiring(role.mData.UID);

        //Page.SetOrderCD(15);

        role.ShowSwitchTarget(true);

        WwiseEventManager.SendEvent(TransformTable.Custom, "FocusDefenceLine");
    }

    public void ShowFocus(bool vis)
    {
        this.FocusRoot.SetActive(vis);
        //this.OrderRoot.gameObject.SetActive(false);
    }

    public bool IsFocusShow()
    {
        return this.FocusRoot.gameObject.activeSelf;
    }

    public void CloseFocusList()
    {
        //EnemyList.SetActive(false);
        this.FocusRoot.SetActive(false);
        CameraManager.Instance.FocusTarget = null;
    }

    public async void ShowOrderRoot(bool vis)
    {
        this.FocusRoot.SetActive(vis);
        //OrderRoot.SetActive(vis);
        if (vis)
        {
            SetDatas();
            await Task.Delay(300);
            GuideManagerV2.Stuff.Notify("DefenceChooseEnemy");
        }
        
        //this.OrderRoot.SetActive(vis);
        Page.CleanFocus();
    }

    public void CleanPreSelect()
    {
        foreach (var VARIABLE in EnemyItems)
        {
            VARIABLE.SetActive(false);
        }
        Page.CleanFocus();
    }

    public Transform GetFocusItem(int index)
    {
        if (EnemyItems.Count < index + 1)
            return null;

        return EnemyItems[index].GetClickNode();
    }
    
    public Transform GetConfirmItem(int index)
    {
        if (EnemyItems.Count < index + 1)
            return null;

        return EnemyItems[index].GetConfirmNode();
    }
    
    public void UpdateHp(Creature role)
    {
        if(!FocusRoot.gameObject.activeSelf)
            return;
        
        foreach (var VARIABLE in EnemyItems)
        {
            if (VARIABLE.data != null && VARIABLE.data.mData.UID == role.mData.UID)
            {
                VARIABLE.UpdateHp();
                break;
            }
        }
    }
}