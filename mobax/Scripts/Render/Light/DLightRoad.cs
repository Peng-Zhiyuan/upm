using System;
using System.Collections;
using UnityEngine;
public class DLightRoad : MonoBehaviour
{
    private void Start()
    {
        throw new Exception("猜测不使用的代码");
        //for (int i = 0; i < lights.Length; i++)
        //{
        //    lights[i].SetActive(false);
        //}
        //SceneEventManager.CoreStuff.RegisterListener("dynLightOn", LightOnDel);
    }

    void LightOnDel(int arg)
    {
       // Debug.Log("=================HereA");
        StartCoroutine(PlayEffect());
    }
    
    
    IEnumerator PlayEffect()
    {
        lights[0].SetActive(true);
        lights[1].SetActive(true);
        var tick = 0.0f;
        var index = 2;
        while (index<6)
        {
            tick += Time.deltaTime;
            if (tick >= indevTime)
            {
                tick = 0;
                lights[index].SetActive(true);
                lights[index+1].SetActive(true);
                index += 2;
            }
            yield return null;
        }
    }
#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            Start();
            StartCoroutine(PlayEffect());
        }
    }
    #endif
    public float indevTime = 0.3f;
    public GameObject[] lights;
}
