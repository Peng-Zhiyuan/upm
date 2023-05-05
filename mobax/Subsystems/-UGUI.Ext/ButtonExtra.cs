using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


/// <summary>
/// 这玩意儿必须在Button组件的下面才行  Move Down 
/// </summary>
public class ButtonExtra : MonoBehaviour, IPointerClickHandler
{
    public ButtonAction action;
    private Selectable _selectObj;

    private void OnEnable()
    {
        _selectObj = gameObject.GetComponent<Selectable>();
        if (_selectObj == null)
        {
            return;
        }

        if (_selectObj is Button button)
        {
            isCooldown = false;
            button.interactable = true;
            // ResetButtonClickPriority(button);
            // button.onClick.AddListener(OnButtonClicked);
        }
        else if (_selectObj is Toggle toggle)
        {
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }
        else
        {
            // 在这里加其他组件的支持
            // throw new Exception("Component not implement yet!");
        }
    }

    // void Awake()
    // {
    //     _selectObj = gameObject.GetComponent<Selectable>();
    //     if (_selectObj == null)
    //     {
    //         return;
    //     }
    //
    //     if (_selectObj is Button button)
    //     {
    //         isCooldown = false;
    //         button.interactable = true;
    //         // ResetButtonClickPriority(button);
    //         // button.onClick.AddListener(OnButtonClicked);
    //     }
    //     else if (_selectObj is Toggle toggle)
    //     {
    //         toggle.onValueChanged.AddListener(OnToggleValueChanged);
    //     }
    //     else
    //     {
    //         // 在这里加其他组件的支持
    //         // throw new Exception("Component not implement yet!");
    //     }
    // }

    void OnButtonClicked()
    {
        if (this.action == ButtonAction.Default)
        {
            WwiseEventManager.SendEvent(TransformTable.UiControls, "OnButtonClicked");
        }
        else if (this.action == ButtonAction.Confirm)
        {
            WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_confirm");
        }
        else if (this.action == ButtonAction.Cancel)
        {
            WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_cancel");
        }
        else if (this.action == ButtonAction.BtnStory)
        {
            WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_hallStory");
        }
        else if (this.action == ButtonAction.BtnBattle)
        {
            WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_hallBattle");
        }
        else if (this.action == ButtonAction.BtnShop)
        {
            WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_hallShop");
        }
        else if (this.action == ButtonAction.BtnMiniGame)
        {
            WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_hallMiniGame");
        }
        else if (this.action == ButtonAction.BtnPrivateChat)
        {
            WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_hallChat");
        }
    }

    void OnToggleValueChanged(bool value)
    {
        WwiseEventManager.SendEvent(TransformTable.UiControls, "OnToggleValueChanged");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        this.CheckButtonLimitClick();
        this.OnButtonClicked();
    }

    #region ---按钮连点屏蔽---

    [ShowInInspector] [LabelText("按钮连点限时")] [LabelWidth(100)]
    private float cooldownTime = 0.2f;

    [ShowInInspector] [LabelText("按钮是否锁定")] [LabelWidth(100)]
    private bool isCooldown = false;

    void CheckButtonLimitClick()
    {
        if (this == null || gameObject == null) return;
        _selectObj = gameObject.GetComponent<Selectable>();
        if (_selectObj == null)
        {
            return;
        }

        if (_selectObj is Button button)
        {
            if (!isCooldown)
            {
                if (!button.isActiveAndEnabled) return;
                // 执行按钮的点击事件
                isCooldown = true;
                button.interactable = false;
                StartCoroutine(ResetCooldown());
            }
            else
            {
                // ToastManager.ShowLocalize("common_clickCooldown");
            }
        }
    }

    private IEnumerator ResetCooldown()
    {
        if (this != null && gameObject != null)
        {
            _selectObj = gameObject.GetComponent<Selectable>();
            if (_selectObj != null)
            {
                if (_selectObj is Button button)
                {
                    yield return new WaitForSeconds(cooldownTime);
                    isCooldown = false;
                    button.interactable = true;
                    yield break;
                }
            }
        }
    }

    #endregion
}

public enum ButtonAction
{
    Default,
    Confirm,
    Cancel,

    [LabelText("主界面-故事按钮")] BtnStory,
    [LabelText("主界面-战斗按钮")] BtnBattle,
    [LabelText("主界面-商城按钮")] BtnShop,
    [LabelText("主界面-小游戏按钮")] BtnMiniGame,

    None,

    BtnPrivateChat
}

public enum ToggleAction
{
    None,

    [LabelText("主界面-大厅按钮")] BtnHall,
    [LabelText("主界面-仓库按钮")] BtnStore,
    [LabelText("主界面-角色按钮")] BtnCharacter,
    [LabelText("主界面-工坊按钮")] BtnWorkshop,
    [LabelText("主界面-召唤按钮")] BtnCall,
}