using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
public class CreateTree : MonoBehaviour
{
    public ExternalBehaviorTree behaviorTree;   
    void Start () {
        throw new System.Exception("[CreateTree] CoreObject do not support prefab");
      //var bt = gameObject.AddComponent<Behavior>();      
      //bt.ExternalBehavior = behaviorTree;      
      //bt.StartWhenEnabled = false;   
    }
}
