using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.IO;

public partial class MovieView : MonoBehaviour
{
    public bool autoSize;
    private Action onVideoUnsuuported;
    private Action onLastFrame;
    private Action onReady;

    void Awake()
    {
        this.VieoPlayer.prepareCompleted += OnPreperCompelte;
        this.VieoPlayer.loopPointReached += OnLoopPointReached;
        this.VieoPlayer.errorReceived += VieoPlayer_errorReceived;
    }

    private void VieoPlayer_errorReceived(VideoPlayer source, string message)
    {
        this.UnsupportGroup.gameObject.SetActive(true);
        this.Text_errorMsg.text = message;
        onVideoUnsuuported?.Invoke();
    }


    public Texture VieoTexture => this.VieoPlayer.texture;


    void RefreshImageTexture()
    {
        var texture = this.VieoPlayer.texture;
        Debug.Log($"[MovieView] width: {texture.width} height: {texture.height}");
        this.VieoPlayer.GetComponent<RawImage>().texture = texture;
        if (autoSize)
        {
            this.VieoPlayer.GetComponent<RectTransform>().SetSizeDelta(new Vector2(texture.width, texture.height));
        }
    }

    void OnPreperCompelte(VideoPlayer player)
    {
        this.RefreshImageTexture();
        this.FadeOutFrontBlack();
        try
        {
            onReady?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        if (this.isAutoPlay)
        {
            this.Play();
        }
    }

    void OnLoopPointReached(VideoPlayer player)
    {
        try
        {
            this.onLastFrame?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    bool isAutoPlay;

    [Obsolete("临时支持，MovieView 不再处理资产加载，使用 VideoClip 作为参数的版本")]
    public async void ChangeMovie(string address, Action onReady = null, Action onEnd = null, Action UnSupport = null, bool fadeIn = true, bool isLoop = false, bool isAutoPlay = true)
    {
        this.UnsupportGroup.gameObject.SetActive(false);
        this.Image_frontBg.SetActive(true);
        this.Image_frontBg.color = Color.black; 
        this.Image_frontBg.DOKill(false);
        var clip = await BucketManager.Stuff.Main.GetOrAquireAsync<VideoClip>(address);
        this.ChangeMovie(clip, onReady, onEnd, UnSupport, fadeIn, isLoop, isAutoPlay);
    }

    /// <summary>
    /// 播放视频，注意：应当总是处理不支持的情况
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="onReady">当准备好播放时，可以调用 Play 进行播放，如果 isAutoPlay 已设置，会在这时自动播放</param>
    /// <param name="onLastFrame">当播放到最后一帧时，无论是否循环。如果视频没有循环，在此之后画面会停留在最后一帧。</param>
    /// <param name="onVideoUnsuuport">此视频不被支持，可能与许多因素有关。注意：此时不再调用 onLastFrame。</param>
    /// <param name="fadeIn">视频开始播放时黑幕淡入</param>
    /// <param name="isLoop">当视频播放到最后一帧时自动回到第一帧</param>
    /// <param name="isAutoPlay">当视频准备好时，自动开始播放</param>
    public void ChangeMovie(VideoClip clip, Action onReady = null, Action onLastFrame = null, Action onVideoUnsuuport = null, bool fadeIn = true, bool isLoop = false, bool isAutoPlay = true)
    {
        this.UnsupportGroup.gameObject.SetActive(false);
        this.onReady = onReady;
        this.onLastFrame = onLastFrame;
        this.onVideoUnsuuported = onVideoUnsuuport;
        this.isAutoPlay = isAutoPlay;
        //var isSupportVideo = HotLocalSettings.IsSuppurtVideo;
        //var isSupportVideo = DeveloperLocalSettings.IsSuppurtVideo;
        //if (!isSupportVideo)
        //{
        //    url += ".test";
        //}
        //this.VieoPlayer.url = url;
        this.VieoPlayer.clip = clip;
        this.VieoPlayer.isLooping = isLoop;
        this.VieoPlayer.Prepare();
        if (fadeIn)
        {
            this.Image_frontBg.SetActive(true);
            this.Image_frontBg.color = Color.black;
            this.Image_frontBg.DOKill(false);
        }
        else
        {
            this.Image_frontBg.SetActive(false);
            this.Image_frontBg.DOKill(false);
        }
    }

    public void Pause()
    {
        this.VieoPlayer.Pause();
    }

    public void Stop()
    {
        this.VieoPlayer.Stop();
    }

    public void Play()
    {
        var clipName = this.VieoPlayer.clip.name;
        //var movieName = Path.GetFileNameWithoutExtension(clipName);
        WwiseEventManager.SendEvent(TransformTable.MovieStart, clipName);
        this.VieoPlayer.Play();
    }

    public void Replay()
    {
        this.VieoPlayer.frame = 0;
        this.Play();
    }

    void FadeOutFrontBlack()
    {
        var tween = this.Image_frontBg.DOFade(0, 0.5f);
        tween.Restart();
    }
}