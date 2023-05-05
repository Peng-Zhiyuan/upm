namespace BattleEngine.Logic
{
    using UnityEngine;

    public class BattleTimeManager : Singleton<BattleTimeManager>, IUpdatable
    {
#region Var
        private float mCurrentBattleTime = 0;
        private int mBattleTime = 0;
        public int BattleTime
        {
            get { return mBattleTime; }
        }
        public int RemainBattleTime
        {
            get
            {
                if (mBattleTime <= mCurrentBattleTime)
                {
                    return 0;
                }
                return mBattleTime <= 0 ? 0 : mBattleTime;
            }
        }
        /// <summary>
        /// 当前战斗运行时间（秒）
        /// </summary>
        public float CurrentBattleTime
        {
            get { return mCurrentBattleTime; }
        }
        private bool mIsBeginTime = false;
        public System.Action delegateBattleTimeEnd;

        public long NowTimestamp
        {
            get { return Clock.TimestampMs; }
        }
#endregion

#region Method
        public void OnUpdate()
        {
            if (!mIsBeginTime)
                return;
            if (mCurrentBattleTime >= mBattleTime)
            {
                BattleTimeEnd();
                return;
            }
            mCurrentBattleTime += Time.deltaTime;
        }

        public void InitBattleTime(int battleTime, System.Action delegateTimeFinish = null)
        {
            UpdateManager.Instance.Add(BattleTimeManager.Instance);
            mBattleTime = battleTime;
            mCurrentBattleTime = 0;
            mIsBeginTime = false;
            delegateBattleTimeEnd = delegateTimeFinish;
        }

        public void BeginBattleTime()
        {
            mIsBeginTime = true;
        }

        public void PauseBattleTime()
        {
            mIsBeginTime = false;
        }

        public void ResetBattleTime()
        {
            mCurrentBattleTime = 0;
        }

        public void EndBattleTime()
        {
            mIsBeginTime = false;
            delegateBattleTimeEnd = null;
            UpdateManager.Instance.Remove(BattleTimeManager.Instance);
        }

        private void BattleTimeEnd()
        {
            mIsBeginTime = false;
            delegateBattleTimeEnd?.Invoke();
            delegateBattleTimeEnd = null;
            EndBattleTime();
        }
#endregion
    }
}