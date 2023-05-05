using ProtoBuf;

namespace BattleEngine.Logic
{
    public sealed class BattleConst
    {
        public bool isClientBattleServer = false;

        public static readonly int ATKCampID = 0;
        public static readonly int DEFCampID = 1;
        public static readonly int ATKTeamID = 0;
        public static readonly int DEFTeamID = 1;
        public static readonly int MainPosStart = 0; //上场队员起始位置
        public static readonly int MainPosEnd = 9; //上场队员结束位置
        public static readonly int SubPosStart = 10; //替补起始位置
        public static readonly int SubPosEnd = 20; //替补结束位置
        public static readonly int PlayerPosIndex = 100; //主角位置
        public static readonly int FriendPosIndex = 200; //好友助战位置
        public static readonly int SummonPosIndexStart = 1000; //召唤起始位置
        public static readonly int SSPAssistPosIndexStart = 1300; //大招召唤人物
        public static readonly int DefenceTargetIndex = 1500; //防守模式A位置
        public static float keepSafeTime = 1.5f;
        public static float keepSafeCD = 10;
        public static int SiLaLiID = 1501003;
        public static int ShuiJinID = 1501099; //水晶ID
        public static bool AutoFight = false;
        public static AutoState AutoState = AutoState.None;
        public static EStageDifficult Difficult = EStageDifficult.Easy;
        public static int DEFTeamKeyIndex
        {
            get { return DEFCampID * 1000 + DEFTeamID; }
        }

        public static int MinAttackDistance = 1;

        // private static float mBattleTimeScale = 1.0f;
        // public static float BattleTimeScale
        // {
        //     get { return mBattleTimeScale; }
        //     set { mBattleTimeScale = value; }
        // }

        public static bool IsFightPosIndex(int posIndex)
        {
            if (posIndex == PlayerPosIndex
                || posIndex == FriendPosIndex
                || posIndex == SSPAssistPosIndexStart)
            {
                return false;
            }
            return true;
        }
    }
}