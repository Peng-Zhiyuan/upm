namespace BattleEngine.Logic
{
    using System;

    public abstract class AbilityTaskData
    {
        public int lifeTime = 0;
        public int startFrame;
        public int endFrame;

        public abstract SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType();
        public abstract SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform();

        public virtual void Init(SkillActionElementItem element)
        {
            startFrame = Math.Max(0, element.startFrame + element.initFrame);
            endFrame = element.endFrame + element.initFrame;
        }
    }
}