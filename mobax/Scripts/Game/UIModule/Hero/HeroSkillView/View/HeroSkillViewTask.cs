/* Created:Loki Date:2022-10-17*/

using BattleEngine.Logic;
using UnityEngine;

public abstract class HeroSkillViewTask
{
    public GameObject ActorObject;
    public AbilityTaskData AbilityTaskData;

    public virtual void InitTask(AbilityTaskData data)
    {
        AbilityTaskData = data;
    }

    public abstract void BeginExecute(int frameIdx);

    public abstract void DoExecute(int frameIdx);

    public abstract void EndExecute();
}