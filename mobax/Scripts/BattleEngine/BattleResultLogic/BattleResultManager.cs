/* Created:Loki Date:2022-09-19*/

namespace BattleEngine.Logic
{
    using System;
    using System.Collections.Generic;

    public enum BATTLE_RESULT_TYPE
    {
        //计时
        TIMEEND = 0,
        //队伍状态是否全体死亡
        TEAM_DOOM = 1,
        //坚持波数
        WAVE_NUM = 2,
        //击杀特定对象
        KILL_TARGET = 3,
        //击杀数量
        KILL_NUM = 4,
        //守卫目标
        DEFEND_TARGET = 5,
        //坚持时间
        DEFEND_TIME = 6,
        //强制结束战斗(外部模式判断)
        OUTCHECKRESULT = 7,
    }

    public sealed class BattleResultCheckData
    {
        public BATTLE_RESULT_TYPE resultType = BATTLE_RESULT_TYPE.TIMEEND;

        /// <summary>
        /// TIMEEND:战斗时间(毫秒)
        /// TEAM_DOOM:BattleData
        /// WAVE_NUM:波数
        /// KILL_TARGET:击杀目标UID
        /// KILL_NUM:击杀数量
        /// DEFEND_TARGET:守卫目标UID
        /// DEFEND_TIME:守卫时间(毫秒)
        /// </summary>
        public Object Param { set; get; }
    }

    public sealed class BattleResultManager : Singleton<BattleResultManager>
    {
        private List<BattleResultLogic> resultList = new List<BattleResultLogic>();

        public void CreateResultData(List<BattleResultCheckData> _resultLst)
        {
            resultList.Clear();
            for (int i = 0; i < _resultLst.Count; i++)
            {
                var logic = CreateBattleResultLogic(_resultLst[i]);
                if (logic == null)
                {
                    continue;
                }
                resultList.Add(logic);
            }
        }

        public void AddCheckData(BattleResultCheckData data)
        {
            var logic = CreateBattleResultLogic(data);
            if (logic == null)
            {
                return;
            }
            resultList.Add(logic);
        }

        private BattleResultLogic CreateBattleResultLogic(BattleResultCheckData data)
        {
            switch (data.resultType)
            {
                case BATTLE_RESULT_TYPE.TIMEEND:
                    var timeResultLogic = new BattleTimeResultLogic();
                    timeResultLogic.Init(data.Param);
                    return timeResultLogic;
                case BATTLE_RESULT_TYPE.TEAM_DOOM:
                    var teamDoomLogic = new BattleTeamStateResultLogic();
                    teamDoomLogic.Init(data.Param);
                    return teamDoomLogic;
                case BATTLE_RESULT_TYPE.WAVE_NUM:
                    var waveNumLoigc = new BattleWaveResultLogic();
                    waveNumLoigc.Init(data.Param);
                    return waveNumLoigc;
                case BATTLE_RESULT_TYPE.KILL_TARGET:
                    var killTargetLogic = new BattleKillTargetResultLogic();
                    killTargetLogic.Init(data.Param);
                    return killTargetLogic;
                case BATTLE_RESULT_TYPE.KILL_NUM:
                    var killNumLogic = new BattleKillNumResultLogic();
                    killNumLogic.Init(data.Param);
                    return killNumLogic;
                case BATTLE_RESULT_TYPE.DEFEND_TARGET:
                    var defendTargetLogic = new BattleDefendTargetResultLogic();
                    defendTargetLogic.Init(data.Param);
                    return defendTargetLogic;
                case BATTLE_RESULT_TYPE.DEFEND_TIME:
                    var defendTimeLogic = new BattleDefendTimeResultLogic();
                    defendTimeLogic.Init(data.Param);
                    return defendTimeLogic;
                case BATTLE_RESULT_TYPE.OUTCHECKRESULT:
                    var outResultLogic = new BattleOutResultLogic();
                    outResultLogic.Init(data.Param);
                    return outResultLogic;
            }
            return null;
        }

        public bool CheckBattleEnd()
        {
            for (int i = 0; i < resultList.Count; i++)
            {
                if (resultList[i].CheckBattleEnd())
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsBattleWin
        {
            get
            {
                for (int i = 0; i < resultList.Count; i++)
                {
                    if (resultList[i].CheckBattleEnd())
                    {
                        return resultList[i].GetBattleResult();
                    }
                }
                return false;
            }
        }

        public void ClearData()
        {
            resultList.Clear();
        }

        public BattleResultLogic ResultMainCheckData()
        {
            if (resultList.Count > 0)
                return resultList[0];
            return null;
        }
    }
}