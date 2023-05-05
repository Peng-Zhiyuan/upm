using System.Collections;
using System.Collections.Generic;
using BattleSystem.Core;
using DG.Tweening;
using Game.Skill;
using Spine.Unity;
using UnityEngine;
using AnimationState = Spine.AnimationState;

//播放动作
public class EET_None : BattleTrigger
{
    protected override void OnTrigger()
    {
        Debug.LogError("没有配置的触发器！！！");
    }
}

//播放动作
public class PlayAnimation : BattleTrigger
{
    protected override void OnTrigger()
    {
        var data = config.EventData as EventPlayAnimation;
        if (data.Caster != 0)
        {
            Creature role = SceneObjectManager.Instance.FindCreatureByConfigID(data.Caster);
            if (role != null)
            {
                role._cac.ChangeAction(data.AnimName);
            }
            return;
        }
        if (ShareData.sceneObj != null)
        {
            var role = ShareData.sceneObj as Creature;
            if (data.FaceCamera)
            {
                role.CreatureLookAt(role.GetPosition() + new Vector3(0, 0, -1f));
            }
           // bool fadeIn = false;
            if (ShareData is TriggerShareDataSkill)
            {
                //fadeIn = (ShareData as TriggerShareDataSkill).atkState == AtkState.CONTINUE_ATK;
                //Debug.LogError("fadeIn:"+ fadeIn);
            }
            string animname = data.AnimName;
            if (data.AnimName.Contains("/"))
            {
                string[] names = data.AnimName.Split('/');
                if (names.Length > 0)
                {
                    animname = names[role.NormalSkillIndex];
                    role.NormalSkillIndex = (role.NormalSkillIndex + 1) % names.Length;
                }
            }

            // //Debug.LogError("-4----------playanima");
            // role.Animator.applyRootMotion = data.IsMove;
            // //bone.transform.SetParent(role.transform.parent);
            // role._cac.ChangeAction(animname, false, null, fadeIn);
            // if (data.IsMove)
            // {
            //     Vector3 lastPos = role.Animator.transform.position;
            //     bool IsEnd = false;
            //     float len = GetClipLength(role.Animator, animname);
            //     //Debug.LogError("动作长度 =" + len);
            //     BattleTimer.Instance.DelayCall(len, delegate(object[] objects)
            //                     {
            //                         if (!ClientEngine.IsAccessable) return;
            //                         var battleCore = Battle.Instance.battleCore;
            //                         var astar = battleCore.coreEngine.FindStuff<AstarPathCore>();
            //                         if (!PathUtil.IsWalkable(astar, role.Animator.transform.position))
            //                         {
            //                             var pos = PathUtil.GetNearestWalkablePos(astar, role.Animator.transform.position);
            //                             //Vector3 pos = lastPos;
            //                             pos.y = role.Animator.transform.position.y;
            //                             role.Animator.transform.position = pos;
            //                         }
            //                         role.Animator.applyRootMotion = false;
            //                         role.Animator.transform.parent.position = role.Animator.transform.position;
            //                         role.Animator.transform.localPosition = Vector3.zero;
            //                         IsEnd = true;
            //                     }
            //     );
            //     int number = 0;
            //     Tween t = DOTween.To(() => number, x => number = x, 10000, len);
            //     // 给执行 t 变化时，每帧回调一次 UpdateTween 方法
            //     t.OnUpdate(delegate
            //                     {
            //                         var battleCore = Battle.Instance.battleCore;
            //                         var astar = battleCore.coreEngine.FindStuff<AstarPathCore>();
            //                         if (!PathUtil.IsWalkable(astar, role.Animator.transform.position))
            //                         {
            //                             if (IsEnd)
            //                                 return;
            //                             var pos = PathUtil.GetNearestWalkablePos(astar, role.Animator.transform.position);
            //                             //Vector3 pos = lastPos;
            //                             pos.y = role.Animator.transform.position.y;
            //                             role.Animator.transform.position = pos;
            //                             /*Vector3 pos = lastPos;
            //                             pos.y = role.Animator.transform.position.y;
            //                             role.Animator.transform.position = pos;*/
            //                         }
            //                         lastPos = role.Animator.transform.position;
            //                     }
            //     );
            // }
        }
    }

