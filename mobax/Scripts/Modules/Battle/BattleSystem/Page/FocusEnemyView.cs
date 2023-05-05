using System;
using BattleEngine.Logic;
using BattleEngine.View;
using UnityEngine;

public partial class FocusEnemyView : MonoBehaviour
{
        public Creature data;
        public static FocusEnemyView LastSelected = null;
        public static FocusEnemyView LastFocusItem = null;
        public static Focus focus;
        public static BattlePage page;
        public void SetData(Creature role)
        {
            data = role;
            this.SetActive(data != null);
            
            var info = StaticData.HeroTable.TryGet(role.ConfigID);
            if (info != null)
            {
                UiUtil.SetSpriteInBackground(HeadIcon, () => info.Head + ".png");
            }
            
            UpdateHp();
        }

        public void UpdateHp()
        {
            if(data == null)
                return;

            this.Hp.fillAmount = data.mData.CurrentHealth.Percent();
        }

        public void Update()
        {
            if (data == null)
                return;
        }

        public void HideSelect()
        {
            this.Selected.SetActive(false);
            this.Root.transform.localScale = Vector3.one;

            if (data != null)
            {
                data.ShowPreFocusTarget(false);
            }
        }
        

        public void ClickSelect()
        {
            if(LastSelected != null)
                LastSelected.HideSelect();
            this.Selected.SetActive(true);
            LastSelected = this;
         
            if (data.mData.IsDead)
                return;
            //Data.mData.Publish(new FocusPreTargetEvent() { targetUID = Data.mData.UID });
            var list = SceneObjectManager.Instance.GetAllPlayer();
            foreach (var VARIABLE in list)
            {
                if (VARIABLE.IsHero)
                    VARIABLE.mData.Publish(new FocusPreTargetEvent() {targetUID = data.mData.UID});
            }

            CameraManager.Instance.FocusTarget = data;

            //focus.CleanPreSelect();
            //page.PreFocus(data);
            
            BattleDataManager.Instance.TimeScale = 0.03f;
            this.Root.transform.localScale = Vector3.one * 1.2f;
            DamageManager.Instance.SetVisible(false);
            BattleSpecialEventManager.Instance.ShowDark(CameraManager.Instance.MainCamera, 0f);
            data.ShowPreFocusTarget(true);
            HudManager.Instance.SetVis(false);
        }

        public void ConfirmClick()
        {
            this.Selected.SetActive(false);
            focus.FocusFight(data);
            HeroItemView.HideDefence();
            Focus();
            
            BattleDataManager.Instance.TimeScale = 1f;
            LastSelected = null;
            
            BattleDataManager.Instance.UseItemByType(4);

            this.Root.transform.localScale = Vector3.one;
            DamageManager.Instance.SetVisible(true);
            BattleSpecialEventManager.Instance.CloseDark(CameraManager.Instance.MainCamera);
            data.ShowSwitchTarget(true);
            HudManager.Instance.SetVis(true);
        }
        
        public void Focus()
        {
            /*go.transform.SetAsLastSibling();
            focus.SetActive(true);
            this.BossStartingHead.localScale = Vector3.one * 1.7f;*/
            
            HideLastFocus();

            LastFocusItem = this;
            this.FocusTag.SetActive(true);
            BattleDataManager.Instance.TimeScale = 1f;
        }

        public static void HideLastFocus()
        {
            if (LastFocusItem != null)
            {
                LastFocusItem.FocusTag.SetActive(false);
                BattleDataManager.Instance.TimeScale = 1f;
            }
        }

        public static void HideLastSelect()
        {
            if (LastSelected != null)
            {
                LastSelected.HideSelect();
                LastSelected = null;
                BattleDataManager.Instance.TimeScale = 1f;
            }
        }

        public void NoFocus()
        {
            this.FocusTag.SetActive(false);
        }

        public Transform GetClickNode()
        {
            return Click.transform;
        }
        
        public Transform GetConfirmNode()
        {
            return FocusConfirm.transform;
        }
}