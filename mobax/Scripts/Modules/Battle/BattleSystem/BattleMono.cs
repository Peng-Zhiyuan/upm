using UnityEngine;

public class BattleMono : MonoSingleton<BattleMono>
{
    void Update()
    {
        Battle.Instance.Update();
    }

    void LateUpdate()
    {
        Battle.Instance.LateUpdate();
    }

    void FixedUpdate()
    {
        Battle.Instance.FixedUpdate();   
    }
    
    
}