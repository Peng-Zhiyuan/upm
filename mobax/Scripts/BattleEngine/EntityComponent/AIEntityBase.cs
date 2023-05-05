namespace BattleEngine.Logic
{
    using UnityEngine;
    using Sirenix.OdinInspector;

    public class AIEntityBase<T> : MonoBehaviour
    {
        [ShowInInspector]
        public T mData;

        public virtual void Init(T _data)
        {
            mData = _data;
        }
    }
}