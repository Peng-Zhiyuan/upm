using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleEngine.Logic;
using BattleSystem.Core;
using Cinemachine;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;



public class CameraState_Free : BaseCameraState
{
    protected override CameraState GetState()
    {
        return CameraState.Free;
    }

    private float m_HAngle = 0f;
    private float m_VAngle = 15f;
    private float m_Distance = 5.9f;
    private float m_FOV = 85f;
    private float m_OffsetSpeed = 0f;
    private Creature FocusEnemyTarget = null;
    private int ProtectTime = 3000;

    protected override void OnStateEnter(object param_UserData)
    {
        //m_StateMachine.ChangeState((int)CameraState.FixLookAt, param_UserData);
        CameraSetting.Ins.SetDeadZone(0.131f, 0.115f, 0.412f, 0.336f);
        //CameraSetting.Ins.SyncPosImmediate();
        /*if (Owner.ShowTransition)
        {
            Owner.ShowTransition = false;
            TransitionEffectController com =
                CameraManager.Instance.MainCamera.GetComponentInChildren<TransitionEffectController>();
            if (com != null)
            {
                com.ShowEffect();
                Proxy.DelayTime = 0.2f;
            }
        }
        else
        {
            Proxy.AutoMove = true;
            Proxy.SetSpeed = false;
            Proxy.HAngleOffset = 0f;
            //Proxy.ReginPos = Owner.MainCamera.transform.position;
            
            
            /*float y = Vector3.Cross(Owner.MainCamera.transform.position - Owner.Target.position, CameraManager.Instance.MainCamera.transform.position - Owner.Target.position).y;
            if (y < 0)
            {
                Offsetm_HAngle += param_deltaTime * OffsetSpeed;
            }
            else
            {
                m_HAngleOffset += param_deltaTime * (-OffsetSpeed);
            }#1#
        }

        Proxy.HOffsetAngle = 0f;*/
        //Proxy.ZOffset = 0.5f;
        //InitParam();
        
        Debug.Log("自由相机");
        CameraSetting.Ins.SetMaxMinRot(180, -180);

        Proxy.m_HAngle = 0;
        Proxy.m_VAngle = this.m_VAngle;
        //Proxy.m_CurrentHAngle = 0;
        if (Battle.Instance.IsArenaMode)
        {
            Proxy.Distance = 13;
            if(UnityEngine.Random.Range(0, 100) < 50)
                Proxy.m_HAngle = -15;
            else
            {
                Proxy.m_HAngle = 15;
            }
            Proxy.SideOffsetY = 4.35f;
            Proxy.m_HAngleDamp = 10f;
        }
        else
        {
            Proxy.Distance = 5.5f;
            Proxy.SideOffsetY = 2.5f;
        }

        //Proxy.RecoveHAngleImmediate();
        Proxy.FieldOfView = this.m_FOV;
        Proxy.m_SideOffset = 0f;

        Proxy.m_HAngle = 15f;
        
        CaculateDir();

        /*PostProcessHandler.SetPostEffectActive<DepthOfField>(CameraManager.Instance.MainCamera.gameObject, true);
        PostProcessHandler.SetPostEffectActive<Vignette>(CameraManager.Instance.MainCamera.gameObject, false);
        PostProcessHandler.SetPostEffectActive<Bloom>(CameraManager.Instance.RTCamera.gameObject, false);
        PostProcessHandler.SetPostEffectActive<Bloom>(CameraManager.Instance.MainCamera.gameObject, true);
        PostProcessHandler.SetPostEffectActive<MotionBlur>(CameraManager.Instance.MainCamera.gameObject, false);*/
       // var fowVolume =  PostProcessHandler.GetPostEffect<FoW.FogOfWarURP>(CameraManager.Instance.MainCamera.gameObject);
       // fowVolume.fogColor = new UnityEngine.Rendering.ColorParameter(new Color(0, 0, 0, 0.851f));

        //var blitFeature = RenderFeatureHandler.Ins.GetRenderFeatureByName("ColorBlendBlit") as Blit;
        //blitFeature.settings.blitMaterial.SetColor("_BlendColor", new Color(0, 0, 0, 0));
        
        GameEventCenter.AddListener(GameEvent.CameraRoll, this, this.CameraRoll);
        GameEventCenter.AddListener(GameEvent.FocusEnemy, this, this.FocusEnemy);
        GameEventCenter.AddListener(GameEvent.ShowBreakCamera, this, this.DoBreakCameraAnim);

        //PlayerModeChanged(null);
        if(!Battle.Instance.IsArenaMode)
            Proxy.FollowSelectHero();
        else
        {
            var localplayer = SceneObjectManager.Instance.TryCreateLocalPlayerCamera();
            /*TimerMgr.Instance.BattleSchedulerTimer(2f, delegate
            {
                localplayer.transform.position =
                    Battle.Instance.GetStageCenter();
            });*/

            //SceneObjectManager.Instance.TryCreateLocalPlayerCamera().transform.localPosition = new Vector3(0, 0, 2f);
            
            CameraSetting.Ins.SetFollow(SceneObjectManager.Instance.TryCreateLocalPlayerCamera().transform);
            //if(role.Target != null)
            //CameraSetting.Ins.LookAt(role.GetBone("body_hit"));
            CameraSetting.Ins.LookAt(SceneObjectManager.Instance.LocalPlayerCamera.transform);
        }

        ResetHAngle();

        CameraSetting.Ins.SetBindingMode(CinemachineTransposer.BindingMode.LockToTargetNoRoll);
        TimerMgr.Instance.BattleSchedulerTimerDelay(2f, delegate
        {
            //CameraSetting.Ins.SetBindingMode(CinemachineTransposer.BindingMode.LockToTarget);
        });
        
    }
    protected override void OnStateLeave()
    {
        //GameEventCenter.RemoveListener(GameEvent.PlayerModeChanged, this, this.PlayerModeChanged);
        GameEventCenter.RemoveListener(GameEvent.CameraRoll, this);
        GameEventCenter.RemoveListener(GameEvent.FocusEnemy, this);
        GameEventCenter.RemoveListener(GameEvent.ShowBreakCamera, this);
    }

