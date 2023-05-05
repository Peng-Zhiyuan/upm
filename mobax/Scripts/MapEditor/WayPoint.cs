using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour
{
    public GameObject SelectedNode;
    public GameObject UnselectNode;

    public void SetSelect (bool active)
    {
        if (SelectedNode != null)
            SelectedNode.SetActive(active);
        if (UnselectNode != null)
            UnselectNode.SetActive(!active);
    }
}
