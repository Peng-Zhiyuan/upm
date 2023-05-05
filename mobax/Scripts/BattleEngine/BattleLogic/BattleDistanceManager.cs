/* Created:Loki Date:2022-11-01*/

namespace BattleEngine.Logic
{
    using System.Collections.Generic;

    public sealed class BattleDistanceManager
    {
        private Dictionary<string, float> actorDistanceDic = new Dictionary<string, float>();

        public float CalActorDistance(CombatActorEntity actor, CombatActorEntity target)
        {
            string key = StrBuild.Instance.ToStringAppend(actor.UID, "&", target.UID);
            if (actorDistanceDic.ContainsKey(key))
            {
                return actorDistanceDic[key];
            }
            key = StrBuild.Instance.ToStringAppend(target.UID, "&", actor.UID);
            if (actorDistanceDic.ContainsKey(key))
            {
                return actorDistanceDic[key];
            }
            key = StrBuild.Instance.ToStringAppend(actor.UID, "&", target.UID);
            actorDistanceDic[key] = MathHelper.DistanceVec3(actor.GetPositionXZ(), target.GetPositionXZ());
            return actorDistanceDic[key];
        }

        public void ClearData()
        {
            if (actorDistanceDic.Count == 0)
            {
                return;
            }
            actorDistanceDic.Clear();
        }
    }
}