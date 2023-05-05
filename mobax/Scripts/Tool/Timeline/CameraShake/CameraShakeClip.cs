using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class CameraShakeClip : PlayableAsset, ITimelineClipAsset
{
    public CameraShakeBehaviour template = new CameraShakeBehaviour ();
    [NonSerialized] public TimelineClip clipPassthrough = null;

    public ClipCaps clipCaps
    {
        get { return ClipCaps.None; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        template.Clip = clipPassthrough;
        var playable = ScriptPlayable<CameraShakeBehaviour>.Create (graph, template);

        return playable;
    }
}
