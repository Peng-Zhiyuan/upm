using System;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public class DeadTest : MonoBehaviour
{
    public RoleRender rolerender;

    public Transform fx_dead_boss;
    public Transform bone;

    [OnValueChanged("OnValueChanged")]
    public bool ShowDead;
    
    private void OnValueChanged()
    {
        if(!ShowDead)
            return;
        
        if(rolerender == null || fx_dead_boss == null)
            return;
        
        rolerender.Dead();
        
        fx_dead_boss.SetParent(bone.transform);
        fx_dead_boss.localPosition = Vector3.zero;
        fx_dead_boss.localScale = Vector3.one;
        fx_dead_boss.SetActive(false);
        fx_dead_boss.SetActive(true);
    }

    private void Awake()
    {
       // MatsPool.Instance.CacheMaterialAsync("Dead");
        fx_dead_boss.SetActive(false);
    }

    /*private async Task PlayDead()
    {
        await Task
    }*/
}