    private void FocusEnemy(object[] data)
    {
        FocusEnemyTarget = data[0] as Creature;
        TimerMgr.Instance.BattleSchedulerTimerDelay(2f, () => { FocusEnemyTarget = null;});
    }

    private float DurTime = 3f;
    public override void LateUpdate(float param_deltaTime)
    {
        
        
        //OnPinch();

        if (FocusEnemyTarget != null && !FocusEnemyTarget.mData.IsDead)
        {
            CameraSetting.Ins.LookAt(FocusEnemyTarget.GetBone("body_hit"));
            CameraSetting.Ins.SetFollow(FocusEnemyTarget.GetBone("body_hit"));
        }
        else
        {
            if(!Battle.Instance.IsArenaMode)
                Proxy.FollowSelectHero();
        }
        

        if (DurTime > 0)
        {
            DurTime -= param_deltaTime;
            if (DurTime <= 0)
            {
                DurTime = 3f;
                //CameraSetting.Ins.SetOffsetX(UnityEngine.Random.Range(-30, 30));

                //Proxy.m_HAngle = UnityEngine.Random.Range(-30, 30);
            }
        }
        
        Proxy.LateUpdate(param_deltaTime);
        
        CheckAreanCamera();
        
        UpdateCamera(param_deltaTime);
    }

