using System.Threading.Tasks;
using UnityEngine;

public static class AnimUtil
{
    public static void ResetAni(this Animation ani, string name = "")
    {
        if (string.IsNullOrEmpty(name))
        {
            name = ani.clip.name;
        }
        AnimationState state = ani[name];
        if (state != null)
        {
            ani.Play(name);
            state.time = 0;
            ani.Sample();
            state.enabled = false;
        }
    }

    public static async Task PlayAnimAsync(this Animation ani, string animName)
    {
        ani.Play(animName);
        var waitTime = Mathf.FloorToInt(ani.GetClip(animName).length * 1000);
        await Task.Delay(waitTime);
        for (var i = 0; i < 100; i++)
        {
            if (ani.isPlaying)
                await Task.Delay(10);
            else
                break;
        }
    }

    public static float GetAnimTime(this Animation ani, string animName)
    {
        AnimationClip clip = ani.GetClip(animName);
        if (clip != null)
        {
            return clip.length;
        }
        return 0.0f;
    }

    ///获取动画状态机animator的动画clip的播放持续时长
    public static float GetClipLength(this Animator animator, string clipName)
    {
        if (string.IsNullOrEmpty(clipName) || animator == null
            || animator.runtimeAnimatorController == null)
            return 0.0f;
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        if (clips == null
            || clips.Length <= 0)
            return 0.0f;
        AnimationClip clip;
        for (int i = 0; i < clips.Length; ++i)
        {
            clip = clips[i];
            if (clip != null
                && clip.name == clipName)
            {
                return clip.length;
            }
        }
        return 0.0f;
    }

    public static async Task WaitAnimationAsync(Animation animation)
    {
        while (animation.isPlaying)
        {
            await Task.Delay(100);
        }
    }
}