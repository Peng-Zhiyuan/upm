using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.855f, 0.8623f, 0.87f)]
[TrackClipType(typeof(CameraShakeClip))]
[TrackBindingType(typeof(Transform))]
public class CameraShakeTrack : TrackAsset
{
    public CameraShakeMixerBehaviour template = new CameraShakeMixerBehaviour();
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var playable = ScriptPlayable<CameraShakeMixerBehaviour>.Create(graph, template, inputCount);

        var clips = GetClips();
        foreach (var clip in clips) {
            var pClip = clip.asset as CameraShakeClip;
            pClip.clipPassthrough = clip;
        }
        return ScriptPlayable<CameraShakeMixerBehaviour>.Create (graph, inputCount);
    }
}
