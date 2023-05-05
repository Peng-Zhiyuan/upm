using PathfindingCore;
using PathfindingCore.RVO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RichAIExtend : RichAI
{
    //private RVOController controller = null;
    private NavmeshCut navCut = null;
    // Start is called before the first frame update
    private void Awake()
    {
        navCut = this.coreObject.AddComponent<NavmeshCut>();
        navCut.circleRadius = 0.3f;
        navCut.rectangleSize = new Vector2(0.6f, 0.6f);
    }
    void Start()
    {
        //controller = this.gameObject.GetComponent<RVOController>();
    }

    protected override void OnTargetReached()
    {
        Stop();
    }

    public void SetTarget(Vector3 target)
    {           
        //if (controller == null)
            //return;
        //controller.locked = false;
        canMove = true;
        navCut.IsEnabled = false;
        destination = target;

    }

    public void Stop()
    {
        navCut.IsEnabled = true;
        SetPath(null);
        destination = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        canMove = false;

        //if(controller)
        //controller.locked = true ;
    }

    // Update is called once per frame
    //void Update()
    //{
    //    base.Update();
    //}
}