    private void UpdateCamera(float param_deltaTime)
    {
        if(isBreakUpdate)
            return;
        
        if (fResetTime <= 0)
            return;
        
        if (fResetTime > 0)
        {
            fResetTime -= param_deltaTime;
            if (fResetTime <= 0)
            {
                ResetHAngle();
            }
        }
        
        //var angle = hero.CamereAngleRot;
        //Proxy.m_HAngle = Mathf.Lerp(Proxy.m_HAngle, fTargetAngle, param_deltaTime * speed);
        //float distCovered = (UnityEngine.Time.time - startTime) * speed;
        //float fracJourney = distCovered / startH;
        //angle.y = Mathf.Lerp(angle.y, fTargetAngle, fracJourney);
        //Proxy.m_HAngle += param_deltaTime * speed;
        Proxy.m_HAngle = Mathf.Clamp(Proxy.m_HAngle + param_deltaTime * speed, hMin, hMax);
        //angle.y = startH;
        //Proxy.m_HAngle = startH;
        
        //hero.CamereAngleRot = angle;
        
        /*Proxy.SideOffsetY = Mathf.Lerp(Proxy.SideOffsetY, fTargetX, param_deltaTime * speed2);
        Proxy.SideOffsetY = Mathf.Clamp(Proxy.SideOffsetY, XMin, XMax);*/
        //Proxy.SideOffsetY = startV;
        //Proxy.SideOffsetY = Mathf.Lerp(Proxy.SideOffsetY, fTargetX, param_deltaTime * speed2);

        var dis = fDisMin + (Proxy.m_HAngle - hMin) * (fDisMax - fDisMin) / (hMax - hMin);
        dis = Mathf.Clamp(dis, fDisMin, fDisMax);
        Proxy.Distance = dis;

        

        Proxy.FieldOfView =
            (fDisMax - dis) * (MaxFov - MinFov) / (fDisMax - fDisMin) + MinFov;
        
        Proxy.SideOffsetY =
            (dis - fDisMin) * (XMax - XMin) / (fDisMax - fDisMin) + XMin;
        
        //UpdateLockState(param_deltaTime);
    }

    private float lastDis = 0;
    private bool isBreakUpdate = false;
    private void DoBreakCameraAnim(object[] data)
    {
        isBreakUpdate = true;
        lastDis = Proxy.Distance;
        DOTween.Kill("BreakCameraAnim");
        var seq = DOTween.Sequence();
        var dis1 = Proxy.Distance - 0f;
        float t1 = 0.07f;
        var tween1 = DOTween.To(() => Proxy.Distance, x => Proxy.Distance = x, dis1, t1);
        /*tween1.onComplete = delegate
        {
            //CameraSetting.Ins.Shake(2, 10f, 0.1f);
        };*/
        seq.Append(tween1);
        //seq.AppendInterval(0.3f);
        float t2 = 0.05f;
        var tween2 = DOTween.To(() => Proxy.Distance, x => Proxy.Distance = x, lastDis, t2);
        seq.Append(tween2);
        //seq.onUpdate();
        seq.onComplete = delegate
        {
            Proxy.Distance = lastDis;
            isBreakUpdate = false;
            
            
        };
    }


    private float fLockTime = 3f;
    public void UpdateLockState(float param_deltaTime)
    {
        var role = SceneObjectManager.Instance.CurSelectHero;
        if(role == null)
            return;
        
        var heroRow = StaticData.HeroTable.TryGet(role.ConfigID);
        if(heroRow == null)
            return;

        if (heroRow.Range == 2)
        {
            CameraSetting.Ins.SetBindingMode(CinemachineTransposer.BindingMode.LockToTarget);
            return;
        }

        if (fLockTime > 0)
        {
            fLockTime -= param_deltaTime;
            if (fLockTime <= 0)
            {
                //ResetLockState();
            }
        }
    }

    private void ResetLockState()
    {
        if (IsLock)
        {
            CameraSetting.Ins.SetBindingMode(CinemachineTransposer.BindingMode.LockToTargetOnAssign);
            fLockTime = 3f;
        }
        else
        {
            CameraSetting.Ins.SetBindingMode(CinemachineTransposer.BindingMode.LockToTarget);
            fLockTime = 1f;
        }

        IsLock = !IsLock;
    }

