using System;
using System.Linq;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

/// <summary>
/// 剧情总控
/// </summary>
public class PlotProxy : IDisposable
{
    private PlayableDirector _director;
    private GameObject _rogueLikeMapParts;

    /// <summary>
    /// 添加一个新场景之前需要把之前激活的场景的节点给隐藏
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="onLoadComp"></param>
    /// <param name="onComp"></param>
    public async void LoadTimeLineScene(string sceneName, Action onLoadComp, Action onComp)
    {
        // 需要判断当前激活得场景是不是就是timeline场景
        var curScene = SceneManager.GetActiveScene();
        if (curScene.name.Equals(sceneName))
        {
            this.DoCurScene2PlayPlot(curScene,onLoadComp, onComp);
            return;
        }
        
        this.DoLoadScene2PlayPlot(sceneName, onLoadComp, onComp);
    }

    private async void DoLoadScene2PlayPlot(string sceneName, Action onLoadComp, Action onComp)
    {
        this.SetRogueLikeMapParts();
        this.SetSceneRootVisible(false);
        this.SetCameraActive(false);

        SceneInstance sceneInstance = await SceneUtil.AddressableLoadSceneAsync(sceneName, LoadSceneMode.Additive);
        onLoadComp?.Invoke();
        var plotScene = sceneInstance.Scene;
        SceneManager.SetActiveScene(plotScene);
        var pveScene = SceneManager.GetActiveScene();

        var objs = pveScene.GetRootGameObjects().ToList();
        var timeLine = objs.Find(val => val.GetComponent<PlayableDirector>() != null);

        if (timeLine == null)
        {
            onComp?.Invoke();
            return;
        }

        this._director = timeLine.GetComponent<PlayableDirector>();

        async void OnDirectorOnStopped(PlayableDirector p)
        {
            await SceneUtil.AddressableUnloadSceneAsync(sceneInstance);
            // 这里是因为我默认加了一个空白的界面  可以将剧情场景透出来
            this.RemovePlotSwitchPage();
            // UIEngine.Stuff.Back();
            onComp?.Invoke();
            this.SetSceneRootVisible(true);
            this.SetCameraActive(true);
        }

        this._director.stopped += OnDirectorOnStopped;
        this._director.Play();
    }

    // 就执行当前场景的timeline
    private void DoCurScene2PlayPlot(Scene curScene,Action onLoadComp,  Action onComp)
    {
        onLoadComp?.Invoke();
        var objs = curScene.GetRootGameObjects().ToList();
        var timeLine = objs.Find(val => val.GetComponent<PlayableDirector>() != null);
        timeLine.SetActive(true);
        var trans = timeLine.GetComponentsInChildren<Transform>(true);
        var camera = Array.Find(trans, val => val.GetComponent<Camera>() != null);
        var cameraList = Array.FindAll(trans, val => val.GetComponent<CinemachineVirtualCamera>() != null);

        foreach (var cm in cameraList)
        {
            cm.gameObject.SetActive(true);
        }
     
        if (timeLine == null)
        {
            onComp?.Invoke();
            return;
        }

        this._director = timeLine.GetComponent<PlayableDirector>();

        this._director.stopped += OnDirectorOnStopped;
        this._director.played += OnDirectOnPlayed;
        this._director.Play();
        
        async void OnDirectorOnStopped(PlayableDirector director)
        {
            if(CameraManager.IsAccessable)
            {
                CameraManager.Instance.SetTimeLineCameraData(camera);
                timeLine.SetActive(false);
                this.SetCameraActive(true);
                this.RemovePlotSwitchPage();
                // 这里是因为我默认加了一个空白的界面  可以将剧情场景透出来
                UIEngine.Stuff.RemoveFromStackAsync("PlotChatPage");
                onComp?.Invoke();
            }
  
        }

        async void OnDirectOnPlayed(PlayableDirector p)
        {
            this.SetCameraActive(false);
        }
    }

    private void RemovePlotSwitchPage()
    {
        var switchPage = UIEngine.Stuff.FindPage("PlotSwitchPage");
        if (switchPage!=null)
        {
            UIEngine.Stuff.RemoveFromStackAsync("PlotSwitchPage");
        }
    }

