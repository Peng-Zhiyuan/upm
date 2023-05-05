using UnityEngine;
namespace Neatly {
    public class NeatlyBehaviour : MonoBehaviour {
        public bool IsEnable { get; private set; }
        public bool IsDestroy { get; private set; }

        protected virtual void OnEnable() {
            IsEnable = true;
        }

        protected virtual void OnDisable() {
            IsEnable = false;
        }

        protected virtual void OnDestroy() {
            IsDestroy = true;
        }
    }
}