    public bool IsLock
    {
        get;
        set;
    } = true;

    public bool IsClockDir = true;
    private float fTargetAngle = 15f;
    private float speed = 1f;
    public float hMax = 20f;
    public float hMin = 10f;
    public float durT = 5f;
    public float fResetTime = 0;
    public float fDisMax = 4.5f;
    public float fDisMin = 3.5f;
    
    public float XMax = 2f;
    public float XMin = 1f;
    public float fTargetX = 0.7f;
    public float speed2 = 2f;
    public float speedK = 100f;

    public float startH;
    public float startV;
    public float startTime;

    public float MinFov = 90f;
    public float MaxFov = 108f;

    private void InitParam()
    {
        hMax = UIController.Instance.hMax;
        hMin = UIController.Instance.hMin;
        durT = UIController.Instance.durT;
        fDisMax = UIController.Instance.fDisMax;
        fDisMin = UIController.Instance.fDisMin;
        XMax = UIController.Instance.XMax;
        XMin = UIController.Instance.XMin;
        //hMax = UIController.Instance.hMax;
    }
    private async void ResetHAngle()
    {
        //var role = SceneObjectManager.Instance.CurSelectHero;
        //if (role != null)
            //startH = role.CamereAngleRot.y;
            if (ProtectTime > 0)
            {
                await Task.Delay(ProtectTime);
                if(!Battle.Instance.IsFight)
                    return;
                ProtectTime = 0;
            }
        await Task.Delay(UnityEngine.Random.Range(1000,1500));
        if(!Battle.Instance.IsFight)
            return;

        startV = fTargetX;
        
        IsClockDir = !IsClockDir;
        /*if (IsClockDir)
        {
            var temp = UnityEngine.Random.Range(hMin, (hMax + hMin)/2);
            startH = fTargetAngle;
            speed = Mathf.Abs(temp - fTargetAngle) / durT;
            fTargetAngle = temp;
            

            var temp2 = UnityEngine.Random.Range(XMin, (XMax + XMin)/2);
            speed2 = (temp2 - fTargetX) / durT;
            fTargetX = temp2;
        }
        else
        {
            var temp = UnityEngine.Random.Range((hMax + hMin)/2, hMax);
            startH = fTargetAngle;
            speed = (temp - fTargetAngle) / durT;
            fTargetAngle = temp;
            
            var temp2 = UnityEngine.Random.Range((XMax + XMin)/2, XMax);
            speed2 = (temp2 - fTargetX) / durT;
            fTargetX = temp2;
        }*/

        var vec = new Vector2(5, hMax - hMin);
        var temp = UnityEngine.Random.Range(5, hMax - hMin);
        if (UnityEngine.Random.Range(0, 100) > 50)
        {
            temp = -temp;
        }

        if (fTargetAngle + 5 > hMax)
        {
            temp = -Mathf.Abs(temp);
        }

        if (fTargetAngle - 5 < hMin)
        {
            temp = Mathf.Abs(temp);
        }
        
        var temp_target = fTargetAngle + temp;
        temp_target = Mathf.Clamp(temp_target, hMin, hMax);
        startH = temp_target;
        speed = (temp_target - fTargetAngle) / durT;
        fTargetAngle = temp_target;
            
        
        var temp2 = UnityEngine.Random.Range(0.2f, XMax - XMin);
        if (UnityEngine.Random.Range(0, 100) > 50)
        {
            temp2 = -temp2;
        }

        if (fTargetX + 0.2f > XMax)
        {
            temp2 = -Mathf.Abs(temp2);
        }

        if (fTargetX - 0.2f < XMin)
        {
            temp2 = Mathf.Abs(temp2);
        }
        
        var temp_target2 = fTargetX + temp2;
        temp_target2 = Mathf.Clamp(temp_target2, XMin, XMax);

        startV = temp_target2;
        speed2 =  Mathf.Abs(Proxy.SideOffsetY - fTargetX) / durT;
        fTargetX = temp_target2;
        
        

        startTime = UnityEngine.Time.time;
        fResetTime = durT;
    }

