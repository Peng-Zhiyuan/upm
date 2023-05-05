using System;
using System.Collections;
using System.Collections.Generic;
using BattleEngine.Logic;
using BattleSystem.Core;
using Cinemachine;
using Unity.Mathematics;
using UnityEngine;



public class CameraState_Arena : BaseCameraState
{
    protected override CameraState GetState()
    {
        return CameraState.Arena;
    }
    
    private float m_FOV = 85f;

    private Transform Npc;

    protected override void OnStateEnter(object param_UserData)
    {
        CameraSetting.Ins.SetMaxMinRot(180, -180);
        
        Proxy.Distance = 10;
        Proxy.m_HAngle = 30;
        Proxy.SideOffsetY = 4.35f;
        Proxy.m_HAngleDamp = 10f;
        Proxy.FieldOfView = this.m_FOV;
        Proxy.m_SideOffset = 0f;
        CameraSetting.Ins.SetDeadZone(0, 0);
        
        CameraSetting.Ins.SetFollow(SceneObjectManager.Instance.LocalPlayerCamera.transform);
        CameraSetting.Ins.LookAt(SceneObjectManager.Instance.LocalPlayerCamera.transform);
    }
    protected override void OnStateLeave()
    {
        
    }

    private void ResetCameraPostion()
    {
        var hero = SceneObjectManager.Instance.CurSelectHero;
        if (hero.ConfigID == BattleConst.SiLaLiID)
        {
            Proxy.SideOffsetY = 4.35f;
            
            if(BattleManager.Instance.ActorMgr.GetCamp(1).Count == 0)
                return;

            var mon = FindFarrestMonster();
            if(mon == null)
                return;

            var dir = mon.GetPosition() - Npc.position;
            var pos = Vector3.Magnitude(dir) * dir.normalized * 3 / 5 + Npc.position;
            SceneObjectManager.Instance.LocalPlayerCamera.SetPosition(pos + new Vector3(0, 0, -0.5f));
        }
        else
        {
            /*fDisMax = 8f;
            fDisMin = 5f;*/
            Proxy.SideOffsetY = 3f;
            Transform target = null;
            var enemy = hero.GetTarget;
            if (enemy == null)
            {
                target = hero.transform;
            }
            else
            {
                if (Vector3.Distance(enemy.position, Npc.position) > Vector3.Distance(hero.GetPosition(), Npc.position))
                {
                    target = enemy.transform;
                }
                else
                {
                    target = hero.transform;
                }
            }
            
            if(target == null)
                return;
            
            var dir = target.position - Npc.position;
            var pos = Vector3.Magnitude(dir) * dir.normalized * 3 / 5 + Npc.position;
            
            SceneObjectManager.Instance.LocalPlayerCamera.SetPosition(pos + new Vector3(0, 0, -0.5f));
        }
        
    }

    private Creature FindFarrestMonster()
    {
        float dis = 0;
        Creature role = null;
        foreach (var VARIABLE in BattleManager.Instance.ActorMgr.GetCamp(1))
        {
            if(VARIABLE.mData.IsDead)
                continue;
            
            var temp = Vector3.Distance(Npc.position, VARIABLE.GetPosition());
            if(temp > 16f)
                continue;

            if (temp > dis)
            {
                dis = temp;
                role = VARIABLE;
            }
        }

        return role;
    }
    

   
    private float DurTime = 3f;
    public override void LateUpdate(float param_deltaTime)
    {
        Proxy.LateUpdate(param_deltaTime);
        
        //CheckAreanCamera();
        UpdateCenterPos();
        UpdateCamera(param_deltaTime);
    }

    public void UpdateCenterPos()
    {
        Vector3 pos = Vector3.zero;
        int count = 0;
        var list = BattleManager.Instance.ActorMgr.GetAllActors();
        foreach (var VARIABLE in list)
        {
            if (!VARIABLE.mData.IsDead)
            {
                pos += VARIABLE.GetPosition();
                count++;
            }
        }
        
        if(count == 0)
            return;

        pos = pos / count;
        pos.z = SceneObjectManager.Instance.LocalPlayerCamera.transform.position.z;
        //SceneObjectManager.Instance.LocalPlayerCamera.SetPosition(pos);
        SceneObjectManager.Instance.LocalPlayerCamera.transform.position =
            Vector3.MoveTowards(SceneObjectManager.Instance.LocalPlayerCamera.transform.position, pos, 2f);
    }

    private float fMaxAngle = 10f;
    private float fMinAngle = -10f;
    private float fSpeed = 2f;
    private float fMaxSpeed = 2f;

    private void UpdateCamera(float param_deltaTime)
    {
        if (Proxy.m_HAngle > fMaxAngle)
        {
            fSpeed = -fMaxSpeed;
        }

        if (Proxy.m_HAngle < fMinAngle)
        {
            fSpeed = fMaxSpeed;
        }

        Proxy.m_HAngle += fSpeed * param_deltaTime;
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
    public float hMax = 35f;
    public float hMin = 15f;
    public float durT = 4f;
    public float fResetTime = 2;
    public float fDisMax = 10f;
    public float fDisMin = 7f;
    
    public float XMax = 2.5f;
    public float XMin = 0.5f;
    public float fTargetX = 0.7f;
    public float speed2 = 2f;
    public float speedK = 100f;

    public float startH;
    public float startV;
    public float startTime;

    public float MinFov = 70f;
    public float MaxFov = 100f;

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
    private void ResetHAngle()
    {
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
        switch ((CameraState)newState)
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
        }
        
        //m_StateMachine.ChangeState(newState, param_UserData);
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
