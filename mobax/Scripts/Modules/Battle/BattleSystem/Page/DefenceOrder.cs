using System.Collections.Generic;
using BattleEngine.Logic;
using UnityEngine;
using UnityEngine.UI;

public class DefenceItem
{
    public GameObject Root;
    public GameObject SelectedImage;
    public Image Head;
    public Creature Data;
    public DefenceOrder Owner;
    public int Index;
    public Image Hp;
    public Image Vim;
    public GameObject SelectBG;

    public DefenceItem(Transform item, DefenceOrder focus, BattlePage page, int index)
    {
        Owner = focus;
        Root = item.gameObject;
        SelectedImage = item.Find("Selected").gameObject;
        SelectBG = item.Find("SelectBG").gameObject;
        Head = item.Find("Mask/Head").GetComponent<Image>();
        Hp = item.Find("Hp").GetComponent<Image>();
        Vim = item.Find("Vim").GetComponent<Image>();
        Index = index;
        item.Find("Click").GetComponent<Button>().onClick.AddListener(delegate
                        {
                            focus.Select(Index);
                            if (Data.mData.IsDead)
                                return;
                            BattleManager.Instance.DefendFocusOnFiring(Data.mData.UID);
                        }
        );
        item.Find("Selected/Click").GetComponent<Button>().onClick.AddListener(delegate
                        {
                            focus.FocusFight(Data);
                            focus.ShowFocus(false);
                            page.ShowOrderDefencePanels(true);
                        }
        );
        SetData(null);
    }

    public void SetData(Creature role)
    {
        Data = role;
        Refresh();
    }

    public void Selected(bool sel)
    {
        SelectedImage.SetActive(sel);
        SelectBG.SetActive(sel);
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
}

public partial class DefenceOrder : MonoBehaviour
{
    public List<DefenceItem> EnemyItems = new List<DefenceItem>();
    public BattlePage Page;

    public GameObject OrderRoot;

    public void Init(BattlePage page)
    {
        Page = page;
        EnemyItems.Add(new DefenceItem(this.Choose1.transform, this, page, 0));
        EnemyItems.Add(new DefenceItem(this.Choose2.transform, this, page, 1));
        EnemyItems.Add(new DefenceItem(this.Choose3.transform, this, page, 2));
    }

    void RoleDie(object[] param)
    {
        /*if(this.EnemyList.gameObject.activeSelf)
            SetDatas();*/
    }

    public void SetDatas()
    {
        foreach (var VARIABLE in EnemyItems)
        {
            VARIABLE.Root.SetActive(false);
        }
        var list = BattleEngine.Logic.BattleManager.Instance.ActorMgr.GetCamp(0);
        int index = 0;
        foreach (var VARIABLE in list)
        {
            if (!VARIABLE.mData.IsDead
                && VARIABLE.IsMain)
            {
                EnemyItems[index].SetData(VARIABLE);
                index++;
                if (index >= 3)
                    return;
            }
        }
    }

    public void Select(int index)
    {
        for (int i = 0; i < EnemyItems.Count; i++)
        {
            EnemyItems[i].Selected(i == index);
        }
    }

    public async void FocusFight(Creature role)
    {
        CloseFocusList();
        BattleEngine.Logic.BattleManager.Instance.AtkerFocusOnFiring(role.mData.UID);
        role.ShowSwitchTarget(true);
    }

    public void ShowFocus(bool vis)
    {
        this.gameObject.SetActive(vis);
        OrderRoot.gameObject.SetActive(false);
    }

    public bool IsFocusShow()
    {
        return this.gameObject.gameObject.activeSelf;
    }

    public void CloseFocusList()
    {
        //EnemyList.SetActive(false);
        this.gameObject.SetActive(false);
        CameraManager.Instance.FocusTarget = null;
    }

    public void ShowOrderRoot(bool vis)
    {
        OrderRoot.SetActive(vis);
    }
}