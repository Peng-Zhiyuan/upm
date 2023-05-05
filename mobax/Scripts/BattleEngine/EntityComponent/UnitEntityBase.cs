namespace BattleEngine.Logic
{
    using UnityEngine;

    public class UnitEntityBase<T> : AIEntityBase<T>
    {
        public void SetParent(Transform parent)
        {
            transform.SetParent(parent);
        }

        public void SetLocalPosition(Vector3 pos)
        {
            transform.localPosition = pos; //new Vector3(pos.x, pos.y, pos.z);
        }

        public void SetLocalRotation(float y)
        {
            transform.localEulerAngles = new Vector3(0, y, 0);
        }

        protected virtual void UnitUpdate() { }
    }
}