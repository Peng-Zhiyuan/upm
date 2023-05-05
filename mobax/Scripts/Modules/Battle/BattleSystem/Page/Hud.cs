using System.Collections.Generic;
using Modules.Battle.BattleSystem.Page;
using UnityEngine;
using UnityEngine.UI;

public class Hud
{
    public RecycledGameObject recycleGo;
    public GameObject go;
    public Transform nameroot;
    public GameObject name_back;
    public Text name;
    public GameObject hp_back;
    public Image hp;
    public Image fade;
    public Image vim;
    public Image warnigBar;
    //public Image head;
    public EmoteRoot emoteRoot;
    //public Image monsterHead;
    public Image job;
    //public Transform indicate;
    public GameObject weak;
    public Image element;
    public Text lv;

    public float durT;
    public Creature info;

    public Transform bufferRoot;

    public List<string> BufferCach = new List<string>();

    public BufferBar BufferBar;
    public GameObject BreakRoot;
    public Text BreakDamage;
    public Transform emojiRoot;

    public Hud(RecycledGameObject recycleGo, Transform root)
    {
        this.recycleGo = recycleGo;
        recycleGo.transform.SetParent(root);
        go = recycleGo.gameObject;
        go.transform.localScale = Vector3.one;
        nameroot = go.transform.Find("Transroot");
        name_back = go.transform.Find("Transroot/RoleName").gameObject;
        name = go.transform.Find("Transroot/RoleName/name").GetComponent<Text>();
        hp_back = go.transform.Find("Transroot/HpMpPanel").gameObject;
        hp = go.transform.Find("Transroot/HpMpPanel/background/hpbar").GetComponent<Image>();
        fade = go.transform.Find("Transroot/HpMpPanel/background/fadebar").GetComponent<Image>();
        vim = go.transform.Find("Transroot/HpMpPanel/background/vimbar").GetComponent<Image>();
        warnigBar = go.transform.Find("Transroot/HpMpPanel/background/warningbar").GetComponent<Image>();
        //head = go.transform.Find("Head").GetComponent<Image>();
        emoteRoot = go.transform.Find("EmoteRoot").GetComponent<EmoteRoot>();
        //monsterHead = go.transform.Find("MonsterHead").GetComponent<Image>();
        job = go.transform.Find("Transroot/HpMpPanel/background/Job").GetComponent<Image>();
        //indicate = go.transform.Find("Indicate");
        bufferRoot = go.transform.Find("Transroot/HpMpPanel/BufferRoot");
        //weak = go.transform.Find("Transroot/HpMpPanel/Weak").gameObject;
        element = go.transform.Find("Transroot/HpMpPanel/background/Element").GetComponent<Image>();
        lv = go.transform.Find("Transroot/HpMpPanel/background/Level").GetComponent<Text>();
        BufferBar = new BufferBar(bufferRoot, "HudBuffer.prefab");
        BreakRoot = go.transform.Find("Transroot/HpMpPanel/background/BreakRoot").gameObject;
        BreakDamage = go.transform.Find("Transroot/HpMpPanel/background/BreakRoot/BreakDamageValue").GetComponent<Text>();
        emojiRoot = go.transform.Find("EmojiRoot");
    }
    
    public void ShowBreakDamgeRoot()
    {
        //BreakRoot.gameObject.SetActive(info.mData.BreakDefComponent.IsBreak);
        UiUtil.SetActive(BreakRoot.gameObject, info.mData.BreakDefComponent.IsBreak);
        if(!info.mData.BreakDefComponent.IsBreak)
            return;

        BreakDamage.text = (int)(info.mData.BreakDefComponent.DamageParam * 100) + "%";
    }

    private float WarningTotalTime = 1f;
    private float WarningRemTime = 1f;
    public void ShowWarningBar(float time)
    {
        WarningTotalTime = time;
        WarningRemTime = time;
        //warnigBar.gameObject.SetActive(true);
        UiUtil.SetActive(warnigBar.gameObject, true);
    }

    public void Update()
    {
        bool vis = warnigBar.gameObject.activeSelf;
        var com = warnigBar.GetComponent<GameObjectExt>();
        if (com)
        {
            vis = com.IsVis;
        }

        if (vis)
        {
            if (WarningRemTime > 0)
            {
                WarningRemTime -= Time.deltaTime;
                if (WarningRemTime < 0)
                {
                    //warnigBar.gameObject.SetActive(false);
                    UiUtil.SetActive(warnigBar.gameObject, false);
                }
            }

            warnigBar.fillAmount = WarningRemTime / WarningTotalTime;
        }
    }

