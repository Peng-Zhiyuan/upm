// Timeline Particle Control Example
// https://github.com/keijiro/TimelineParticleControl

using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Klak.Timeline {

// Playable clip class for particle system control

[System.Serializable]
public class ParticleSystemControlPlayable : PlayableBehaviour
{
        [NonSerialized] public TimelineClip Clip;
        public float rateOverTime = 10;
    public float rateOverDistance = 0;
}

}