    private void CheckAreanCamera()
    {
        if(!Battle.Instance.IsArenaMode)
            return;

        float offset = 10;
        if (Proxy.m_CurrentHAngle > offset)
        {
            Proxy.m_HAngle = -offset;
        }
        else if (Proxy.m_CurrentHAngle < -offset)
        {
            Proxy.m_HAngle = offset;
        }
    }

    private void CameraRoll(object[] data)
    {
        if(Battle.Instance.IsArenaMode)
            return;
        
        bool bRot;
        bRot = (bool) data[0];
        if (bRot == false)
        {
            Proxy.HAngleOffset = 0;
            Proxy.m_HAngleDamp = 0.2f;
            return;
        }

        
        /*Proxy.m_HAngleDamp = 6f;
        if(UnityEngine.Random.Range(0, 10) > 5)
            Proxy.HAngleOffset = 30f;
        else
        {
            Proxy.HAngleOffset = -30f;
        }*/
    }
    
    public override void OnStateChangeRequest(int newState, object param_UserData)
    {
        /*switch ((CameraState)newState)
        {
            case CameraState.Front:
            case CameraState.Ready:
            case CameraState.Exchange:
            case CameraState.Skill:
            case CameraState.Settlement:
            case CameraState.LookAt:
                m_StateMachine.ChangeState(newState, param_UserData);
                break;
            default:
                break;
        }*/
        
        m_StateMachine.ChangeState(newState, param_UserData);
    }

    /*public override Vector3 GetTargetPos()
    {
        return SceneObjectManager.Instance.GetPlayerCenter();
    }*/

    private float distance = 0.0f;

    private float newdis = 0;
    private float olddis = 0;
    
    private void OnPinch()
    {
        float maxDistance = 2;
        float minDistance = -5f;
        float speed = 77f;

        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (Input.touchCount == 1)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    Proxy.HAngleOffset = 0;
                    Proxy.m_HAngleDamp = 0.2f;
                    Proxy.LocalHOffset += Mathf.Sign(InputProxy.TouchDelta().x) * UnityEngine.Time.deltaTime*speed * .5f;
                }
            }

            if (Input.touchCount == 2)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved)
                {
                    Vector3 s1 = Input.GetTouch(0).position;
                    Vector3 s2 = Input.GetTouch(1).position;
                    newdis = Vector2.Distance(s1,s2);
                    if(newdis > olddis)
                    {
                        distance -= UnityEngine.Time.deltaTime * speed;
                    }
                    if(newdis < olddis)
                    {
                        distance += UnityEngine.Time.deltaTime * speed;
                    }
                    distance = Mathf.Clamp(distance, minDistance, maxDistance);
                    Proxy.m_DistanceOffset = distance;
                    olddis = newdis;
                }
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                Proxy.m_DistanceOffset -= UnityEngine.Time.deltaTime * speed;
            }
            //Zoom in
            if (Input.GetKey(KeyCode.DownArrow))
            {
                Proxy.m_DistanceOffset += UnityEngine.Time.deltaTime * speed;
            }
            Proxy.m_DistanceOffset = Mathf.Clamp(Proxy.m_DistanceOffset,  minDistance, maxDistance);
        
            if (Input.GetKey(KeyCode.A))
            {
                Proxy.HAngleOffset = 0;
                Proxy.m_HAngleDamp = 0.2f;
                Proxy.LocalHOffset -= UnityEngine.Time.deltaTime*speed * .5f;
            }
        
            if (Input.GetKey(KeyCode.D))
            {
                Proxy.HAngleOffset = 0;
                Proxy.m_HAngleDamp = 0.2f;
                Proxy.LocalHOffset += UnityEngine.Time.deltaTime*speed * .5f;
            }
        }
        
        
    }
}
