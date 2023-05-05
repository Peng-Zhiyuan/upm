namespace BattleEngine.View
{
    using Logic;
    using Neatly.Timer;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class BattleOperateManager : IUpdatable
    {
        private BattleData battleData;
        private BattleLogicManager battleLogicMgr;
        private Creature selectActorObject;
        public Creature SelectActorObject
        {
            get { return selectActorObject; }
        }
        private Creature dragActorObject;
        private Creature clickActorObject;
        private Vector3 mouseClick;
        private bool isBeginRenderLine = false;
        private bool isDragPlane = false;
        private bool isDragTime = true;
        private float dragTime = 0.0f;
        public bool isLocalAtker = true;
        private Vector3 navMeshPos;

        public System.Action<bool> ShowHp;

        private int currentUserCampID = 0;
        public int CurrentUserCampID
        {
            get { return currentUserCampID; }
        }

        public void SetCurrentTeamID(int id)
        {
            if (currentUserCampID != id)
                currentUserCampID = id;
        }

        public Action<bool, string, string> delegateIsSelectObj;
        public Action<bool, string> delegateIsShowHp;
        public Action<bool, string> delegateIsShowObjOutLight;
        public Action<string> delegateShowFocusOnFiring;
        public Action<bool, string> delegateIsShowTargetObj;
        public Action<bool> delegateHideRaycastTarget;

        public Action<bool> delegateIsShowRenderLine;
        public Action<Vector3, Vector3> delegateSetRenderLinePos;
        public Action<string> delegateSetRenderLineColor;

        public Action<string, List<BuffAbility>> delegateRefreshStateBuff;
        public Action<bool, string> delegateShowOrHideStateBuff;
        public Action<bool, string, string> delegateNormalShowOrHideHp;
        public Action<string, int> delegateReviveCountDown;
        public Action<bool> delegateBattleMask;
        public Action<string> delegateRefreshDisplacementSkill;

        public BattleOperateManager(BattleData _battleData, BattleLogicManager _battleLogic)
        {
            battleData = _battleData;
            battleLogicMgr = _battleLogic;
            currentUserCampID = BattleConst.ATKCampID;
            // InputManager.Instance.delegateMouseOnPress = MouseOnPress;
            // InputManager.Instance.delegateMouseOnDrag = MouseOnDrag;
            // InputManager.Instance.delegateMouseOnClick = MouseOnClick;
            // InputManager.Instance.delegateMouseOnRelease = MouseOnRelease;
        }

        public void Dispose()
        {
            ShowHp = null;
            delegateIsSelectObj = null;
            delegateIsShowHp = null;
            delegateIsShowObjOutLight = null;
            delegateShowFocusOnFiring = null;
            delegateHideRaycastTarget = null;
            delegateIsShowRenderLine = null;
            delegateSetRenderLinePos = null;
            delegateSetRenderLineColor = null;
            delegateRefreshStateBuff = null;
            delegateShowOrHideStateBuff = null;
            delegateNormalShowOrHideHp = null;
            delegateReviveCountDown = null;
            delegateIsShowTargetObj = null;
            delegateBattleMask = null;
            delegateRefreshDisplacementSkill = null;
            // InputManager.Instance.delegateMouseOnPress = null;
            // InputManager.Instance.delegateMouseOnDrag = null;
            // InputManager.Instance.delegateMouseOnClick = null;
            // InputManager.Instance.delegateMouseOnRelease = null;
        }

        public void BeginUpdata()
        {
            UpdateManager.Instance.Add(this);
            EventManager.Instance.AddListener<CombatActorEntity>("BattleRefreshBuff", (str) =>
                            {
                                List<BuffAbility> buffList = BattleControlUtil.GetCombatActorEntityForbidBuff(str);
                                //LuaHelper.CallLuaFunction("OnRefreshStateBuff", str.UID, buffList);
                            }
            );
        }

        public void EndUpdata()
        {
            UpdateManager.Instance.Remove(this);
            EventManager.Instance.RemoveListener<CombatActorEntity>("BattleRefreshBuff", (str) => { });
        }

#region 鼠标按下
        // public void MouseOnPress(GameObject avatar)
        // {
        //     isBeginRenderLine = false;
        //     RefreshDragTime();
        //     Creature actor = avatar.GetComponent<Creature>();
        //     if (actor != null
        //         && !actor.mData.IsCantSelect
        //         && actor.mData.isAtker == isLocalAtker)
        //     {
        //         SelectActor(actor);
        //     }
        // }
#endregion

#region 鼠标点击
        // public void MouseOnClick(GameObject avatar, Vector3 clickPos)
        // {
        //     isBeginRenderLine = false;
        //     ActorObject actor = avatar.GetComponent<ActorObject>();
        //     if (actor != null
        //         && actor.mData.IsCantSelect == false)
        //     {
        //         if (actor.mData.isAtker != isLocalAtker)
        //         {
        //             if (selectActorObject != null)
        //             {
        //                 BattleDataBridging.UnselectActor(selectActorObject, false);
        //                 LuaHelper.CallLuaFunction("OnIsSelectObj", false, selectActorObject.mData.UID);
        //                 selectActorObject = null;
        //             }
        //             if (clickActorObject != null)
        //             {
        //                 LuaHelper.CallLuaFunction("OnIsShowHp", false, clickActorObject.mData.UID);
        //                 clickActorObject = null;
        //             }
        //             FocusOnFiring(actor);
        //         }
        //     }
        //     else
        //     {
        //         if (selectActorObject != null
        //             && IsClickPlane(avatar))
        //         {
        //             MoveToPos(clickPos);
        //         }
        //     }
        // }
        //
        // private float lastFocusFiring = 0.0f;
        //
        // public void FocusOnFiring(ActorObject actor)
        // {
        //     if (Time.time - lastFocusFiring < 1.0f) return;
        //     lastFocusFiring = Time.time;
        //     clickActorObject = actor;
        //     BattleDataBridging.SelectActor(actor, true);
        //     LuaHelper.CallLuaFunction("OnShowFocusOnFiring", actor.mData.UID);
        //     LuaHelper.CallLuaFunction("OnIsShowHp", true, actor.mData.UID);
        //     battleLogicMgr.AtkerFocusOnFiring(currentUserCampID, actor.mData.UID);
        //     List<CombatActorEntity> lst = battleData.atkActorDic[currentUserCampID];
        //     for (int i = 0; i < lst.Count; i++)
        //     {
        //         if (!lst[i].IsCantSelect
        //             && lst[i].isSSPMoveCoolDown())
        //         {
        //             battleLogicMgr.ActorMoveSkill(lst[i].UID, actor.mData.UID);
        //         }
        //     }
        // }
#endregion

#region 鼠标拖拽
        // private bool dragSelect = true;
        //
        // public void MouseOnDrag(GameObject avatar, Vector3 clickPos, string dragMode)
        // {
        //     if (dragMode == "LineDrag")
        //     {
        //         ActorObject actor = avatar.GetComponent<ActorObject>();
        //         if (actor != null
        //             && !actor.mData.IsCantSelect
        //             && actor.mData.isAtker)
        //         {
        //             if (dragSelect)
        //             {
        //                 dragSelect = false;
        //                 SelectActor(actor);
        //             }
        //         }
        //     }
        //     if (selectActorObject != null)
        //     {
        //         if (selectActorObject.mData.IsCantSelect)
        //         {
        //             //选中角色死亡，取消选中
        //             isBeginRenderLine = false;
        //             ReEnableLogic();
        //             delegateHideRaycastTarget?.Invoke(false);
        //             selectActorObject.ShowOperateOutline(false);
        //             selectActorObject = null;
        //             BattleResManager.Instance.HideTargetPos();
        //             HideDragEffect();
        //             BattleDataBridging.ShowNavHitRange(false);
        //         }
        //         else
        //         {
        //             mouseClick = clickPos;
        //             isDragTime = true;
        //             isBeginRenderLine = true;
        //             ShowDragEffect();
        //             selectActorObject.ShowOperateOutline(true, 0);
        //             delegateHideRaycastTarget?.Invoke(true);
        //             if (dragMode == "LineDrag")
        //             {
        //                 isDragPlane = true;
        //                 DragPlane();
        //             }
        //             else
        //             {
        //                 ActorObject actor = avatar.GetComponent<ActorObject>();
        //                 if (actor != null)
        //                 {
        //                     if (actor.mData.IsCantSelect)
        //                     {
        //                         isDragPlane = true;
        //                         DragPlane();
        //                     }
        //                     else
        //                     {
        //                         isDragPlane = false;
        //                         if (selectActorObject.mData.battleItemInfo.GetHeroRow().unit_class != 2)
        //                         {
        //                             if (actor.mData.isAtker != isLocalAtker)
        //                             {
        //                                 UnSelectDragObj(true, actor);
        //                                 actor.ShowOperateOutline(true, 1);
        //                                 RendererControl.Instance.SetLineColor("#FF0000");
        //                             }
        //                             else DragPlane();
        //                         }
        //                         BattleResManager.Instance.HideTargetPos();
        //                         RendererControl.Instance.SetLinePos(selectActorObject.transform.localPosition, mouseClick);
        //                     }
        //                 }
        //                 else
        //                 {
        //                     if (IsClickPlane(avatar))
        //                     {
        //                         isDragPlane = true;
        //                         DragPlane();
        //                     }
        //                 }
        //             }
        //         }
        //     }
        //     if (isDragPlane && selectActorObject != null)
        //     {
        //         navMeshPos = selectActorObject.SampleNavPosition(mouseClick);
        //         BattleResManager.Instance.ShowTargetPos(navMeshPos);
        //         RendererControl.Instance.SetLinePos(selectActorObject.transform.localPosition, navMeshPos);
        //     }
        //     else
        //     {
        //         BattleResManager.Instance.HideTargetPos();
        //     }
        // }
        //
        // private void DragPlane()
        // {
        //     UnSelectDragObj(false);
        //     RendererControl.Instance.SetLineColor("#00FFFF");
        // }
        //
        // private void UnSelectDragObj(bool flag, ActorObject actor = null)
        // {
        //     if (dragActorObject != null
        //         && dragActorObject != selectActorObject)
        //     {
        //         dragActorObject.ShowOperateOutline(false);
        //         IsShowDragSelect(false);
        //         dragActorObject = null;
        //     }
        //     if (flag)
        //     {
        //         dragActorObject = actor;
        //         IsShowDragSelect(true);
        //     }
        // }
        //
        // private void IsShowDragSelect(bool flag)
        // {
        //     dragActorObject.IsShowSelectFx(flag);
        //     LuaHelper.CallLuaFunction("OnIsShowHp", flag, dragActorObject.mData.UID);
        // }
#endregion

#region 鼠标松开
        // public void MouseOnRelease(GameObject avatar, Vector3 clickPos, string releaseMode)
        // {
        //     dragSelect = true;
        //     if (selectActorObject != null)
        //     {
        //         ReEnableLogic();
        //         RefreshDragTime();
        //         isBeginRenderLine = false;
        //         delegateHideRaycastTarget?.Invoke(false);
        //         BattleResManager.Instance.HideTargetPos();
        //         HideDragEffect();
        //         selectActorObject.ShowOperateOutline(false);
        //         if (releaseMode == "LineRelease")
        //             MoveToPos(clickPos);
        //         else
        //         {
        //             ActorObject actor = avatar.GetComponent<ActorObject>();
        //             if (actor != null)
        //             {
        //                 if (actor.mData.IsCantSelect) //目标已经死亡
        //                     MoveToPos(clickPos);
        //                 else
        //                 {
        //                     if (actor.mData.isAtker != isLocalAtker)
        //                     {
        //                         //当前对象为敌方
        //                         if (selectActorObject.mData.battleItemInfo.GetHeroRow().unit_class != 2)
        //                         {
        //                             //选中的角色为DPS
        //                             MouseOnReleaseTarget(actor);
        //                             BattleDataBridging.SelectActor(actor, true);
        //                         }
        //                         else MoveToPos(clickPos);
        //                     }
        //                 }
        //             }
        //             else
        //             {
        //                 if (IsClickPlane(avatar))
        //                     MoveToPos(clickPos);
        //             }
        //         }
        //     }
        // }
        //
        // private void MouseOnReleaseTarget(ActorObject actor)
        // {
        //     battleLogicMgr.ResetTargetKey(selectActorObject.mData.UID, actor.mData.UID);
        //     if (SelectActorObject != null
        //         && SelectActorObject.mData.isSSPMoveCoolDown())
        //     {
        //         battleLogicMgr.ActorMoveSkill(selectActorObject.mData.UID, actor.mData.UID);
        //     }
        //     LuaHelper.CallLuaFunction("OnIsShowHp", false, actor.mData.UID);
        //     actor.ShowOperateOutline(false);
        // }
        //
        // private void MoveToPos(Vector3 pos)
        // {
        //     Vector3 navMeshPos = selectActorObject.SampleNavPosition(pos);
        //     battleLogicMgr.MoveActorToPos(selectActorObject.mData.UID, navMeshPos);
        //     ShowTargetPos(navMeshPos);
        // }
#endregion

        public void OnUpdate()
        {
            // if (isBeginRenderLine)
            // {
            //     if (selectActorObject != null)
            //     {
            //         if (selectActorObject.mData.IsCantSelect)
            //         {
            //             //选中角色死亡，取消选中
            //             isBeginRenderLine = false;
            //             ReEnableLogic();
            //             delegateHideRaycastTarget?.Invoke(false);
            //             selectActorObject.ShowOperateOutline(false);
            //             selectActorObject = null;
            //             BattleResManager.Instance.HideTargetPos();
            //             HideDragEffect();
            //         }
            //         else
            //         {
            //             ShowDragEffect();
            //             if (isDragPlane)
            //             {
            //                 navMeshPos = selectActorObject.SampleNavPosition(mouseClick);
            //                 BattleResManager.Instance.ShowTargetPos(navMeshPos);
            //                 RendererControl.Instance.SetLinePos(selectActorObject.transform.localPosition, navMeshPos);
            //             }
            //             else
            //             {
            //                 BattleResManager.Instance.HideTargetPos();
            //                 RendererControl.Instance.SetLinePos(selectActorObject.transform.localPosition, mouseClick);
            //             }
            //         }
            //     }
            //
            //     //划线计时
            //     if (isDragTime)
            //     {
            //         dragTime += Time.deltaTime;
            //         if (dragTime >= 0.25f)
            //         {
            //             RefreshDragTime();
            //             StopLogic();
            //         }
            //     }
            // }
        }
        //
        // private void RefreshDragTime()
        // {
        //     isDragTime = false;
        //     dragTime = 0.0f;
        // }
        //
        // public string GetSelectActorType()
        // {
        //     if (selectActorObject != null)
        //     {
        //         if (selectActorObject.mData.battleItemInfo.GetHeroRow().unit_class != 2)
        //             return "DPS";
        //         else
        //             return "Cure";
        //     }
        //     return null;
        // }
        //
        // public void SelectActor(Creature actor)
        // {
        //     if (selectActorObject != null)
        //     {
        //         BattleDataBridging.UnselectActor(selectActorObject, false);
        //         LuaHelper.CallLuaFunction("OnIsSelectObj", false, selectActorObject.mData.UID);
        //         selectActorObject = null;
        //     }
        //     selectActorObject = actor;
        //     BattleDataBridging.SelectActor(actor, false);
        //     LuaHelper.CallLuaFunction("OnIsSelectObj", true, actor.mData.UID);
        // }
        //
        // public void SelectHeroByUI(string id)
        // {
        //     SelectActor(BattleDataBridging.GetActor(id));
        // }
        //
        // private bool IsClickPlane(GameObject obj)
        // {
        //     if (obj.layer == 8)
        //         return true;
        //     else
        //         return false;
        // }

        // public void StopLogic(bool AllStop = false)
        // {
        //     if (NeatlyTimer.instance != null)
        //     {
        //         NeatlyTimer.instance.timeStop = AllStop;
        //     }
        //     if (AllStop)
        //     {
        //         Time.timeScale = 0;
        //     }
        //     else
        //     {
        //         Time.timeScale = 0.15f;
        //         //InputManager.Instance.BeginStopLogicTime();
        //     }
        // }
        //
        // public void ReEnableLogic()
        // {
        //     if (NeatlyTimer.instance != null)
        //     {
        //         NeatlyTimer.instance.timeStop = false;
        //     }
        //     Time.timeScale = BattleConst.BattleTimeScale;
        // }

#region Event
        // public void BtnClickActorParts(string _actorID, string _partName)
        // {
        //     Debug.Log("Click Monseter " + _actorID + "   PartName : " + _partName);
        //     battleLogicMgr.AttackPartsEvent(currentUserCampID, _actorID, _partName);
        // }
        //
        // public void ClearSelectObject()
        // {
        //     ReEnableLogic();
        //     RefreshDragTime();
        //     isBeginRenderLine = false;
        //     delegateHideRaycastTarget?.Invoke(false);
        //     //LuaHelper.CallLuaFunction("OnIsShowRenderLine", false);
        //     BattleResManager.Instance.HideTargetPos();
        //     if (selectActorObject != null)
        //     {
        //         selectActorObject.ShowOperateOutline(false);
        //         LuaHelper.CallLuaFunction("OnIsSelectObj", false, selectActorObject.mData.UID);
        //     }
        //     selectActorObject = null;
        //     if (TouchProxy.TMPIns != null)
        //     {
        //         TouchProxy.TMPIns.OnRelease();
        //     }
        //     HideDragEffect();
        // }
        //
        // public void OpenOperateMgr()
        // {
        //     LuaHelper.CallLuaFunction("OnBattleMask", false);
        //     if (TouchProxy.TMPIns != null)
        //     {
        //         TouchProxy.TMPIns.OpenTouch();
        //     }
        // }
        //
        // public void CloseOperateMgr()
        // {
        //     LuaHelper.CallLuaFunction("OnBattleMask", true);
        //     ClearSelectObject();
        //     if (TouchProxy.TMPIns != null)
        //     {
        //         TouchProxy.TMPIns.CloseTouch();
        //     }
        // }
#endregion

#region TargetEffect
        // GameObject targetPosFx = null;
        //
        // public async void CreateTargetPosFx()
        // {
        //     targetPosFx = await BattleResManager.Instance.CreatorFx("BattleOperator/Click_Feedback");
        //     HideTargetPos();
        // }
        //
        // /// <summary>
        // /// 改变目标点的位置
        // /// </summary>
        // public void ShowTargetPos(Vector3 pos)
        // {
        //     HideTargetPos();
        //     if (targetPosFx != null)
        //     {
        //         targetPosFx.SetActive(true);
        //         targetPosFx.transform.position = pos;
        //     }
        // }
        //
        // /// <summary>
        // /// 消除目标点
        // /// </summary>
        // public void HideTargetPos()
        // {
        //     if (targetPosFx != null)
        //     {
        //         targetPosFx.SetActive(false);
        //     }
        // }
#endregion

#region DragLine & DragRangMesh
        // public void ShowDragEffect()
        // {
        //     RendererControl.Instance.ShowRenderer();
        //     BattleDataBridging.ShowNavHitRange(true);
        // }
        //
        // public void HideDragEffect()
        // {
        //     RendererControl.Instance.HideRenderer();
        //     BattleDataBridging.ShowNavHitRange(false);
        // }
#endregion
    }
}