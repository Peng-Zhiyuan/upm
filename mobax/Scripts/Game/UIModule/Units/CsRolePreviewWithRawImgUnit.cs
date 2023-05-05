using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System;
public class RoleViewConfData
{
    public float maxOffset;
    public float minOffset;
    public float lookMinOffset;
    public float lookMaxOffset;
}

public enum CameraViewMode
{ 
    NORMAL,
    LEFT,
    FAR
}

public enum CameraCtrlMode
{
    LOCK_NEAR,
    LOCK_FAR,
    UNLOCK_FAR,
    UNLOCK_NEAR,
}

public partial class CsRolePreviewWithRawImgUnit : MonoBehaviour
{
    //public RolePreviewCtrl rolePreviewCtrl;
    public UITouch uiTouch;
    public CsRolePreviewRoot previewRoot;
    public CustomCameraController ccc;
    private int _roleId;
    private int _index;

    private float _originDistance;
    private float _originOffsetX;
    private float _originOffsetY;
    private float _originOffsetZ;

    private Dictionary<int, RoleViewConfData> confDic;
    private Dictionary<int, RoleViewConfData> ConfDic
    {
        get 
        {
            if (confDic == null)
            {
                confDic = new Dictionary<int, RoleViewConfData>();
                var confData0 = new RoleViewConfData();

                //far
                confData0.maxOffset = -5.25f;
                confData0.lookMinOffset = -0.5f;

                //near
                confData0.minOffset = -6.5f;
                confData0.lookMaxOffset = -0.5f;
                confDic[0] = confData0;

                var confData1 = new RoleViewConfData();
                //far
                confData1.maxOffset = -5.25f;
                confData1.lookMinOffset = -0.41f;

                //near
                confData1.minOffset = -7.3f;
                confData1.lookMaxOffset = -0.26f;
                confDic[1] = confData1;

                var confData2 = new RoleViewConfData();
                //far
                confData2.maxOffset = -4.8f;
                confData2.lookMinOffset = -0.38f;

                //near
                confData2.minOffset = -7.04f;
                confData2.lookMaxOffset = -0.08f;
                confDic[2] = confData2;

                var confData5 = new RoleViewConfData();
                //far
                confData5.maxOffset = -4.8f;
                confData5.lookMinOffset = -0.38f;
                //near
                confData5.minOffset = -7.5f;
                confData5.lookMaxOffset = 0f;
                confDic[5] = confData5;
             
            }
            return confDic;

        }

    }

    public void SwitchCameraMode(int heroId, CameraViewMode viewMode, bool forceLookCamera, bool smooth = true)
    {
    
        this.ccc.OpenSmoothDamp = smooth;
        switch (viewMode)
        {
            default:
            case CameraViewMode.NORMAL:
                {
                   // this.ccc.m_Distance = 12;
                    this.ccc.m_SideXOffset = 0;
                    this.SetCameraMode(heroId, CameraCtrlMode.UNLOCK_FAR, forceLookCamera);
                   // this.HeroGo.GetComponent<Animation>().CrossFade("idle", 0.2f);
                   // var rr = this.HeroGo.GetComponent<RoleRender>();
                   // rr.ShowWeapon(true);
                    break;
                }
            case CameraViewMode.LEFT:
                {
                   // this.ccc.m_Distance = 12;
                    this.ccc.m_SideXOffset = 0.14f;
                    this.SetCameraMode(heroId, CameraCtrlMode.LOCK_NEAR, forceLookCamera);
     
                  //  var rr = this.HeroGo.GetComponent<RoleRender>();
                 //   rr.ShowWeapon(false);
                    break;
                }
            case CameraViewMode.FAR:
                {
                    // this.ccc.m_Distance = 20;
                    this.SetCameraMode(heroId, CameraCtrlMode.LOCK_FAR, forceLookCamera);
                    this.ccc.m_SideXOffset = 0;
                    break;

                }
        }
    }