    public float GetClipLength(Animator animator, string clip)
    {
        if (null == animator
            || string.IsNullOrEmpty(clip)
            || null == animator.runtimeAnimatorController)
            return 0;
        RuntimeAnimatorController ac = animator.runtimeAnimatorController;
        AnimationClip[] tAnimationClips = ac.animationClips;
        if (null == tAnimationClips
            || tAnimationClips.Length <= 0) return 0;
        AnimationClip tAnimationClip;
        for (int tCounter = 0, tLen = tAnimationClips.Length; tCounter < tLen; tCounter++)
        {
            tAnimationClip = ac.animationClips[tCounter];
            if (null != tAnimationClip
                && tAnimationClip.name == clip)
                return tAnimationClip.length;
        }
        return 0F;
    }
}

//播放动作
public class PlaySoundClip : BattleTrigger
{
    protected override void OnTrigger()
    {
        var data = config.EventData as EventPlaySoundClip;
        if (string.IsNullOrEmpty(data.SoundName))
            return;
        //Debug.LogError(data.SoundName);
        //Debug.LogError("播放音效" + data.SoundName);
        string[] sounds = data.SoundName.Split('/');
        if (sounds.Length > 0)
        {
            var index = Random.Range(0, sounds.Length);
            //AudioEngine.Stuff.AquireClipIfNeedThenPlaySeAsync(sounds[index], 1, data.SoundChannel, 0, 0);
            //AudioManager.PlaySeInBackground(sounds[index] + ".wav");
        }
    }
}

//技能位移
public class SkillMove : BattleTrigger
{
    protected override void OnTrigger()
    {
        var data = config.EventData as EventSkillMove;
        var skillShare = ShareData as TriggerShareDataSkill;
        if (ShareData.sceneObj != null)
        {
            var role = ShareData.sceneObj as Creature;
            role.transform.DOMove(skillShare.destinationPos, Vector3.Distance(skillShare.destinationPos, role.GetPosition()) / data.Speed);
        }
    }
}

//播放动作
public class AddSceneEffect : BattleTrigger
{
    protected override void OnTrigger()
    {
        var data = config.EventData as EventAddSceneEffect;
        Vector3 pos = data.Pos;
        if (data.UseObjectPos)
        {
            TriggerShareDataSkill shareData = ShareData as TriggerShareDataSkill;
            pos = shareData.targetPos;
            int index = EffectManager.Instance.CreateMapEffect(data.EffectName, pos, shareData.sceneObj.GetDirection(), data.DurTime);
            ShareData.AddEffectIndex(index);
        }
        else
        {
            int index = EffectManager.Instance.CreateMapEffect(data.EffectName, pos, data.Rotation, data.DurTime);
        }
    }
}

//播放动作
public class PlayCameraShake : BattleTrigger
{
    protected override void OnTrigger()
    {
        var data = config.EventData as EventPlayCameraShake;
        //CameraManager.Instance.StartShake(data.ShakeIntensity, data.Duration);
        CameraSetting.Ins.Shake(data.ShakeIntensity, data.Frenquence, data.Duration);
        //Debug.LogError(data.ShakeIntensity);
    }
}

//播放TimeLine
public class TimeLine : BattleTrigger
{
    protected override void OnTrigger()
    {
        DamageManager.Instance.SetVisible(false);
        GameEventCenter.Broadcast(GameEvent.SuperSkillStart);
        HudManager.Instance.Visible = false;
        var data = config.EventData as EventTimeLine;
        //Debug.LogError("播放TimeLine");
        TriggerShareDataSkill shareData = ShareData as TriggerShareDataSkill;

        //CameraManager.Instance.TryChangeState(CameraState.Front);
        List<GameObject> targets = new List<GameObject>();
        targets.Add(ShareData.sceneObj.gameObject);

        //float bgmVolum = AudioEngine.Stuff.BgmVolume;
        float bgmVolum = AudioManager.BgmVolume;
        //AudioEngine.Stuff.BgmVolume = 0.4f;
        AudioManager.BgmVolume = bgmVolum * 0.4f;
        BattleTimelineManager.Instance.PlayAsync(data.TimeLineName, shareData.sceneObj, targets, () =>
                        {
                            //CameraManager.Instance.TryChangeState(CameraState.Free);

                            //AudioEngine.Stuff.BgmVolume = bgmVolum;
                            AudioManager.BgmVolume = bgmVolum;
                        }
        );
    }
}

