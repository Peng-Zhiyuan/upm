using UnityEngine;

namespace BattleEngine.Logic
{
    public class ComponentView : MonoBehaviour
    {
        public string Type;
        public object Component { get; set; }
    }
}