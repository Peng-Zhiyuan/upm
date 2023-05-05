using System;
using System.Collections.Generic;
using UnityEngine;

public class CheckClick : MonoBehaviour
{
    public Transform UpUI;
    public List<Transform> UIList = new List<Transform>();

    public void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                Debug.LogError(hit.collider.name);
            }
        }
    }

    public void Check()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(UpUI.position);
        if (Physics.Raycast(ray, out hit))
        {
            Debug.LogError(hit.collider.name);
        }
    }
}