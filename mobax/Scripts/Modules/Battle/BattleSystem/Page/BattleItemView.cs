using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BattleItemData
{
        public int ID;
        public int type;
        public bool disable;
        public Action Action;
        public float remtime;
        public TacticRow info;
}
public partial class BattleItemView : MonoBehaviour
{
        public BattleItemData Data;

        public void SetData(BattleItemData data)
        {
                Data = data;
                if (data == null)
                {
                        ItemIcon.enabled = false;
                        return;
                }

                Data.info = StaticData.TacticTable.TryGet(data.ID);
                Refresh();
        }

        private void Refresh()
        { 
                if(Data == null)
                        return;
                
                var num = BattleDataManager.Instance.GetItemNum(Data.ID);
                this.ItemNum.text = num.ToString();
                UiUtil.SetSpriteInBackground(ItemIcon, () => Data.info.Icon + ".png");
                RefreshEnable();
        }

        public void RefreshNum()
        {
                if(Data == null)
                        return;  
                
                var num = BattleDataManager.Instance.GetItemNum(Data.ID);
                this.ItemNum.text = num.ToString();

                PlayUseAnim();
        }

        public void StartCD()
        {
                Data.remtime = Data.info.Cd / 1000f;
        }

        public void Update()
        {
                if(Data == null)
                        return;

                var rem = BattleDataManager.Instance.GetItemCD(Data.type);
                
                if(rem <= 0)
                        return;
                
                this.CD.fillAmount = rem * 1000f / Data.info.Cd;
        }

        public void ButtonClick()
        {
                if(Data == null)
                        return;
             BattleDataManager.Instance.ResetItemEnable(Data.type);
             RefreshEnable();
        }

        public void RefreshEnable()
        {
                if(Data == null)
                        return;
                
                bool enable = BattleDataManager.Instance.GetTypeEnableState(Data.type);
                this.Disable.SetActive(!enable);
        }

        public void PlayUseAnim()
        {
                var go = GameObject.Instantiate(ItemIcon, ItemIcon.transform.parent);
                go.transform.localPosition = ItemIcon.transform.localPosition;

                Sequence seq = DOTween.Sequence();
                seq.SetId($"bling_{transform.name}_{name}");
                seq.Append(go.transform.DOLocalMoveY(100f, 1f));
                seq.Insert(0.3f, go.transform.GetComponent<Image>().DOFade(0f, 0.7f));
                seq.AppendCallback(delegate
                {
                        GameObject.Destroy(go);
                });
        }
}