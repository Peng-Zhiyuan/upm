using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugRoleInfo : MonoBehaviour
{
    public string uid;
    public Vector3 ClientPos;
    public float Hp;
    public float MaxHp;


    public State roleState;
    
    public Vector3 forward;

    public float MoveSpeed;

    private Creature _curCre;
	public CameraState cameraState;
    public Transform Target;

    public string AnimName = "";

    public void SetCreature(Creature tmp)
    {
        _curCre = tmp;
    }

    public void Clear()
    {
        _curCre = null;
        uid = "";
    }

//#if UNITY_EDITOR
//    void OnDrawGizmos()
//    {
//        GizmosTool.DrawCircle(transform.position, 2, Vector3.up * 0.3f, Color.yellow);
//    }
//#endif

    void Update()
    {
        if (_curCre != null)
        {
            uid = _curCre.ID;
      

            ClientPos = _curCre.GetPosition();

            forward = _curCre.transform.forward;
           

            //roleState = (State)_curCre._stateMachine.CurrentState.GetStateID();
            
            /*var track = _curCre.SKAnimation.AnimationState.GetCurrent(0);
            if (track != null)
            {
                AnimName = track.Animation.Name;
            }*/
            if(_curCre.Target != null)
            Target = _curCre.Target.transform;

        }
    }
}
