/* Created:Loki Date:2023-02-04*/

namespace BattleEngine.Logic
{
    public class AbilityViewTask : Entity
    {
        public object taskInitData { get; set; }
        private SkillAbilityExecution _skillAbilityExecution;
        public SkillAbilityExecution SkillAbilityExecution
        {
            get
            {
                if (_skillAbilityExecution == null)
                {
                    _skillAbilityExecution = GetParent<SkillAbilityExecution>();
                }
                return _skillAbilityExecution;
            }
        }

        public override void Awake(object initData)
        {
            taskInitData = initData;
        }

#pragma warning disable 1998
        public virtual async void BeginExecute(int frameIdx)
#pragma warning restore 1998
        { }

        public virtual void DoExecute(int frameIdx) { }
        public virtual void PauseExecute(int frameIdx) { }

        public virtual void BreakExecute(int frameIdx)
        {
            EndExecute();
        }

        public virtual void EndExecute()
        {
            taskInitData = null;
            Entity.Destroy(this);
        }
    }
}