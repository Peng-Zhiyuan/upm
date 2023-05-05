using System;
using System.Collections;
using System.Collections.Generic;
using ScRender;
using UnityEngine;
using Object = UnityEngine.Object;

public class LightShow : MonoBehaviour
{
    void Start()
    {
        Init();
    }
    async void Init()
    {
        //await GameService.InitAsync();
        //await GameService.LoadStaticData(false);

        var address = SpineUtil.Instance.GetSpinePath(spineId);
        //var tr = await AddressableRes.AquireAsync<Object>(address);
        var bucket = BucketManager.Stuff.Battle;
        var tr = await bucket.GetOrAquireAsync<Object>(address);

        if (tr == null)
        {
            return;
        }
        role= GameObject.Instantiate(tr) as GameObject;
        role.transform.position=Vector3.zero;
        cameraCon.m_Target = role.transform;
        roleRender = role.GetComponent<RoleRender>();
        if (roleRender != null)
        {
         
           // roleRender.subRender.transform.rotation = cameraCon.transform.rotation;
            roleRender.SwitchPlayMode(false);
            roleRender.Init();
        }
        camera = cameraCon.GetComponent<Camera>();
    }

    public void Update()
    {
        if (Input.GetMouseButton(0))
        {
            // //control light
            // var mpos = Input.mousePosition;
            // var ray=camera.ScreenPointToRay(mpos);
            // RaycastHit result;
            // if (Physics.Raycast(ray, out result))
            // {
            //     shadower.transform.position=mainLightTrans.position = result.point+Vector3.up;
            //     shadower.dir = Vector3.Normalize(shadower.transform.position);
            // }
        }
        if (Input.GetMouseButton(1))
        {
            //control pos
            var mpos = Input.mousePosition;
            var ray=camera.ScreenPointToRay(mpos);
            RaycastHit result;
            if (Physics.Raycast(ray, out result))
            {
               Move(result.point);
            }
        }

        if (moveTick > 0.0f)
        {
            moveTick -= Time.deltaTime;
            if (moveTick < 0.0f)
            {
                moveTick = -1f;
                role.transform.position = targetPos;
            }
            else
            {
                role.transform.position = Vector3.Lerp(beginPos, targetPos, 1.0f-moveTick / moveTime);
            }
          
        }
    }

    void Move(Vector3 pos)
    {
        targetPos = pos;
        float dis = Vector3.Distance(role.transform.position, targetPos);
        if (dis<= 0.1f)
        {
            return;
        }
        beginPos = role.transform.position;
        moveTick = moveTime = dis / moveSpeed;
    }


    public void selDel(int _sel)
    {
        youhuaStep = _sel;
        roleRender.SwitchKeyword("_MAGIC_ON",_sel>=1);
    }


    public static int youhuaStep = 0;
    

    private const int spineId = 50005;
    public  CustomCameraController cameraCon;
    private Camera camera;
    public Transform mainLightTrans;
    private GameObject role;
    private Vector3 targetPos;
    private Vector3 beginPos;
    private float moveTick;
    private float moveTime;
    public float moveSpeed = 1.0f;
    public DLightNode shadower;
    private RoleRender roleRender;
}
