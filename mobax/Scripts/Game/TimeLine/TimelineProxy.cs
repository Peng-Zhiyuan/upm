using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public enum TimelineProxyPlayState
{
    Playing,
    Paused,
    Stopped
}

public class TimelineProxy : IDisposable
{
    private Dictionary<string, PlayableBinding> tracksMap;
    private Dictionary<string, Dictionary<string, PlayableAsset>> tracksClipsMap;
    private List<string> trackNames;

    public PlayableDirector Director { get; private set; }
    public PlayableAsset Asset { get; private set; }
    public TimelineProxyPlayState PlayState { get; private set; }

    public TimelineProxy()
    {
        PlayState = TimelineProxyPlayState.Stopped;
        tracksMap = new Dictionary<string, PlayableBinding>();
        tracksClipsMap = new Dictionary<string, Dictionary<string, PlayableAsset>>();
        trackNames = new List<string>();
    }

    public static PlayableDirector PreLoadDirector(GameObject go)
    {
        PlayableDirector director = go.GetComponent<PlayableDirector>();
        if (director == null)
        {
            Debug.LogError(go.name + " : PlayableDirector is null");
            return null;
        }
        director.playOnAwake = false;
        go.SetActive(false);
        return director;
    }

    public void SetAsset(PlayableDirector director)
    {
        Clear();
        this.Director = director;
        this.Asset = this.Director.playableAsset;
        foreach (var o in this.Asset.outputs)
        {
            var trackName = o.streamName;
            if (tracksMap.ContainsKey(trackName))
                continue;
            if (o.sourceObject == null)
                continue;
            tracksMap.Add(trackName, o);
            trackNames.Add(trackName);
            var trackAsset = o.sourceObject as TrackAsset;
            var clipList = trackAsset.GetClips();
            foreach (var c in clipList)
            {
                if (!tracksClipsMap.ContainsKey(trackName))
                {
                    tracksClipsMap[trackName] = new Dictionary<string, PlayableAsset>();
                }
                var map = tracksClipsMap[trackName];
                if (!map.ContainsKey(c.displayName))
                {
                    map.Add(c.displayName, c.asset as PlayableAsset);
                }
            }
        }
    }

    public string[] GetMatchingNames(string name)
    {
        List<string> names = new List<string>();
        string temp = string.Empty;
        for (int i = 0; i < trackNames.Count; i++)
        {
            temp = trackNames[i];
            if (temp.Contains(name))
                names.Add(temp);
        }
        return names.ToArray();
    }

    public bool ContainsMatchingName(string name)
    {
        return GetMatchingNames(name).Length > 0;
    }

    public void SkipFrame(float time)
    {
        if (Director)
            Director.time = time;
    }

    private PlayableBinding _tempBinding;
    private UnityEngine.Object value;

    public TimelineProxy SetBinding(string trackName, UnityEngine.Object o)
    {
        if (o == null)
            return null;
        _tempBinding = default(PlayableBinding);
        value = o;
        if (tracksMap.TryGetValue(trackName, out _tempBinding))
        {
            Director.SetGenericBinding(_tempBinding.sourceObject, value);
        }
        return this;
    }

    public PlayableBinding GetPlayableBinding(string trackName)
    {
        _tempBinding = default(PlayableBinding);
        if (tracksMap.TryGetValue(trackName, out _tempBinding))
        {
            return _tempBinding;
        }
        return _tempBinding;
    }

    public PlayableBinding GetTrack(string trackName)
    {
        _tempBinding = default(PlayableBinding);
        if (tracksMap.TryGetValue(trackName, out _tempBinding))
        {
            return _tempBinding;
        }
        return _tempBinding;
    }

    public T GetClip<T>(string trackName, string clipName) where T : PlayableAsset
    {
        Dictionary<string, PlayableAsset> track = null;
        if (tracksClipsMap.TryGetValue(trackName, out track))
        {
            PlayableAsset ret = null;
            if (track.TryGetValue(clipName, out ret))
            {
                return ret as T;
            }
            else
            {
                Debug.LogError("GetClip trackName: " + trackName + " not found clipName: " + clipName);
            }
        }
        else
        {
            Debug.LogError("GetClip not found trackName: " + trackName);
        }
        return null;
    }

    public void Clear()
    {
        if (trackNames != null)
        {
            trackNames.Clear();
        }
        if (tracksClipsMap != null)
        {
            tracksClipsMap.Clear();
        }
        if (tracksMap != null)
        {
            tracksMap.Clear();
        }
        if (tracksClipsMap != null)
        {
            foreach (var item in tracksClipsMap)
            {
                item.Value.Clear();
            }
            tracksClipsMap.Clear();
        }
        Asset = null;
        Director = null;
    }

    public void Dispose()
    {
        Clear();
        trackNames = null;
        tracksMap = null;
        tracksClipsMap = null;
    }
}