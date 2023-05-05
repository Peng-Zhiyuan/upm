namespace BattleEngine.Logic
{
    using UnityEngine;

    public static class MathHelper
    {
        /// <summary>
        /// 输出数值为距离的2倍,注意比对值大小,并且会带入双方的半径
        /// </summary>
        public static float ActorDistance(CombatActorEntity a1, CombatActorEntity a2)
        {
            if (a1 == null
                || a2 == null)
            {
                BattleLog.LogError("Actor Transform is null " + a1.battleItemInfo.id + "    " + a2.battleItemInfo.id);
                return 0.0f;
            }
            return BattleLogicManager.Instance.BattleData.GetActorDistance(a1, a2);
        }

        /// <summary>
        ///计算机计算两次方和开根的运算量比加减法要费时的多
        /// </summary>
        public static float DoubleDistanceVect3(Vector3 pos1, Vector3 pos2)
        {
            return (pos1 - pos2).sqrMagnitude;
        }

        public static float DistanceVec3(Vector3 pos1, Vector3 pos2)
        {
            return Vector3.Distance(pos1, pos2);
        }

        public static Vector3 GetLookAtEuler(Vector3 originPos, Vector3 targetPos)
        {
            Vector3 forwardDir = targetPos - originPos;
            Vector3 resultEuler = Vector3.zero;
            if (forwardDir != Vector3.zero)
            {
                Quaternion lookAtRot = Quaternion.LookRotation(forwardDir);
                resultEuler = lookAtRot.eulerAngles;
            }
            return resultEuler;
        }

        public static Vector3 GetDirection(Vector3 currentForward, float OffsetYAngle)
        {
            Quaternion offsetRot = Quaternion.AngleAxis(OffsetYAngle, Vector3.up);
            Vector3 targetDir = offsetRot * currentForward;
            return targetDir;
        }
    }
}