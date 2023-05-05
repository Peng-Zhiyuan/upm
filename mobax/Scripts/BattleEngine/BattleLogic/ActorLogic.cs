namespace BattleEngine.Logic
{
    using System.Collections.Generic;

    public class ActorLogic
    {
        private BattleData battleData;
        private bool isWin = false;
        public bool IsWin
        {
            get { return isWin; }
        }

        public ActorLogic(BattleData _battleData)
        {
            battleData = _battleData;
        }

        private List<CombatActorEntity> actorList = new List<CombatActorEntity>();
        public void DoLogic(int currentFrame)
        {
            battleData.ClearDistanceData();
            battleData.ClearOccupyData();
            actorList = battleData.allActorLst;
            for (int i = 0; i < actorList.Count; i++)
            {
                actorList[i].OnUpdate(currentFrame);
            }
        }

        //设置队长  按照队列顺序设置队长
        private void SetTeamCommander(List<CombatActorEntity> _actorList)
        {
            CombatActorEntity commander = null;
            for (int i = 0; i < _actorList.Count; i++)
            {
                if (_actorList[i] == null
                    || _actorList[i].CurrentHealth.Value <= 0)
                {
                    continue;
                }
                commander = _actorList[i];
                break;
            }
            if (commander != null)
            {
                for (int i = 0; i < _actorList.Count; i++)
                {
                    if (_actorList[i] == null
                        || _actorList[i].CurrentHealth.Value <= 0)
                    {
                        continue;
                    }
                    _actorList[i].CommanderID = commander.UID;
                }
            }
        }

        public CombatActorEntity CalculateTeamCommander(int teamID)
        {
            List<CombatActorEntity> lst = new List<CombatActorEntity>();
            if (battleData.atkActorDic.ContainsKey(teamID))
            {
                lst = battleData.atkActorDic[teamID];
            }
            if (battleData.defActorDic.ContainsKey(teamID))
            {
                lst = battleData.defActorDic[teamID];
            }
            SetTeamCommander(lst);
            return GetTeamCommander(teamID);
        }

        public CombatActorEntity GetTeamCommander(int teamID)
        {
            if (battleData.atkActorDic.ContainsKey(teamID))
            {
                List<CombatActorEntity> lst = battleData.atkActorDic[teamID];
                for (int i = 0; i < lst.Count; i++)
                {
                    if (lst[i].isLeader)
                    {
                        return lst[i];
                    }
                }
            }
            if (battleData.defActorDic.ContainsKey(teamID))
            {
                List<CombatActorEntity> lst = battleData.defActorDic[teamID];
                for (int i = 0; i < lst.Count; i++)
                {
                    if (lst[i].isLeader)
                    {
                        return lst[i];
                    }
                }
            }
            return null;
        }

        public List<CombatActorEntity> GetTeamActorLst(int teamID)
        {
            List<CombatActorEntity> lst = new List<CombatActorEntity>();
            if (battleData.atkActorDic.ContainsKey(teamID))
            {
                lst = battleData.atkActorDic[teamID];
            }
            if (battleData.defActorDic.ContainsKey(teamID))
            {
                lst = battleData.defActorDic[teamID];
            }
            return lst;
        }
    }
}