// Timeline Particle Control Example
// https://github.com/keijiro/TimelineParticleControl

using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Klak.Timeline {

// Clip asset class for particle system control

[System.Serializable]
public class ParticleSystemControlClip : PlayableAsset, ITimelineClipAsset
{
        [NonSerialized] public TimelineClip clipPassthrough = null;
        public ParticleSystemControlPlayable template = new ParticleSystemControlPlayable();

    public ClipCaps clipCaps { get { return ClipCaps.Blending; } }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
            template.Clip = clipPassthrough;
            return ScriptPlayable<ParticleSystemControlPlayable>.Create(graph, template);
    }
}

}
