using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnPlay : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // if (BattleManager.Instance.GetCurrentState() != eBattleState.None)
        // {
            Destroy(this.gameObject);
        // }
    }
}
