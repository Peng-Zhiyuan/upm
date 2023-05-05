using System;
using System.Linq;
using Spine;
using Spine.Unity;
using UnityEngine;
using Animation = Spine.Animation;

public class SpineAnimControl
{
    #region ---PROPERTIES---

    public float TimeScale = 1;

    #endregion

    #region ---ACTIONS---

    private Action _onCompleted;
    private Action _onEnd;

    #endregion

    #region ---VAR---

    private const float DEFAULT_FRAME_NUM = 30f; // 默认帧率是30帧每秒
    private TrackEntry _currentTrack; // 当前播放的动画轨道

    #endregion

    #region ---NEW---

    private SkeletonAnimation _anim;
    private Spine.AnimationState _animState;

    public SpineAnimControl(SkeletonAnimation anim)
    {
        _anim = anim;
    }

    private void SetTrack()
    {
        _animState = _anim.state;

        // 内部在播放animation的时候也是取的tracks[0];
        _currentTrack = _animState.Tracks.First();
        AddEvent();
    }

    #endregion

    #region ---GET---

    /// <summary>
    ///  获取当前的轨道
    /// </summary>
    /// <param name="trackIndex"></param>
    /// <returns></returns>
    public TrackEntry GetTrack(int trackIndex)
    {
        return _animState.GetCurrent(trackIndex);
    }

    /// <summary>
    /// 获取当前的animation
    /// </summary>
    /// <param name="animName"></param>
    /// <returns></returns>
    public Animation GetAnimation(string animName)
    {
        return _animState.Data.SkeletonData.FindAnimation(animName);
    }

    #endregion

    #region ---ANIMATION---

    public void Play(string animName, bool loop)
    {
        _animState = _anim.state;
        _anim.loop = loop;
        _anim.timeScale = TimeScale;

        // 这个需要设置在最后面,否则loop等属性启用无法适用
        _anim.AnimationName = animName;
        SetTrack();
    }

    #endregion

    #region ---TRACK---

    /// <summary>
    /// 指定播放spine内某个轨道动画
    /// </summary>
    /// <param name="trackIndex"></param>
    /// <param name="aniName"></param>
    /// <param name="loop"></param>
    public void PlayTrack(int trackIndex, string aniName, bool loop)
    {
        _currentTrack = _animState.SetAnimation(trackIndex, aniName, loop);
    }

    /// <summary>
    /// 指定播放spine内某个队列延迟轨道动画
    /// </summary>
    /// <param name="trackIndex"></param>
    /// <param name="aniName"></param>
    /// <param name="loop"></param>
    /// <param name="delay"></param>
    public void QueueTrack(int trackIndex, string aniName, bool loop, float delay)
    {
        _currentTrack = _animState.AddAnimation(trackIndex, aniName, loop, delay);
    }

    /// <summary>
    /// 暂停
    /// </summary>
    public void Pause()
    {
        _currentTrack.TimeScale = 0;
    }

    public void Stop()
    {
        _currentTrack.TimeScale = 0;
    }

    #endregion

    #region ---SET TIME---

    /// <summary>
    /// 当前轨道的任意时间点开始播放
    /// </summary>
    /// <param name="time"></param>
    public void SetStartTime(float time)
    {
        SetStartTime(_currentTrack, time);
    }

    public void SetStartTime(int trackIndex, float time)
    {
        SetStartTime(GetTrack(trackIndex), time);
    }

    private void SetStartTime(TrackEntry track, float time)
    {
        if (time <= 0) return;

        track.AnimationStart = time;
    }

    public void SetStartFrame(float frame)
    {
        SetStartTime(frame / DEFAULT_FRAME_NUM);
    }

    public void SetStartFrame(int trackIndex, float frame)
    {
        SetStartTime(trackIndex, frame / DEFAULT_FRAME_NUM);
    }

    public void SetEndTime(float time)
    {
        SetEndTime(_currentTrack, time);
    }

    public void SetEndTime(int trackIndex, float time)
    {
        SetEndTime(GetTrack(trackIndex), time);
    }

    private void SetEndTime(TrackEntry track, float time)
    {
        if (time <= 0) return;

        track.AnimationEnd = time;
    }

    public void SetEndFrame(float frame)
    {
        SetEndTime(frame / DEFAULT_FRAME_NUM);
    }

    public void SetEndFrame(int trackIndex, float frame)
    {
        SetEndTime(trackIndex, frame / DEFAULT_FRAME_NUM);
    }

    #endregion

    private void Jump2Time(float time, bool skipEvent, bool stop)
    {
        // 如果此时已完成
        if (JudgeIsCompleted()) return;

        _currentTrack.TrackTime = time;

        if (skipEvent)
        {
            _currentTrack.TrackEnd = time;
        }

        if (stop)
        {
            _currentTrack.TimeScale = 0f;
        }
    }


    #region ---EVENT---

    public void AddCompletedListener(Action onCompleted)
    {
        _onCompleted = onCompleted;
    }
    
    public void AddEndListener(Action onEnd)
    {
        _onEnd = onEnd;
    }

    private void AddEvent()
    {
        _currentTrack.Start += OnSpineAnimationStart;
        _currentTrack.Interrupt += OnSpineAnimationInterrupt;
        _currentTrack.End += OnSpineAnimationEnd;
        _currentTrack.Dispose += OnSpineAnimationDispose;
        _currentTrack.Complete += OnSpineAnimationComplete;
        // _currentTrack.Event += OnUserDefinedEvent;
    }

    private void OnSpineAnimationStart(TrackEntry trackEntry)
    {
        Debug.Log("[SpineAnim]:开始播放！");
    }

    private void OnSpineAnimationInterrupt(TrackEntry trackEntry)
    {
        Debug.Log("[SpineAnim]:播放中被打断！");
    }

    private void OnSpineAnimationEnd(TrackEntry trackEntry)
    {
        _onEnd?.Invoke();
        Debug.Log("[SpineAnim]:动画已停止(ended) .");
    }

    private void OnSpineAnimationDispose(TrackEntry trackEntry)
    {
        Debug.Log("[SpineAnim]:动画和其所处的 TrackEntry 已被销毁!");
    }

    private void OnSpineAnimationComplete(TrackEntry trackEntry)
    {
        _onCompleted?.Invoke();
        Debug.Log("[SpineAnim]:动画无中断地播放已完成(completed) , 如果是循环动画, 该事件可能会多次出现.");
    }

    #endregion

    #region ---DISPOSE---

    public void Dispose()
    {
        // _anim.ClearState();
        // _anim = null;
    }

    #endregion

    #region ---JUDGE---

    private bool JudgeIsCompleted()
    {
        return _currentTrack.IsComplete;
    }

    #endregion
}