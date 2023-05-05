/* Created:Loki Date:2022-11-08*/

namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// 占坑管理
    /// </summary>
    public sealed class BattleOccupyPosManager
    {
        private Dictionary<string, Vector3> occupySortDic = new Dictionary<string, Vector3>();

        public void ClearData()
        {
            if (occupySortDic.Count == 0)
            {
                return;
            }
            occupySortDic.Clear();
        }

        public bool ExitOccupyPosition(CombatActorEntity actorEntity)
        {
            if (occupySortDic.ContainsKey(actorEntity.UID))
            {
                return false;
            }
            return true;
        }

        public void PushOccupyPosition(CombatActorEntity actorEntity)
        {
            if (!occupySortDic.ContainsKey(actorEntity.UID))
            {
                occupySortDic.Add(actorEntity.UID, actorEntity.GetPosition());
            }
        }
    }
}