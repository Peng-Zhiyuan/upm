using UnityEngine;


namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class SharedGameObject : SharedVariable<CoreObject>
    {
        public static implicit operator SharedGameObject(CoreObject value) { return new SharedGameObject { mValue = value }; }
    }
}