//关闭TimeLine
public class TimeLineEnd : BattleTrigger
{
    protected override void OnTrigger()
    {
        var data = config.EventData as EventTimeLineEnd;
        BattleTimelineManager.Instance.Stop();
        HudManager.Instance.Visible = true;
        DamageManager.Instance.SetVisible(true);
        //Debug.LogError("关闭TimeLine");
        //TriggerShareDataSkill shareData = ShareData as TriggerShareDataSkill;
        //CameraManager.Instance.TryChangeState(CameraState.Free);
        //CoreEngine.Instance.Pause = false;
    }
}

public class Card : BattleTrigger
{
    protected override void OnTrigger()
    {
        var data = config.EventData as EventCard;
        PlayLive(data.CardName, data.Offset, data.AnimName);
    }

    public async void PlayLive(string name, Vector3 pos, string animationname)
    {
        var bucket = BucketManager.Stuff.Battle;
        var obj = await bucket.GetOrAquireAsync<GameObject>(name + ".prefab");
        var spine = GameObject.Instantiate(obj) as GameObject;
        GameObject uiroot = GameObject.Find("UIEngine");
        if (uiroot == null)
            return;
        spine.transform.SetParent(uiroot.transform);
        spine.transform.localPosition = pos;
        spine.transform.localScale = Vector3.one * 0.2f;
        var graph = spine.GetComponent<SkeletonGraphic>();
        graph.AnimationState.SetAnimation(0, animationname, false);
        AnimationState.TrackEntryDelegate ac = null;
        ac = delegate
        {
            graph.AnimationState.Complete -= ac;
            if (animationname == "animation1")
            {
                PlayAnimation(graph, "animation2", spine);
            }
            else
            {
                GameObject.Destroy(spine);
            }
        };
        graph.AnimationState.Complete += ac;
    }

    private void PlayAnimation(SkeletonGraphic graph, string animationname, GameObject spine)
    {
        graph.AnimationState.SetAnimation(0, animationname, false);
        AnimationState.TrackEntryDelegate ac = null;
        ac = delegate
        {
            graph.AnimationState.Complete -= ac;
            if (animationname == "animation1")
            {
                graph.AnimationState.SetAnimation(0, "animation2", false);
            }
            GameObject.Destroy(spine);
        };
        graph.AnimationState.Complete += ac;
    }
}

public class Transition : BattleTrigger
{
    protected override void OnTrigger()
    {
        var data = config.EventData as EventTransition;
        Debug.LogError("播放转场");
        TriggerShareDataSkill shareData = ShareData as TriggerShareDataSkill;
        List<GameObject> list = shareData.targets;
        Creature role = shareData.sceneObj as Creature;
        // if (role != null)
        //    list.Add(role.RoleRender.subRender.gameObject);
        //list.Add(shareData.sceneObj.gameObject);
        var endColor = new Color(data.FadeColorR, data.FadeColorB, data.FadeColorB, data.FadeColorA);
        if (!data.FadeOut)
            PostProcessHandler.Ins.PlayFadeInColor(data.DurTime, data.TransTime, endColor, list);
        else
        {
            PostProcessHandler.Ins.PlayFadeOutColor(data.DurTime, data.TransTime, endColor, null);
        }
    }
}

