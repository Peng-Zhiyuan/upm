using System.Collections.Generic;
using System.Linq;
using BattleEngine.Logic;
using BattleEngine.View;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class BreakItem
{
    public GameObject go;
    public Text TimeShow;
    public string uid;
    public float durTime = 0;
    public bool enable = false;
    public Image CD;
    public BreakDefUI Owner;

    public BreakItem(GameObject go, BreakDefUI owner = null)
    {
        this.go = go;
        TimeShow = go.transform.Find("Time").GetComponent<Text>();
        CD = go.transform.Find("CD").GetComponent<Image>();
        ButtonUtil.SetClick(go, delegate()
                        {
                            var role = SceneObjectManager.Instance.Find(this.uid);
                            if (role != null)
                            {
                                //role.mData.Publish(new BreakDefQteEvent());
                            }
                            End();
                        }
        );
        End();
    }

    public void SetData(string _uid)
    {
        enable = true;
        durTime = 3f;
        this.uid = _uid;
        go.SetActive(true);
    }

    public void End()
    {
        go.SetActive(false);
        enable = false;
    }

    public void Update()
    {
        if (!enable)
            return;
        if (durTime > 0)
        {
            durTime -= Time.deltaTime;
            if (durTime <= 0)
            {
                End();
            }
            CD.fillAmount = durTime / 3f;
            //TimeShow.text = string.Format("{%.1f", durTime);
        }
    }
}

public partial class BreakDefUI
{
    public List<BreakItem> Items = new List<BreakItem>();
    private GameObject[] QteBtns = new GameObject[3];
    public Dictionary<Creature, float> AllBreakers = new Dictionary<Creature, float>();
    public float durTime = 3f;
    public GameObject BreakTips;

    public Transform GetQTE()
    {
        return QteBtn1.transform;
    }

    void Awake()
    {
        GameEventCenter.AddListener(GameEvent.BreakDef, this, this.BreakDefHandler);
    }

    public void QTEClick()
    {
        CheckOtherBreak();
        this.BreakLoop.SetActive(false);
        if (AllBreakers.Count > 0)
        {
            this.BreakEnd.SetActive(false);
            this.BreakEnd.SetActive(true);
            int count = 0;
            foreach (var VARIABLE in AllBreakers)
            {
                if (!VARIABLE.Key.mData.IsDead)
                    count++;
            }
            if (count > 0)
                GameEventCenter.Broadcast(GameEvent.BreakQtePress, count);
        }
        foreach (var VARIABLE in AllBreakers)
        {
            VARIABLE.Key.mData.Publish(new BreakDefQteEvent());
        }
                            
        this.QteBtn1.SetActive(false);
        BreakTips.SetActive(false);
        BattleSpecialEventManager.Instance.LowPlay();
        BattleManager.Instance.BattleInfoRecord.AddBreakNum();
        if (AllBreakers.Count <= 0)
            return;
                            
        AllBreakers.Clear();

        BattleDataManager.Instance.UseItemByType(1);
    }
    

    public bool IsBreakUIShow()
    {
        return QteBtn1.gameObject.activeSelf;
    }

    private void CheckOtherBreak()
    {
        var list = SceneObjectManager.Instance.actMgr.GetCamp(1);
        foreach (var VARIABLE in list)
        {
            if (!VARIABLE.mData.IsDead
                && !VARIABLE.mData.BreakDefComponent.IsBreak
                && VARIABLE.mData.BreakDefComponent.CanBreak())
            {
                VARIABLE.mData.BreakDefComponent.TriggerBreakImmediate();
            }
        }
    }

    private void BreakDefHandler(object[] data)
    {
        var uid = data[0] as string;
        var role = SceneObjectManager.Instance.Find(uid);
        if (role == null)
            return;
        if (AllBreakers.ContainsKey(role))
            return;
        AllBreakers.Add(role, 2);
        durTime = 3f;
        this.QteBtn1.SetActive(true);
        this.BreakStart.SetActive(true);
        TimerMgr.Instance.BattleSchedulerTimerDelay(0.5f, delegate
                        {
                            this.BreakStart.SetActive(false);
                            this.BreakLoop.SetActive(AllBreakers.Count > 0);
                        }
        );
        BreakTips.SetActive(Battle.Instance.BreakItemUse);
    }

    public void OnDestroy()
    {
        GameEventCenter.RemoveListener(GameEvent.BreakDef, this);
    }

    public void Update()
    {
        /*foreach (var VARIABLE in AllBreakers)
        {
            if (VARIABLE.Value > 0)
            {
                AllBreakers[VARIABLE.Key] = VARIABLE.Value - Time.deltaTime;
            }
        }*/
        for (int i = 0; i < AllBreakers.Count; i++)
        {
            var item = AllBreakers.ElementAt(i);
            if (item.Value > 0)
            {
                AllBreakers[item.Key] = item.Value - Time.deltaTime;
            }
            if (item.Key.mData.IsDead
                || item.Value <= 0)
            {
                AllBreakers.Remove(item.Key);
            }
            this.QteBtn1.gameObject.SetActive(AllBreakers.Count > 0);
            if (this.AllBreakers.Count == 0)
            {
                this.BreakLoop.SetActive(false);
                this.BreakTips.SetActive(false);
            }
        }
        if (durTime > 0)
        {
            durTime -= Time.deltaTime;
            if (durTime <= 0) { }
            CD.fillAmount = durTime / 3f;
            //TimeShow.text = string.Format("{%.1f", durTime);
        }
    }
}