    private void SetRogueLikeMapParts()
    {
        var curActiveScene = SceneManager.GetActiveScene();
        var objs = curActiveScene.GetRootGameObjects();
        if (!curActiveScene.name.Equals("RoguelikeMap")) return;

        this._rogueLikeMapParts = Array.Find(objs, val => val.name.Equals("Parts"));
    }

    private void SetSceneRootVisible(bool isShow)
    {
        if (this._rogueLikeMapParts == null) return;

        this._rogueLikeMapParts.SetActive(isShow);
    }

    public void StopTimeLine()
    {
        var director = this._director;
        director.time = director.duration;
        // director.Evaluate();
        // if (director != null) director.Stop();
    }

    #region ---DOTWEEN---

    private float _tweenValue = 0f;

    public float TweenValue
    {
        get => _tweenValue;
        set
        {
            if (this._tweenValue.Equals(value)) return;
            this._tweenValue = value;

            var curVal = (this._startPosX - this._endPosX) * value;
            if (this._camera == null) return;
            this._camera.m_SideXOffset = this._isLeft ? this._startPosX - curVal : -(this._startPosX - curVal);
        }
    }

    private CustomCameraController _camera;
    private float _startPosX = 0;
    private float _endPosX = 0;
    private bool _isLeft = false;
    private Tweener _tweener;

    /// <summary>
    /// 设置剧情对话tween的目标  以及初始值等  必须在tween前设置好
    /// </summary>
    /// <param name="camera"></param>
    /// <param name="startPosX"></param>
    /// <param name="endPosX"></param>
    /// <param name="isLeft"></param>
    public void SetTweenTarget(CustomCameraController camera, float startPosX, float endPosX, bool isLeft)
    {
        this._camera = camera;
        this._startPosX = startPosX;
        this._endPosX = endPosX;
        this._isLeft = isLeft;
    }

    public void DoTween(float startVal, float endVal, float duration, Action onComp)
    {
        this._tweener = DOTween.To(x => this.TweenValue = x, startVal, endVal, duration)
            .OnComplete(() => { onComp?.Invoke(); });
    }

    #endregion

    /// <summary>
    ///  延迟调用
    /// </summary>
    /// <param name="time"></param>
    /// <param name="onComp"></param>
    /// <param name="timerName"></param>
    public void Delay(float time, Action onComp,string timerName)
    {
        TimerMgr.Instance.ScheduleTimer(time, delegate { onComp?.Invoke(); }, false, timerName);
    }

    public void CleanDelay(string timerName)
    {
        TimerMgr.Instance.Remove(timerName);
    }
    
    public string IdleAction()
    {
        return CharacterActionConst.Idle;
    }

    public void DoCompAction(Action cb)
    {
        cb?.Invoke();
    }

    // 设置战斗场景内相机是否显示
    public async void SetCameraActive(bool isActive)
    {
        CameraManager.Instance.CVCamera.gameObject.SetActive(isActive);
        CameraManager.Instance.MainCamera.gameObject.SetActive(isActive);
        if (isActive)
        {
            // await Task.Delay(10000);
            //Debug.LogError("SetCameraActive");
            //EnvEffectSetting.Stuff.OpenEffect();
        }
    }


    /// <summary>
    /// 废弃 -> 直接Play() 多层级Layer:BaseLayer,EmotionLayer
    /// </summary>
    /// <param name="animator"></param>
    /// <param name="modelAction"></param>
    /// <param name="emotionAction"></param>
    public void SetAnimation(Animator animator, string modelAction, string emotionAction)
    {
        RuntimeAnimatorController runtimeAnimatorController = animator.runtimeAnimatorController;
        var animations = runtimeAnimatorController.animationClips;
        if (animations.Length <= 0) return;

        var overrideController = new AnimatorOverrideController();
        overrideController.runtimeAnimatorController = runtimeAnimatorController;

        var modelAnim = Array.Find(animations, val => val.name.Equals(modelAction));
        var emotionAnim = Array.Find(animations, val => val.name.Equals(emotionAction));
        if (modelAnim)
        {
            overrideController[modelAction] = modelAnim;
        }

        if (emotionAnim)
        {
            overrideController[emotionAction] = emotionAnim;
        }
    }

    public void Dispose()
    {
        this._tweenValue = 0f;
        this._tweener.Kill();
    }
}