    public void Awake()
    {
        // 取消平滑过渡动画
        this.Main_Camera.OpenSmoothDamp = false;
        this._originDistance = this.Main_Camera.m_Distance;
        this._originOffsetX = this.Main_Camera.m_SideXOffset;
        this._originOffsetY = this.Main_Camera.m_SideYOffset;
        this._originOffsetZ = this.Main_Camera.m_SideZOffset;
    }

  /*  public void OnDisable()
    {
        this.Main_Camera.m_Distance = this._originDistance;
        this.Main_Camera.m_SideXOffset = this._originOffsetX;
        this.Main_Camera.m_SideYOffset = this._originOffsetY;
        this.Main_Camera.m_SideZOffset = this._originOffsetZ;
    }*/

    public CustomCameraController MainCamera => this.Main_Camera;

    public GameObject HeroGo => this.RolePreviewRoot.HeroGo;


    public void Clean()
    {
        this.Main_Camera.m_Distance = this._originDistance;
        this.Main_Camera.m_SideXOffset = this._originOffsetX;
        this.Main_Camera.m_SideYOffset = this._originOffsetY;
        this.Main_Camera.m_SideZOffset = this._originOffsetZ;
        //this.RolePreviewRoot.OnDisable();
    }

    public async Task SetData(int roleId, int avatarId)
    {
        this._roleId = roleId;

        var roleView = RolePreviewRoot;
        await roleView.SetData(roleId, avatarId);
        if (null != roleView)
        {
            roleView.SetLookAt(this.Main_Camera);
        }
    }

    public async Task SetData(int roleId, (int, int, int, long) avatarInfo)
    {
        this._roleId = roleId;

        var roleView = RolePreviewRoot;
        await roleView.SetData(roleId, avatarInfo);
        if (null != roleView)
        {
            roleView.SetLookAt(this.Main_Camera);
        }
    }

    public void SetCameraMode(int heroId, CameraCtrlMode cameraMode, bool forceLookCamera)
    {

        var bodyType = StaticData.HeroTable[heroId].bodyType;
        if (uiTouch != null && this.ConfDic != null && this.ConfDic.ContainsKey(bodyType))
        {
            uiTouch.SetConfData(this.ConfDic[bodyType], cameraMode, forceLookCamera);
        }
    }

    public void SetIndex(int index)
    {
        this._index = index;
    }

    public async void SetCameraByData(ERegulateModule type, int roleId)
    {
        var address = RoleRegulateConfig.GetDataName(type, roleId);
        Bucket Bucket =  BucketManager.Stuff.GetBucket(UIEngine.LatestNavigatePageName);
        var data = await Bucket.GetOrAquireAsync(address, true) as RoleRegulateData2;
        var camera = this.Main_Camera;
        if (data == null)
        {
            camera.m_Distance = this._originDistance;
            camera.m_SideXOffset = this._originOffsetX;
            camera.m_SideYOffset = this._originOffsetY;
            camera.m_SideZOffset = this._originOffsetZ;
            return;
        }

        var info = data.info;
        camera.m_Distance = this._originDistance + info.distance;
        camera.m_SideXOffset = this._originOffsetX + info.offsetX;
        camera.m_SideYOffset = this._originOffsetY + info.offsetY;
        camera.m_SideZOffset = this._originOffsetZ + info.offsetZ;
    }

    public void SetCamera(RolePreviewCameraInfo cameraInfo)
    {
        this.Main_Camera.m_SideYOffset = cameraInfo.cameraPos.x;
        this.Main_Camera.m_SideXOffset = cameraInfo.cameraPos.y;
        this.Main_Camera.m_Distance = cameraInfo.distance;
        this.Main_Camera.m_VAngle = cameraInfo.vAngle;
        this.Main_Camera.m_HAngle = cameraInfo.hAngle;
    }

    public void SetHAngle(float hAngle)
    {
        this.Main_Camera.m_HAngle = hAngle;
    }
}

public class RolePreviewCameraInfo
{
    // 相机位置
    public Vector2 cameraPos = Vector2.zero;

    // 缩放
    public float scale = 1;

    // 显示距离  默认是5
    public float distance = 5;

    public float vAngle = 0;
    public float hAngle = 0;
}