public class SkillCamera : BattleTrigger
{
    protected override void OnTrigger()
    {
        var data = config.EventData as EventSkillCamera;
        if (data.Open)
        {
            CameraPosData param = new CameraPosData();
            param.distance = data.Distance;
            param.fov = data.FOV;
            param.hAngle = data.HAngle;
            param.vAngle = data.VAngle;
            param.smoothscale = data.SmoothScale;
            param.sideOffset = data.SideOffset;
            Debug.LogError("切換鏡頭");
            CameraManager.Instance.TryChangeState(CameraState.Skill, param);
        }
        else
        {
            Debug.LogError("恢復鏡頭");
            CameraManager.Instance.TryChangeState(CameraState.Free2);
        }
    }
}

public class TargetFocus : BattleTrigger
{
    protected override void OnTrigger()
    {
        var data = config.EventData as EventTargetFocus;
        if (data.Focus)
        {
            Debug.LogError("開聚焦");
            List<string> ids = new List<string>();
            //Vector3 centerPos = CoreSkill.Instance.CalculateSkillTargets(ShareData.sceneObj.ID, 501105, ids);
            SceneObjectManager.Instance.LocalPlayerCamera.RegionPosition = SceneObjectManager.Instance.LocalPlayerCamera.GetPosition();
            //SceneObjectManager.Instance.LocalPlayer.SetPosition(centerPos);
            //SceneObjectManager.Instance.LocalPlayer.MoveTo(centerPos);
            SceneObjectManager.Instance.LocalPlayerCamera.DisableFollowSelect = true;
        }
        else
        {
            Debug.LogError("聚焦関");
            SceneObjectManager.Instance.LocalPlayerCamera.DisableFollowSelect = false;
            CameraManager.Instance.TryChangeState(CameraState.Free2);
            //SceneObjectManager.Instance.LocalPlayer.SetFocusTarget(ShareData.sceneObj as Creature);
            SceneObjectManager.Instance.LocalPlayerCamera.MoveTo(SceneObjectManager.Instance.LocalPlayerCamera.RegionPosition, data.DurTime);
        }
    }
}

public class AddFlyEffect : BattleTrigger
{
    protected override void OnTrigger()
    {
        var data = config.EventData as EventAddFlyEffect;
        TriggerShareDataSkill shareData = ShareData as TriggerShareDataSkill;
        List<GameObject> list = shareData.targets;
        if (ShareData.sceneObj != null)
        {
            var role = ShareData.sceneObj as Creature;
            if (role != null)
            {
                bool bFind = false;
                var bone = role.GetBone(data.BoneName);
                if (!string.IsNullOrEmpty(data.BoneName)
                    && bone == null)
                {
                    Debug.LogError(role.ID + "没有找到骨骼点--" + data.BoneName);
                    return;
                }
                else if (string.IsNullOrEmpty(data.BoneName))
                {
                    bone = role.transform;
                }
                if (data.Trace)
                {
                    foreach (var VARIABLE in shareData.attackIDs)
                    {
                        var player = SceneObjectManager.Instance.Find(VARIABLE, true);
                        if (player != null)
                        {
                            int index = EffectManager.Instance.CreateFlyEffect(data.EffectName, bone.position + data.OffsetPos, (player.GetPosition() - role.GetPosition()).normalized, data.Speed, player.ID, data.DurTime, data.IsFoucs, null, data.Trace);
                            ShareData.AddEffectIndex(index);
                            bFind = true;
                        }
                    }
                }
                if (bFind == false)
                {
                    int index = EffectManager.Instance.CreateFlyEffect(data.EffectName, bone.position + data.OffsetPos, (shareData.targetPos - role.GetPosition()).normalized, data.Speed, "", data.DurTime, data.IsFoucs, null, data.Trace);
                    ShareData.AddEffectIndex(index);
                }

                //role.CharaterModel.Spring();

                //if(role.sceneObjectType == SceneObjectType.Player)
                //Debug.LogError("播放特效" + data.EffectName);
            }
        }
    }
}

public class SendEvent : BattleTrigger
{
    protected override void OnTrigger()
    {
        var data = config.EventData as EventSendEvent;
        EventManager.Instance.SendEvent(data.EventID, data.EventParam);
    }
}