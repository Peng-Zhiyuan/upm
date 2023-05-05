using UnityEngine;

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class SharedTransform : SharedVariable<CoreTransform>
    {
        public static implicit operator SharedTransform(CoreTransform value) { return new SharedTransform { mValue = value }; }
    }
}