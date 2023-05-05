#if UNITY_2017_1_OR_NEWER
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Pegasus
{
    [TrackColor(0.09f, 0.45f, 0.8f)]
    [TrackMediaType(TimelineAsset.MediaType.Script)]
    [TrackClipType(typeof(PegasusClip))]
    //[TrackBindingType(typeof(PegasusManager))]
    public class PegasusTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            // Set the display name of the clip to match the pegasus manager
            foreach (var c in GetClips())
            {
                PegasusClip pegasus = (PegasusClip)c.asset;
                PegasusManager manager = pegasus.PegasusManager.Resolve(graph.GetResolver());
                c.displayName = manager == null ? "Pegasus" : manager.name;
            }

            var mixer = ScriptPlayable<PegasusMixerBehaviour>.Create(graph);
            mixer.SetInputCount(inputCount);
            return mixer;
        }
    }
}
#endif