    public void ShowBreakDamageAnim()
    {
        ShowBreakDamgeRoot();
        
        UiUtil.ScaleBling(BreakDamage.transform, 0.7f, 0.5f, 0.2f, 0.2f, null, info.ID);
    }

    public async void ShowEmoji(string emoji, bool isleft)
    {
        var bucket = BucketManager.Stuff.Battle;
        var go = await bucket.GetOrAquireAsync<GameObject>(emoji, true);
        if (go != null && emojiRoot != null && emojiRoot.parent.gameObject.activeSelf)
        {
            go.transform.SetParent(emojiRoot);
            if (isleft)
            {
                go.transform.localPosition = new Vector3(-50, -80, 0);
            }
            else
            {
                go.transform.localPosition = new Vector3(50, -80, 0);
            }
            go.transform.localScale = Vector3.one;
        }
    }
    

    public void SetData(Creature info)
    {
        UiUtil.SetActive(warnigBar.gameObject, false);
        //warnigBar.gameObject.SetActive(false);
        this.info = info;
        this.go.name = info.mData.UID;
        BufferBar.SetVisible(true);

        RefreshBuffer();
        if (!info.mData.isAtker)
        {
            SelectChange(SceneObjectManager.Instance.CurSelectHero);
        }
        hp.fillAmount = 1;
        vim.fillAmount = 1;
        name.text = info.Name;
        ShowBreakDamgeRoot();
        /*if(info.IsEnemy)
        {
            name.color = new Color(1, 0, 0);
        }
        else if(info.IsHero)
        {
            name.color = new Color(1, 1, 1);
        }
        else
        {
            name.color = new Color(0, 1, 0);
        }*/
        //name_back.SetActive(false);
        UiUtil.SetActive(name_back.gameObject, false);
        //hp_back.SetActive(info.IsEnemy && !info.mData.battleItemInfo.isBoss);
        //hp_back.SetActive(info.IsEnemy || info.IsDefenceTarget());
        UiUtil.SetActive(hp_back.gameObject, info.IsEnemy || info.IsDefenceTarget());
        //head.gameObject.SetActive(false && info.IsHero);
        //monsterHead.gameObject.SetActive(false && info.IsEnemy);
        HeroRow hero = StaticData.HeroTable.TryGet(info.mData.ConfigID);
        if (hero == null)
            return;
        if (hero.Job != 0)
        {
            SetHeadJob(job, "Icon_occ" + hero.Job + ".png");
            job.enabled = true;
        }
        else
        {
            job.enabled = false;
        }
        lv.text = $"{info.mData.battleItemInfo.lv}";
        //nameroot.localPosition = Vector3.zero;
        if (info.mData.Weak == 0)
        {
            //element.SetActive(false);
            UiUtil.SetActive(element.gameObject, false);
        }
        else
        {
            //element.SetActive(true);
            UiUtil.SetActive(name_back.gameObject, true);
            SetHeadJob(element, "element_" + info.mData.Weak + ".png");
        }
        
    }

    async void SetHeadJob(Image image, string address)
    {
        var bucket = BucketManager.Stuff.Battle;
        var sprite2 = await bucket.GetOrAquireAsync<Sprite>(address, true);
        image.sprite = sprite2;
        image.enabled = sprite2 != null;
    }

    public void UpdateHpMp()
    {
        hp.fillAmount = info.mData.CurrentHealth.Percent();
        vim.fillAmount = info.mData.CurrentVim.Percent();
        durT = 5;
        SetVisible(true);
        
        HudUtil.DoHpFade(info, fade, 1f, "hud");
    }

    public void Destroy()
    {
        BufferBar.SetData(null);
        if (recycleGo != null)
            recycleGo.Recycle();
    }

    public void SetVisible(bool vis)
    {
        /*if (nameroot.gameObject.activeSelf != vis)
            nameroot.SetActive(vis && (!info.mData.isAtker || info.IsDefenceTarget()) && Battle.Instance.BattleStarted);*/
        
        UiUtil.SetActive(nameroot.gameObject, vis && (!info.mData.isAtker || info.IsDefenceTarget()) && Battle.Instance.BattleStarted);
    }

    public void RefreshBuffer()
    {
        BufferBar.SetData(this.info.mData);
    }

    public void SelectChange(Creature role)
    {
        if (info == null)
            return;
        if (role == null)
            return;

        //weak.SetActive(role.mData.isAnitJob(info.mData.Job));
        if (role.mData.targetKey == info.mData.UID)
        {
            this.nameroot.localScale = Vector3.one * 1.4f;
        }
        else
        {
            this.nameroot.localScale = Vector3.one;
        }
    }
}