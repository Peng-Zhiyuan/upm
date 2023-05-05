/* Created:Loki Date:2022-11-25*/

namespace BattleEngine.Logic
{
    using System.Collections.Generic;

    public sealed class BattleSenceRecord
    {
        //记录大招使用情况
        private Dictionary<string, int> heroSPSkillDic = new Dictionary<string, int>();

        public void SetHeroSPSkillUse(string _id, int useNum = 1)
        {
            if (!BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                return;
            }
            if (heroSPSkillDic.ContainsKey(_id))
            {
                heroSPSkillDic[_id] += useNum;
            }
            else
            {
                heroSPSkillDic.Add(_id, useNum);
            }
        }

        public Dictionary<string, int> PlayerSPSkillDic
        {
            get { return heroSPSkillDic; }
        }

        public string GetPlaySPSkillToStr()
        {
            if (!BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                return "";
            }
            StrBuild.Instance.ClearSB();
            var data = heroSPSkillDic.GetEnumerator();
            while (data.MoveNext())
            {
                StrBuild.Instance.Append(data.Current.Key);
                StrBuild.Instance.Append(",");
                StrBuild.Instance.Append(data.Current.Value);
                StrBuild.Instance.Append(";");
            }
            return StrBuild.Instance.GetString();
        }

        //记录道具使用情况
        private Dictionary<int, int> itemUseDic = new Dictionary<int, int>();

        public void SetItemUse(int itemID, int useNum = 1)
        {
            if (!BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                return;
            }
            if (itemUseDic.ContainsKey(itemID))
            {
                itemUseDic[itemID] += useNum;
            }
            else
            {
                itemUseDic.Add(itemID, useNum);
            }
        }

        public Dictionary<int, int> ItemUseDic
        {
            get { return itemUseDic; }
        }

        public string GetItemUseToStr()
        {
            if (!BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                return "";
            }
            StrBuild.Instance.ClearSB();
            var data = itemUseDic.GetEnumerator();
            while (data.MoveNext())
            {
                StrBuild.Instance.Append(data.Current.Key);
                StrBuild.Instance.Append(",");
                StrBuild.Instance.Append(data.Current.Value);
                StrBuild.Instance.Append(";");
            }
            return StrBuild.Instance.GetString();
        }

        private int focusNum;
        private int defenceNum;
        private int breakNum;
        private int switchCameraNum = 0;

        public void AddForcusNum()
        {
            focusNum += 1;
        }

        public void AddDefenceNum()
        {
            defenceNum += 1;
        }

        public void AddBreakNum()
        {
            breakNum += 1;
        }

        /// <summary>
        /// 切换镜头
        /// </summary>
        public void AddSwitchCameraNum()
        {
            switchCameraNum += 1;
        }

        public int GetForceNum
        {
            get { return focusNum; }
        }
        public int GetDefenceNum
        {
            get { return defenceNum; }
        }
        public int GetBreakNum
        {
            get { return breakNum; }
        }
        public int GetSwitchCameraNum
        {
            get { return switchCameraNum; }
        }

        public bool IsDonate = false;
        public int helpHeroID = -1;
        public bool IsUseHelpHero = false;
        public bool isAutoBattle = false;
        public int battleTime = 0;
        public bool isDonate = false;

        public void InitData()
        {
            heroSPSkillDic.Clear();
            itemUseDic.Clear();
            focusNum = 0;
            defenceNum = 0;
            breakNum = 0;
            IsDonate = false;
            helpHeroID = -1;
            IsUseHelpHero = false;
            isAutoBattle = false;
            switchCameraNum = 0;
            battleTime = 0;
            isDonate = false;
        }

        public void CheckHelpHeroID(BattleData _battleData)
        {
            if (!BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                return;
            }
            List<CombatActorEntity> actors = _battleData.atkActorLst;
            for (int i = 0; i < actors.Count; i++)
            {
                if (actors[i].CurrentHealth.Value <= 0)
                {
                    continue;
                }
                if (actors[i].CurrentLifeState == ACTOR_LIFE_STATE.Assist)
                {
                    helpHeroID = actors[i].ConfigID;
                }
            }
        }

        public string GetTrackInstruction()
        {
            if (!BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                return "";
            }
            StrBuild.Instance.ClearSB();
            StrBuild.Instance.Append("focus", ",", focusNum.ToString(), ";");
            StrBuild.Instance.Append("defence", ",", defenceNum.ToString(), ";");
            return StrBuild.Instance.GetString();
        }

        public string GetTrackOther()
        {
            if (!BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                return "";
            }
            StrBuild.Instance.ClearSB();
            StrBuild.Instance.Append("helphero", ",", helpHeroID.ToString(), ";");
            StrBuild.Instance.Append("isuse", ",", IsUseHelpHero ? "1" : "0", ";");
            StrBuild.Instance.Append("breaknum", ",", breakNum.ToString(), ";");
            StrBuild.Instance.Append("auto", ",", isAutoBattle ? "1" : "0", ";");
            StrBuild.Instance.Append("herocamera", ",", switchCameraNum.ToString(), ";");
            return StrBuild.Instance.GetString();
        }

        public Dictionary<string, string> GetTrackDic()
        {
            if (!BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                return new Dictionary<string, string>();
            }
            Dictionary<string, string> sendData = new Dictionary<string, string>();
            sendData.Add("item", GetItemUseToStr());
            sendData.Add("hero", GetPlaySPSkillToStr());
            sendData.Add("instructions", GetTrackInstruction());
            sendData.Add("other", GetTrackOther());
            sendData.Add("donate", isDonate ? "1" : "0");
            sendData.Add("time", ((int)BattleTimeManager.Instance.CurrentBattleTime).ToString());
            return sendData;
        }
    }
}