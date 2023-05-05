using BattleEngine.Logic;
using UnityEngine;
using UnityEngine.UI;

public class BattleItem
{
    public GameObject Root;
    public Image CD;
    public GameObject Select;
    public int ItemID;

    public Image HeadIcon;

    //public GameObject HeadIconSelected;
    public Image Click;
    public BattlePage Owner;
    public int Index;
    public float CDTime;
    public float CDRemTime;
    public Text CDTimeDes;
    public Image CanUse;

    public BattleItem(GameObject go, BattlePage owner, int index)
    {
        Root = go;
        this.Owner = owner;
        this.Index = index;
        HeadIcon = go.transform.Find("HeadMask/HeadIcon").GetComponent<Image>();
        Click = go.transform.Find("Click").GetComponent<Image>();
        CanUse = go.transform.Find("CanUse").GetComponent<Image>();
        go.name = index.ToString();
        CD = go.transform.Find("CD").GetComponent<Image>();
        CD.enabled = false;
        CDTimeDes = go.transform.Find("CDTime").GetComponent<Text>();
        CDTimeDes.text = "";
        ButtonUtil.SetClick(Click, () =>
                        {
                            if (ItemID == 0)
                                return;
                            /*foreach (var VARIABLE in Owner.battle_items)
                            {
                                if (VARIABLE == this)
                                {
                                    VARIABLE.SetCD(8f + BattleSystem.Core.BattleUtil.GetGlobalK(GlobalK.UseItemCD_24));
                                    
                                }
                                else
                                {
                                    VARIABLE.SetCD(BattleSystem.Core.BattleUtil.GetGlobalK(GlobalK.UseItemCD_24));
                                }
                            }*/
                            if (!BattleDataManager.Instance.UseItem(ItemID))
                            {
                                var des = LocalizationManager.Stuff.GetText("M10_defense_chat_004");
                                ToastManager.Show(des);
                                return;
                            }
                            SetCD((int)(BattleUtil.GetGlobalK(GlobalK.UseItemCD_24)));
                            var tacticRow = StaticData.TacticTable.TryGet(ItemID);
                            var role = SceneObjectManager.Instance.GetSelectPlayer();
                            BattleManager.Instance.SendSpendItemSkill(role.mData.UID, tacticRow.skillId);
                            BattleManager.Instance.BattleInfoRecord.SetItemUse(tacticRow.Id);
                            owner.UseItemTalk();
                            RefreshNum();
                        }
        );
    }

    public void RefreshNum()
    {
        if (ItemID == 0)
            return;
        CanUse.enabled = !BattleDataManager.Instance.ItemIsCanUse(ItemID);
    }

    public async void SetData(int id)
    {
        ItemID = id;
        if (ItemID == 0)
        {
            Root.transform.parent.SetActive(false);
        }
        else
        {
            Root.transform.parent.SetActive(true);
            var tacticRow = StaticData.TacticTable.TryGet(ItemID);
            var bucket = BucketManager.Stuff.Battle;
            var address = tacticRow.Icon + ".png";
            var sprite = await bucket.GetOrAquireAsync<Sprite>(address, true);
            HeadIcon.enabled = sprite != null;
            HeadIcon.sprite = sprite;
        }
        RefreshNum();
    }

    public void SetCD(float t)
    {
        CD.enabled = true;
        CDTime = t;
        CDRemTime = t;
        CD.fillAmount = 1f;
    }

    public void UpdateCDTime(float dt)
    {
        if (CDRemTime > 0)
        {
            CDRemTime -= dt;
            if (CDRemTime <= 0)
            {
                CD.enabled = false;
                CDTimeDes.text = "";
            }
            else
            {
                CD.fillAmount = CDRemTime / CDTime;
                CDTimeDes.text = Mathf.CeilToInt(CDRemTime) + "s";
            }
        }
    }
}