namespace BattleEngine.Logic
{
    using UnityEngine;

    public sealed class BattleInputOperaterData : IBattleEventData
    {
        public string eventName = null;
        public string originID = null;
        public int targetTeamKey;
        public string targetID = null;
        public Vector3 targetPos;
        public string parentNode = null;
        public int skillID = 0;
        public SKILL_BREAK_CAUSE SkillBreakCauseType = SKILL_BREAK_CAUSE.None;

        public string GetEventName()
        {
            return this.eventName;
        }

        /// <summary>
        /// 集火机制
        /// </summary>
        /// <param name="targetID">目标ID</param>
        /// <param name="selectTeamKey">集火队伍ID</param>
        public void InitAtkFocusOnFiringEvent(string targetID, int selectTeamKey)
        {
            this.eventName = "AtkerFocusOnFiringEvent";
            this.targetID = targetID;
            this.targetTeamKey = selectTeamKey;
            this.SkillBreakCauseType = SKILL_BREAK_CAUSE.Focus;
        }

        /// <summary>
        /// 保护机制
        /// </summary>
        /// <param name="friendUID">需要保护的人</param>
        public void InitDefFocusOnFiringEvent(string friendUID)
        {
            this.eventName = "DefFocusOnFiringEvent";
            this.targetID = friendUID;
            this.SkillBreakCauseType = SKILL_BREAK_CAUSE.Focus;
        }

        /// <summary>
        /// 重置目标对象
        /// </summary>
        /// <param name="id">源ID</param>
        /// <param name="targetKey">目标ID</param>
        public void InitResetTargetKeyEvent(string id, string targetKey)
        {
            this.eventName = "ResetTargetKeyEvent";
            this.originID = id;
            this.targetID = targetKey;
        }

        /// <summary>
        /// 治疗对象
        /// </summary>
        /// <param name="targetID"></param>
        /// <param name="selectTeamKey"></param>
        public void InitCureHurtEvent(string targetID, int selectTeamKey)
        {
            this.eventName = "CureHurtEvent";
            this.targetID = targetID;
            this.targetTeamKey = selectTeamKey;
            this.SkillBreakCauseType = SKILL_BREAK_CAUSE.ManualAttack;
        }

        /// <summary>
        /// 指定位移
        /// </summary>
        /// <param name="targetID">目标ID</param>
        /// <param name="targetPos">目标位置</param>
        public void InitMoveToPosEvent(string targetID, Vector3 targetPos)
        {
            this.eventName = "MoveToPosEvent";
            this.targetID = targetID;
            this.targetPos = targetPos;
            this.SkillBreakCauseType = SKILL_BREAK_CAUSE.ManualMove;
        }

        /// <summary>
        /// 释放大招
        /// </summary>
        /// <param name="targetID">目标ID</param>
        public void InitApplySkillEvent(string targetID)
        {
            this.eventName = "ApplySkillEvent";
            this.targetID = targetID;
            this.SkillBreakCauseType = SKILL_BREAK_CAUSE.UltraSkill;
        }

        /// <summary>
        /// 触发移动技
        /// </summary>
        /// <param name="originID">释放技能者</param>
        /// <param name="targetID">目标ID</param>
        public void InitApplyMoveSkillEvent(string originID, string targetID)
        {
            this.eventName = "ApplyMoveSkillEvent";
            this.originID = originID;
            this.targetID = targetID;
            this.targetPos = Vector3.zero;
            this.SkillBreakCauseType = SKILL_BREAK_CAUSE.MoveAttack;
        }

        /// <summary>
        /// 触发移动技
        /// </summary>
        /// <param name="originID">释放技能者</param>
        /// <param name="targetPos">目标坐标</param>
        public void InitApplyMoveSkillEvent(string originID, Vector3 targetPos)
        {
            this.eventName = "ApplyMoveSkillEvent";
            this.originID = originID;
            this.targetID = "";
            this.targetPos = targetPos;
            this.SkillBreakCauseType = SKILL_BREAK_CAUSE.MoveAttack;
        }

        /// <summary>
        /// 使用道具
        /// </summary>
        /// <param name="originID">使用者</param>
        /// <param name="targetID">目标</param>
        /// <param name="skillID">技能ID</param>
        public void InitApplyItemSkillEvent(string originID, string targetID, int skillID)
        {
            this.eventName = "ApplyItemSkillEvent";
            this.originID = originID;
            this.targetID = targetID;
            this.skillID = skillID;
            this.SkillBreakCauseType = SKILL_BREAK_CAUSE.None;
        }

        /// <summary>
        /// 集火部件
        /// </summary>
        /// <param name="selectTeamKey">队伍ID</param>
        /// <param name="targetID">目标ID</param>
        /// <param name="parentNode">部件ID</param>
        public void InitAttackPartsEvent(int selectTeamKey, string targetID, string parentNode)
        {
            this.eventName = "AttackPartsEvent";
            this.targetID = targetID;
            this.targetTeamKey = selectTeamKey;
            this.parentNode = parentNode;
            this.SkillBreakCauseType = SKILL_BREAK_CAUSE.ManualAttack;
        }

        /// <summary>
        /// 替补上阵
        /// </summary>
        /// <param name="leaveUID">下场英雄UID</param>
        /// <param name="joinUID">上场英雄UID</param>
        public void InitSwitchTurnOnEvent(string leaveUID, string joinUID, Vector3 targetPos)
        {
            this.eventName = "ApplyTurnOnEvent";
            this.originID = leaveUID;
            this.targetID = joinUID;
            this.targetPos = targetPos;
            this.SkillBreakCauseType = SKILL_BREAK_CAUSE.Focus;
        }

        /// <summary>
        /// 替补下场
        /// </summary>
        /// <param name="leaveUID">下场英雄UID</param>
        public void InitSwitchTurnOffEvent(string leaveUID)
        {
            this.eventName = "ApplyTurnOffEvent";
            this.originID = leaveUID;
            this.SkillBreakCauseType = SKILL_BREAK_CAUSE.Focus;
        }

        /// <summary>
        /// 好友助战
        /// </summary>
        /// <param name="friendUID">好友英雄UID</param>
        /// <param name="chooseUID">当前玩家选择英雄</param>
        public void InitFriendToBattleEvent(string _friendUID, string _chooseUID)
        {
            this.eventName = "ApplyFriendToBattleEvent";
            this.originID = _chooseUID;
            this.targetID = _friendUID;
        }

        /// <summary>
        /// 好友退出助战
        /// </summary>
        /// <param name="friendUID">好友英雄UID</param>
        public void InitFriendQuitBattleEvent(string _friendUID)
        {
            this.eventName = "ApplyFriendQuitBattleEvent";
            this.originID = _friendUID;
        }
    }
}