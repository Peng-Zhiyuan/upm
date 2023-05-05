using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechShow : MonoBehaviour
{
    void Start()
    {
        Init();
    }

    async void Init()
    {
        //await GameService.InitAsync();
        //await GameService.LoadStaticData(false);

        var address = SpineUtil.Instance.GetSpinePath(showHeroId);
        //var tr = await AddressableRes.AquireAsync<Object>(address);
        var bucket = BucketManager.Stuff.Battle;
        var tr = await bucket.GetOrAquireAsync<Object>(address);

        if (tr == null)
        {
            return;
        }
        var role = GameObject.Instantiate(tr) as GameObject;
        role.transform.SetLocalScale(SpineUtil.Instance.GetSDScale(showHeroId));
        role.transform.position = Vector3.zero;
        cameraC.m_Target = role.transform;
        var roleRender = role.GetComponent<RoleRender>();
        if (roleRender != null)
        {

           // roleRender.subRender.transform.LookAt(cameraC.transform);
            

            roleRender.SwitchPlayMode(false);
        }
        refGirl.transform.LookAt(cameraC.transform);
        cameraC.m_Distance = 10;
    }

    public void OnCameraLenValueChange(float v)
    {
        cameraC.m_Distance = v;
    }

    private int showHeroId = 50005;
    public CustomCameraController cameraC;
    public GameObject refGirl;
}
