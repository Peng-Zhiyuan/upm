/* Created:Loki Date:2023-01-04*/

using UnityEngine;
using System.Threading.Tasks;
using System;

namespace BattleEngine.Logic
{
    public class TestBattleServerManager : Singleton<TestBattleServerManager>
    {
        public async void BattlePVE(int stageID)
        {
            try
            {
                // BattleLog.isOpenBuffLog = false;
                // BattleServerData serverData = await BattleServerUtil.CreateBattleServerData(stageID, FormationUtil.GetDefaultFormationIndex(), null);
                // Debug.LogWarning("Begin BattleTime");
                // float time = Time.realtimeSinceStartup;
                // BattleServerManager.Instance.InitManager(serverData);
                // Debug.LogWarning("Create Server Data Time : " + Math.Ceiling((Time.realtimeSinceStartup - time) * 1000) + "ms");
                // await Task.Delay(1000);
                // time = Time.realtimeSinceStartup;
                // await BattleServerManager.Instance.ExecuteBattleVerify();
                // Debug.LogWarning("BattleTime : " + Math.Ceiling((Time.realtimeSinceStartup - time) * 1000) + "ms");
            }
            catch (Exception e)
            {
                BattleLog.LogError(e.Message);
                throw;
            }
        }
    }
}