/* Created:Loki Date:2022-10-17*/

using BattleEngine.Logic;
using BattleEngine.View;
using UnityEngine;

public class HeroSkillCreateEffectViewTask : HeroSkillViewTask
{
    public SkillCreateEffectTaskData TaskData;
    private GameObject fx;
    private string EffectPrefabName;
    private bool hasbreak = false;

    public override void InitTask(AbilityTaskData data)
    {
        base.InitTask(data);
        TaskData = AbilityTaskData as SkillCreateEffectTaskData;
    }

    public override void BeginExecute(int frameIdx)
    {
        PlayEffect();
    }

    public override void DoExecute(int frameIdx) { }

    public override void EndExecute() { }

    private async void PlayEffect()
    {
        if (TaskData == null
            || string.IsNullOrEmpty(TaskData.effectPrefabName)
            || ActorObject == null)
        {
            return;
        }
        EffectPrefabName = TaskData.effectPrefabName;
        Transform actorTrans = ActorObject.transform;
        switch (TaskData.attachType)
        {
            case COORDINATE_TYPE.Global:
                fx = await BattleResManager.Instance.CreatorFx(EffectPrefabName, TaskData.isLoop);
                if (fx == null)
                {
                    return;
                }
                fx.name = EffectPrefabName;
                if (TaskData.isAttachLookAt)
                {
                    fx.transform.position = actorTrans.position + Quaternion.Euler(actorTrans.eulerAngles) * TaskData.Offset;
                    fx.transform.forward = Quaternion.Euler(TaskData.rotation) * actorTrans.forward;
                }
                else
                {
                    fx.transform.position = actorTrans.position + TaskData.Offset;
                    fx.transform.rotation = Quaternion.Euler(TaskData.rotation);
                }
                fx.transform.localScale = TaskData.scale;
                break;
            case COORDINATE_TYPE.Local:
                Transform parentTrans = actorTrans;
                if (string.IsNullOrEmpty(TaskData.attachPoint))
                {
                    parentTrans = actorTrans;
                    fx = await BattleResManager.Instance.CreatorFx(EffectPrefabName, parentTrans, Vector3.zero, TaskData.isLoop);
                }
                else
                {
                    parentTrans = GameObjectHelper.FindChild(actorTrans, TaskData.attachPoint);
                    fx = await BattleResManager.Instance.CreatorFx(EffectPrefabName, parentTrans, Vector3.zero, TaskData.isLoop);
                }
                if (fx == null)
                {
                    return;
                }
                fx.name = EffectPrefabName;
                if (!TaskData.isAttachLock)
                {
                    if (TaskData.isAttachLookAt)
                    {
                        fx.transform.localPosition = TaskData.Offset;
                        fx.transform.localRotation = Quaternion.Euler(TaskData.rotation);
                    }
                    else
                    {
                        fx.transform.position = parentTrans.position + TaskData.Offset;
                        fx.transform.rotation = Quaternion.Euler(TaskData.rotation);
                    }
                }
                else
                {
                    if (TaskData.isAttachLookAt)
                    {
                        fx.transform.localPosition = TaskData.Offset;
                        fx.transform.localRotation = Quaternion.Euler(TaskData.rotation);
                    }
                    else
                    {
                        fx.transform.position = parentTrans.position + TaskData.Offset;
                        fx.transform.rotation = Quaternion.Euler(TaskData.rotation);
                    }
                }
                fx.transform.localScale = TaskData.scale;
                break;
        }
    }
}