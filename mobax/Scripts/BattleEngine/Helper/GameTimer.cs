namespace BattleEngine.Logic
{
    using System;

    public class GameTimer
    {
        private float _maxTime;
        private float _time;
        public bool IsFinished => _time >= _maxTime;
        public bool IsRunning => _time < _maxTime;
        public float MaxTime
        {
            get => _maxTime;
            set => _maxTime = value;
        }

        public GameTimer(float maxTime)
        {
            if (maxTime < 0)
            {
                BattleLog.LogError("_maxTime can not be 0 or negative");
                maxTime = 0;
            }
            _maxTime = maxTime;
            _time = 0f;
        }

        public void Reset()
        {
            _time = 0f;
        }

        public void SetTime(float time)
        {
            _time = time;
        }

        public float GetTime()
        {
            return _time;
        }

        public void Finish()
        {
            _time = _maxTime;
        }

        public GameTimer UpdateAsFinish(float delta, Action onFinish = null)
        {
            if (!IsFinished)
            {
                _time += delta;
                if (IsFinished)
                {
                    onFinish?.Invoke();
                }
            }
            return this;
        }

        public void UpdateAsRepeat(float delta, Action onRepeat = null)
        {
            _time += delta;
            while (_time >= _maxTime)
            {
                _time -= _maxTime;
                onRepeat?.Invoke();
            }
        }
    }
}