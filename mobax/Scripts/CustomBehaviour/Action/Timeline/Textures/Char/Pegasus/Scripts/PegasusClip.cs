#if UNITY_2017_1_OR_NEWER
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Pegasus
{
    [Serializable]
    public class PegasusClip : PlayableAsset, ITimelineClipAsset
    {
        public ExposedReference<PegasusManager> PegasusManager;
        [Range(0f, 1f)]
        public float PegasusProgress;

        [HideInInspector]
        public PegasusBehaviour template = new PegasusBehaviour ();

        public ClipCaps clipCaps
        {
            get { return ClipCaps.Blending; }
        }

        public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<PegasusBehaviour>.Create (graph, template);
            playable.GetBehaviour().pegasusManager = PegasusManager.Resolve(graph.GetResolver());
            playable.GetBehaviour().pegasusProgress = PegasusProgress;
            return playable;
        }
    }
}
#endif