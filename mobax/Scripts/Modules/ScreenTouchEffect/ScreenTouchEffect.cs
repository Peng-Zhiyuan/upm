using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class ScreenTouchEffect : MonoBehaviour
{
    private GameObject particle = null;
    private void Start()
    {
        LoadParticle();
    }

    private  async void LoadParticle()
    {
        var address = "fx_ui_dianji_2.prefab";
        var go = await BucketManager.Stuff.Main.GetOrAquireAsync<GameObject>(address, true);
        if (go == null)
        {
            Debug.Log($"[ProjectBattleCore] graph address '{address}' not found");
            return;
        }
        particle = Instantiate(go);

       // particle = GetComponent<ParticleSystem>();
    }


    public void Update()
    {
        if (InputProxy.TouchUp())
        {

            if (particle != null)
            {
     
                var  pos = InputProxy.TouchPosition();
                particle.transform.SetParent(UIEngine.Stuff.Canvas.transform);
                particle.transform.position = pos;
                particle.transform.localScale = Vector3.one;
                particle.SetActive(false);
                particle.SetActive(true);
            }
        }
    }
}
