using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public interface IBattleComponent 
{
    public void Setup(Battle battle);

    public void OnBattleCreate();

    public Task OnLoadResourcesAsync();

    public void OnUpdate();

    public void LateUpdate();

    public void FixedUpdate();

    public void OnDestroy();

    public void OnCoreCreated();
}
