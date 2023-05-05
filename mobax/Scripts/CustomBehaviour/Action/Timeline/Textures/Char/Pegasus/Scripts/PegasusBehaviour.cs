#if UNITY_2017_1_OR_NEWER
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Pegasus
{
    [Serializable]
    public class PegasusBehaviour : PlayableBehaviour
    {
        [HideInInspector]
        public PegasusManager pegasusManager;
        [HideInInspector]
        public float pegasusProgress;
    }
}
#endif
