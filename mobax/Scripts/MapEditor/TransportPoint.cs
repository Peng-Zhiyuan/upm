using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleSystem.Core;
using System.Collections;
using UnityEngine;
using System;
using ScMap;
using Sirenix.OdinInspector;
public class TransportPoint : GizmosMonoBehaviour
{
    private bool open = true;
    public TransportPoint target;
    public void OnReset()
    {
        open = true;
    }
    public void OnClose()
    {
        open = false;
    }

    [ShowInInspector]
    public bool IsOpened
    {
        get
        {
            return open;
        }
    }
    public List<SceneObjectType> triggerTargetType = new List<SceneObjectType>();
    private HashSet<SceneObjectType> triggerTargetHash = new HashSet<SceneObjectType>();

    private void Awake()
    {
        for (int i = 0; i < triggerTargetType.Count; i++)
        {
            triggerTargetHash.Add(triggerTargetType[i]);
        }
    }



#if UNITY_EDITOR
    //public TriggerType triggerType = TriggerType.MISC;
    public override void OnGizmos()
    {
        Color boxColor = Color.green;
        Gizmos.color = boxColor;
        var box = this.GetComponent<BoxCollider>();
        Vector3 s = box.transform.localScale;
        Vector3 size = box.transform.rotation * new Vector3(s.x * box.size.x, s.y * box.size.y, s.z * box.size.z);
        Gizmos.DrawWireCube(box.transform.position + box.center, size);

    }
#endif

    //private float stayTime = -1;
    public void OnTriggerEnter(Collider other)
    {
        if (open)
        {
            var agent = other.gameObject.GetComponent<RoleAgent>();
            if (agent != null && this.triggerTargetHash.Contains(agent.sceneObjectType))
            {
                //stayTime = 0;
                this.Transport(agent._id);
               // TriggerSequenceManager.Ins.AddState(this);
                open = false;
            }
        }
    }

/*    public void OnTriggerStay(Collider other)
    {
        if (stayTime >= 0)
        {
            stayTime += Time.deltaTime;
            var agent = other.gameObject.GetComponent<RoleAgent>();
            if (agent != null && this.triggerTargetHash.Contains(agent.sceneObjectType))
            {
                this.Transport(agent._id);
                // TriggerSequenceManager.Ins.AddState(this);
                open = false;
            }
        }
    }*/

    private void Transport(string _id,float duration = 0)
    {
        throw new System.Exception("猜测不使用的代码");
        //if (this.target == null) return;
        //var pos = target.transform.position;

        //CoreAction.CoreStuff.Transport(_id, pos);
    }

    public void OnUndo()
    {
        if (open)
        {

        }
    }
}
