using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class CameraShakeBehaviour : PlayableBehaviour
{
    [NonSerialized] public TimelineClip Clip;
    public float Intensity;

    public override void OnPlayableCreate (Playable playable)
    {
        
    }
}
