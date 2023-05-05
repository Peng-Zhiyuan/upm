/* Created:Loki Date:2023-02-09*/

using BattleSystem.ProjectCore;

namespace BattleEngine.Logic
{
    using System;
    using System.Threading.Tasks;
    using UnityEngine;

    public static class BattleServerEnter
    {
        public static async Task<BattleServerResultData> ExecuteBattleCheck(BattleCoreParam battleCoreParam, CreateBattleResponse createBattlResponse, BattlePlayer battlePlayer)
        {
            try
            {
                BattleLog.isOpenBuffLog = false;
                // -----------------------------------
                //              初始化战斗数据
                // -----------------------------------
                BattleServerData serverData = await BattleServerUtil.CreateBattleServerData(battleCoreParam, battleCoreParam.pveParam.FormationIndex, battlePlayer.heroes);
                serverData.BattleKey = createBattlResponse.id;
                serverData.BattleSeed = battlePlayer.Seed;
                serverData.createBattleResponse = createBattlResponse;
                serverData.battlePlayer = battlePlayer;
                BattleLog.LogWarning("Begin BattleTime");
                float time = Time.realtimeSinceStartup;
                BattleServerManager.Instance.InitManager(serverData, battleCoreParam);
                BattleLog.LogWarning("Create Server Data Time : " + Mathf.FloorToInt((Time.realtimeSinceStartup - time) * 1000) + "ms");
                await Task.Delay(1000);
                time = Time.realtimeSinceStartup;
                // -----------------------------------
                //              战斗开始验证
                // -----------------------------------
                BattleServerResultData resultData = await BattleServerManager.Instance.ExecuteBattleVerify();
                BattleLog.LogWarning("BattleTime : " + Mathf.FloorToInt((Time.realtimeSinceStartup - time) * 1000) + "ms");
                BattleLog.isOpenBuffLog = true;
                return resultData;
            }
            catch (Exception e)
            {
                BlockManager.Stuff.RemoveBlock("ExecureBattleCheck");
                BattleLog.LogError(e.Message);
                throw;
            }
        }
    }
}