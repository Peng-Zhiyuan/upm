using System;
using BattleSystem.Core;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum AngleMode
{
    ANIGLE_90,
    ANIGLE_120,
    ANIGLE_150,
}
public enum SpeedMode
{
    Low,
    Normal,
    Fast,
}
public class DebugData : MonoBehaviour
{
    [HideInInspector]
    public string uid;

    private Creature _curCre;

    public float MoveSpeed;
    public float TurnSpeedRate = 0.8f;
    public float RotateSpeed = 180;
    [Range(0, 1)]
    public float MixDefault = 0.2f;
    public float MixTurnTime = 0.2f;
    public float VisionRange;
    //public float AttackRange;
    public AngleMode attackAngleMode;
    public SpeedMode CameraSpeed = SpeedMode.Normal;

    public float HAngle;
    public float VAngle;
    public float Distance;
    public float SideOffsetX;
    public float SideOffsetY;
    public float FOV;


    public void SetCreature(Creature tmp)
    {
        _curCre = tmp;

        ReLoad();
    }

    public void ReLoad()
    {
        if(true)
            return;
        
       


        //相机参数
        var proxy = CameraManager.Instance.CameraProxy;
        HAngle = proxy.m_HAngle;
        VAngle = proxy.m_VAngle;
        Distance = proxy.Distance;
        SideOffsetX = proxy.m_SideOffset;
        SideOffsetY = proxy.m_LookOffsetY;
        FOV = proxy.FieldOfView;
    }

   

    public void OnValidate()
    {
        
    }

    protected void OnDrawGizmos()
    {

        if (_curCre != null)
        {
            Gizmos.color = Color.blue;
            Vector3 lastPosition = _curCre.transform.position;
            Gizmos.DrawLine(lastPosition, lastPosition + _curCre.transform.forward * 3f);
        }
    }

    private bool ShowAttr = false;
    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            ShowAttr = !ShowAttr;
        }
    }

    public void OnGUI()
    {
        /*if (_curCre.Selected && ShowAttr)
        {
            GUI.skin.label.fontSize = 30;
            GUI.skin.label.alignment = TextAnchor.UpperLeft;

            string str = "";
            foreach (var VARIABLE in _curCre.RoleItemData.RoleAttr.GetAllAttr())
            {
                str += VARIABLE.Key + "\t" + VARIABLE.Value + "\n";
            }
            GUI.Label( new Rect(10, 10, 800, 1800), str);
        }*/
    }
}
