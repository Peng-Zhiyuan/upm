using System;
using System.Collections;
using System.Collections.Generic;
using BattleEngine.Logic;
using BattleSystem.Core;
using Cinemachine;
using Unity.Mathematics;
using UnityEngine;



public class CameraState_Defence : BaseCameraState
{
    protected override CameraState GetState()
    {
        return CameraState.Defence;
    }
    
    private float m_FOV = 85f;

    private Transform Npc;

    protected override void OnStateEnter(object param_UserData)
    {
        CameraSetting.Ins.SetMaxMinRot(180, -180);
        
        Proxy.Distance = 6;
        Proxy.m_HAngle = 30;
        Proxy.SideOffsetY = 4.35f;
        Proxy.m_HAngleDamp = 10f;
        Proxy.FieldOfView = this.m_FOV;
        Proxy.m_SideOffset = 0f;
        CameraSetting.Ins.SetDeadZone(0, 0);

        var role = param_UserData as Creature;
        Npc = role.transform;

        SceneObjectManager.Instance.LocalPlayerCamera.SetPosition(new Vector3(-46f, 2f, 0));

        SceneObjectManager.Instance.LocalPlayerCamera.SetDirection(role.GetDirection());
        CameraSetting.Ins.SetFollow(SceneObjectManager.Instance.LocalPlayerCamera.transform);
        CameraSetting.Ins.LookAt(SceneObjectManager.Instance.LocalPlayerCamera.transform);
    }
    protected override void OnStateLeave()
    {
    }

    private void ResetCameraPostion()
    {
        var hero = SceneObjectManager.Instance.FindCreatureByConfigID(BattleConst.ShuiJinID);;
        if (hero.ConfigID == BattleConst.ShuiJinID)
        {
            Proxy.SideOffsetY = 4.35f;
            

            /*var mon = FindFarrestMonster();
            if(mon == null)
                return;*/
            
            if(SceneObjectManager.Instance.CurSelectHero == null)
                return;

            var mon = SceneObjectManager.Instance.CurSelectHero; 

            var dir = mon.GetPosition() - Npc.position;
            var pos = Vector3.Magnitude(dir) * dir.normalized * 4 / 5 + Npc.position;
            SceneObjectManager.Instance.LocalPlayerCamera.SetPosition(pos + new Vector3(0, 0, -0.5f));

            var dis = 4 + Vector3.Distance(pos + new Vector3(0, 0, -0.5f), hero.transform.position) * 6f / 10;
            dis = Mathf.Min(dis, 10);
            Proxy.Distance = dis;
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
        
        UpdateCamera(param_deltaTime);

        ResetCameraPostion();
    }

    private void UpdateCamera(float param_deltaTime)
    {
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
        /*var role = SceneObjectManager.Instance.FindCreatureByConfigID(BattleConst.SiLaLiID);
        if (role != null)
        {
            var dis = Vector3.Distance(SceneObjectManager.Instance.LocalPlayerCamera.GetPosition(), role.GetPosition());
            dis = fDisMin + (dis) * (fDisMax - fDisMin) / 11f;
            dis = Mathf.Clamp(dis, fDisMin, fDisMax);
            Proxy.Distance = dis;
        }*/
        
        

       

        
/*
        Proxy.FieldOfView =
            (fDisMax - dis) * (MaxFov - MinFov) / (fDisMax - fDisMin) + MinFov;
        
        Proxy.SideOffsetY =
            (dis - fDisMin) * (XMax - XMin) / (fDisMax - fDisMin) + XMin;
            */
        
        //UpdateLockState(param_deltaTime);
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
        //var role = SceneObjectManager.Instance.CurSelectHero;
        //if (role != null)
            //startH = role.CamereAngleRot.y;

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
