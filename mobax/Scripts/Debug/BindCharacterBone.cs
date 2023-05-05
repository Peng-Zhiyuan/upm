using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BindItem
{
    public Transform root;
    public Transform sub;
}
public class BindCharacterBone : MonoBehaviour
{
    public List<Transform>Root = new List<Transform>();
    public List<Transform>Children = new List<Transform>();

    public Dictionary<Transform, Transform> bones = new Dictionary<Transform,Transform>();
    // Start is called before the first frame update
    void Start()
    {
        BattleTimer.Instance.DelayCall(2f, delegate(object[] objects)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].SetParent(Root[i]);
                Children[i].localPosition = Vector3.zero;
                Children[i].localRotation = Quaternion.identity;
            }
            this.gameObject.SetActive(false);
            this.gameObject.SetActive(true);
        });
        
    }
}
