using BattleEngine.Logic;
using BattleEngine.View;
using UnityEngine;

public partial class DefFocusEnemyView : MonoBehaviour
{
        //private Creature data;
        public static DefFocusEnemyView LastSelected = null;
        public static BattlePage page;
        public static DefFocusEnemyView LastFocusItem = null;
        public string uid;
        public Creature data;
        public void SetData(Creature paramRole)
        {
            data = paramRole;
            this.SetActive(data != null);
            
            var info = StaticData.HeroTable.TryGet(data.ConfigID);
            if (info != null)
            {
                UiUtil.SetSpriteInBackground(Head, () => info.Head + ".png");
            }
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
            BattleDataManager.Instance.TimeScale = 0.1f;
            this.Root.transform.localScale = Vector3.one * 1.2f;
            
            DamageManager.Instance.SetVisible(false);
            HudManager.Instance.SetVis(false);
            BattleSpecialEventManager.Instance.ShowDark(CameraManager.Instance.MainCamera, 0f);
            data.ShowPreFocusTarget(true);

            //focus.CleanPreSelect();
           // page.PreFocus(role);
        }

        public void ConfirmClick()
        {
            this.Selected.SetActive(false);
            BattleManager.Instance.AtkerFocusOnFiring(data.mData.UID);
            data.ShowSwitchTarget(true);
            WwiseEventManager.SendEvent(TransformTable.Custom, "FocusDefenceLine");

            HeroItemView.HideDefence();
            Focus();
            BattleDataManager.Instance.TimeScale = 1f;
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
        }

        public void NoFocus()
        {
            this.FocusTag.SetActive(false);
        }
        
        public static void HideLastFocus()
        {
            if (LastFocusItem != null)
            {
                LastFocusItem.NoFocus();
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
        
}