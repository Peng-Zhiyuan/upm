/* Created:Loki Date:2023-02-28*/

namespace BattleEngine.Logic
{
    public class BattleSBManager : Singleton<BattleSBManager>
    {
#region 技能间隔CD
        private float SBTimeOffset = 0.5f;
        private float SBTimeMinTime = 0.0f;
        private float SBTimeMaxTime = 2.0f;
        private float currentSBTime = 0.0f;

        public float GetSingleCDOffsetTime()
        {
            currentSBTime += SBTimeOffset;
            if (currentSBTime > SBTimeMaxTime)
            {
                currentSBTime = SBTimeMinTime;
            }
            return currentSBTime;
        }
#endregion

#region 站位圆圈角度
        private int SBAngleMin = 0;
        private int SBAngleMax = 360;
        private int currentSBAngle = 0;

        public int GetSingleAngleOffset(int offsetAngle)
        {
            currentSBAngle += offsetAngle;
            if (currentSBAngle >= SBAngleMax)
            {
                currentSBAngle = SBAngleMin;
            }
            return currentSBAngle;
        }
#endregion
    }
}