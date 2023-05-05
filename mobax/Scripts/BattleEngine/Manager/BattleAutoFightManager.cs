using BattleEngine.Logic;

namespace BattleEngine.Logic
{
    public class BattleAutoFightManager : Singleton<BattleAutoFightManager>
    {
        public AutoState AutoState = AutoState.None;
        private BattleData battleData;

        private CombatActorEntity FriendActor = null;
        public bool FriendIsUsed = false;

        public bool AutoUseFriendItem = false;

        private float AutoSuperSkillCD = 0;
        private float AutoSuperSkillCD_Def = 0;
        private float FriendCD = 0;

        public void CleanSSPCD()
        {
            AutoSuperSkillCD = 0;
        }

        public void CleanSSPCD_Def()
        {
            AutoSuperSkillCD_Def = 0;
        }

        public void InitManager(BattleData data)
        {
            battleData = data;
            foreach (var VARIABLE in data.atkActorLst)
            {
                if (VARIABLE.PosIndex == BattleConst.FriendPosIndex)
                {
                    FriendActor = VARIABLE;
                    FriendIsUsed = false;
                    break;
                }
            }

            FriendCD = 6f;
        }

        public void UpdateFrame(float delta)
        {
            if (battleData == null)
            {
                return;
            }
            switch (AutoState)
            {
                case AutoState.Auto:
                    CheckAutoSPSkill(delta);
                    CheckAutoFriends(delta);
                    break;
                default:
                    break;
            }
        }

        //检测大招释放
        public void CheckAutoSPSkill(float dt)
        {
            if (AutoSuperSkillCD > 0)
            {
                AutoSuperSkillCD -= dt;
            }
            if (AutoSuperSkillCD_Def > 0)
            {
                AutoSuperSkillCD_Def -= dt;
            }
            if (AutoSuperSkillCD <= 0)
            {
                for (int i = 0; i < battleData.atkActorLst.Count; i++)
                {
                    if (!BattleUtil.IsCanExecuteSPSkill(battleData.atkActorLst[i]))
                    {
                        continue;
                    }
                    AutoSuperSkillCD = 6f;
                    BattleLogicManager.Instance.ActorUseSPSkill(battleData.atkActorLst[i].UID);
                    return;
                }
            }
            if (AutoSuperSkillCD_Def <= 0)
            {
                for (int i = 0; i < battleData.defActorLst.Count; i++)
                {
                    if (!BattleUtil.IsCanExecuteSPSkill(battleData.defActorLst[i]))
                    {
                        continue;
                    }
                    AutoSuperSkillCD_Def = 6f;
                    BattleLogicManager.Instance.ActorUseSPSkill(battleData.defActorLst[i].UID);
                    return;
                }
            }
        }

        //检测自动使用助战角色
        public void CheckAutoFriends(float dt)
        {
            if (FriendIsUsed)
                return;
            if (FriendActor == null)
                return;
            if (FriendActor.IsDead)
                return;
            
            if (FriendCD > 0)
            {
                FriendCD -= dt;
            }
            
            if(FriendCD > 0)
                return;
            
            FriendIsUsed = true;
            /*
            string HeroID = "";
            for (int i = 0; i < battleData.atkActorLst.Count; i++)
            {
                if (battleData.atkActorLst[i].IsCantSelect)
                {
                    continue;
                }
                HeroID = battleData.atkActorLst[i].UID;
                break;
            }
            BattleLogicManager.Instance.SendFriendToBattleInputEvent(FriendActor.UID, HeroID);
            */
           
            BattleManager.Instance.ExecuteFreindToBattle();
        }

        //检测自动使用道具
        public void CheckAutoUseItem() { }
    }
}