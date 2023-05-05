namespace BattleEngine.Logic
{
    using UnityEngine;

    public enum LogicState
    {
        Nothing = 0,
        Playing = 1,//逻辑帧播放
        Quick = 2,//逻辑帧加速
        End = 3//逻辑结束
    }

    public class BattleLogicDefine
    {
        public static int LogicSecFrame = 30; //每秒x个逻辑帧
        private static float _logicSecTime = 0.0f;
        public static float LogicSecTime
        {
            get
            {
                if (_logicSecTime == 0.0f)
                {
                    _logicSecTime = Mathf.FloorToInt(1000.0f / LogicSecFrame) * 0.001f;
                }
                return _logicSecTime;
            }
        }
    }
}