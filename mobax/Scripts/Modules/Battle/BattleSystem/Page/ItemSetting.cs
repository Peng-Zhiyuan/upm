using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Debug = behaviac.Debug;

public class item_info
{
        public int ConfigID;
        public GameObject go;
        public Text num;
        public Image icon;
        public GameObject disable;
        public Button click;
        public bool IsFirst;
}

public class ItemSetting : MonoBehaviour
{
        public Transform parent;
        public GameObject prefab;
        public List<item_info> Items = new List<item_info>();

        public Image bindIcon;
        public Text bindNum;
        public GameObject bindDisable;

        private int DefaultID = 0;

        public void Init(List<int> datas, Action<int, bool> call, int defalutID, bool enable)
        {
                //GameEventCenter.Broadcast();
                DefaultID = defalutID;
                bool first = true;
                foreach (var VARIABLE in datas)
                {
                        int configID = VARIABLE;
                        var itemRow = StaticData.TacticTable.TryGet(configID);
                        var go = GameObject.Instantiate(prefab, parent);
                        go.transform.localScale = Vector3.one;

                        item_info item = new item_info();
                        item.ConfigID = configID;
                        item.go = go;
                        item.num = go.transform.Find("Number").GetComponent<Text>();
                        item.icon = go.transform.Find("BattleCommonBtn/Icon").GetComponent<Image>();
                        item.disable = go.transform.Find("BattleCommonBtn/BattlePropdisable").gameObject;
                        item.click = go.transform.Find("BattleCommonBtn").GetComponent<Button>();
                        item.IsFirst = first;
                        
                        if (itemRow == null)
                        {
                                Debug.LogError("config = " + configID);
                                item.icon.SetActive(false);
                                item.disable.SetActive(true);
                                item.num.SetActive(false);
                                continue;
                        }

                        item.icon.SetActive(true);
                        item.num.SetActive(true);
                        UiUtil.SetSpriteInBackground(item.icon, () => itemRow.Icon + ".png");
                        item.disable.SetActive(first);

                        ButtonUtil.SetClick(item.click, () =>
                        {
                                call.Invoke(configID, !item.IsFirst);
                                DefaultID = configID;
                                RefreshBindData(item);
                        });

                        if (first)
                        {
                                first = false;    
                        }
                                

                        Items.Add(item);

                        if (!item.IsFirst && defalutID == configID)
                        {
                                TimerMgr.Instance.BattleSchedulerTimerDelay(1f, delegate { RefreshBindData(item, enable); });
                        }
                }

                if (Items.Count == 0)
                {
                        bindIcon.SetActive(false);
                        bindNum.text = "";
                        bindDisable.SetActive(true); 
                }

                RefreshItemsNum();
        }

        public void RefreshItemsNum()
        {
                foreach (var VARIABLE in Items)
                {
                        VARIABLE.num.text = BattleDataManager.Instance.GetItemNum(VARIABLE.ConfigID).ToString();
                }

        }

        public void RefreshBindData(item_info info, bool isEnable = true)
        {
                bindIcon.sprite = info.icon.sprite;
                bindIcon.SetActive(true);
                var num = BattleDataManager.Instance.GetItemNum(info.ConfigID);
                bindNum.text = num.ToString();
                bindDisable.SetActive(info.IsFirst ||  !isEnable);

                if (num <= 0)
                {
                        UiUtil.SetGrey(bindIcon);
                }
                else
                {
                        bindIcon.material = null;
                }
        }

        public void RefreshData()
        {
                foreach (var VARIABLE in Items)
                {
                        if (VARIABLE.ConfigID == DefaultID && !VARIABLE.IsFirst)
                        {
                                RefreshBindData(VARIABLE);
                                break;
                        }
                } 
        }

        public void PlayUseAnim()
        {
                var go = GameObject.Instantiate(bindIcon, bindIcon.transform.parent);
                go.transform.localPosition = bindIcon.transform.localPosition;

                Sequence seq = DOTween.Sequence();
                seq.SetId($"bling_{transform.name}_{name}");
                seq.Append(go.transform.DOLocalMoveY(100f, 1f));
                seq.Insert(0.3f, go.transform.GetComponent<Image>().DOFade(0f, 0.7f));
                seq.AppendCallback(delegate
                {
                        GameObject.Destroy(go);
                });
        }

        public bool IsCanUse()
        {
                return BattleDataManager.Instance.GetItemNum(DefaultID) > 0;
        }
}