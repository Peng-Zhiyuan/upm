using PathfindingCore;
using PathfindingCore.RVO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class CustomAI : CustomAIPath
{
    // Start is called before the first frame update
    public bool checkGround = true;
    public bool ReachedDestination 
    {
        get 
        {
            return this.isStopped || this.reachedEndOfPath;// || this.reachedDestination;
        }
    }

/*   public bool HasGround(Vector3 pos)
    {
        RaycastHit hit;
        if (Physics.Raycast(pos + Vector3.up, Vector3.down, out hit, 100, groundMask, QueryTriggerInteraction.Ignore))
        {
            return hit.point.y >= CoreConf.minGroundOffest;
        }
        return false;
    }*/

    /*    public bool UsingGravity
        {
            get 
            {
                return this.usingGravity;
            }
            set 
            {
                this.usingGravity = value;
            }
        }
    */
    /*    private Vector3 RaycastGround(Vector3 pos, float rayLength)
        {
            RaycastHit hit;
            if (Physics.Raycast(position + Vector3.up, Vector3.down, out hit, rayLength, LayerMask.NameToLayer("Default"), QueryTriggerInteraction.Ignore))
            {
                verticalVelocity *= System.Math.Max(0, 1 - 5 * lastDeltaTime);
                return hit.point;
            }
            return pos;
        }
        public Vector3 CheckGround()
        {
            Vector3 v3 = RaycastGround(this.position, 100);
            return v3;
        }*/


    /*    public bool CanMove
        {
            get
            {
                return this.canMove;
            }
        }*/

    public void ClearPathNow()
    {
        this.ClearPath();
    }

    public void ResetRotation(Vector3 dir)
    {
        this.rotation = Quaternion.FromToRotation(Vector3.forward, dir);
        rotationFilterState = new Vector2(dir.x, dir.z);
        rotationFilterState2 = new Vector2(dir.x, dir.z);
    }
    //protected override  void Update()
    //{

    //}
    /*   public override void SearchPath()
       {
           if (float.IsPositiveInfinity(destination.x)) return;

           if (onSearchPath != null) onSearchPath();
           *//*			Debug.LogError("SearchPath");*//*
        lastRepath = Time.time;
        waitingForPathCalculation = true;

        seeker.CancelCurrentPathRequest();

        Vector3 start, end;
        CalculatePathRequestEndpoints(out start, out end);
        // Alternative way of requesting the path
        //ABPath p = ABPath.Construct(start, end, null);
        //seeker.StartPath(p);

        // This is where we should search to
        // Request a path to be calculated from our current position to the destination
        seeker.StartPath(start, end, ApplyGroundCheck);

    }*/
    public void UpdatePathDestination(Vector3 destinationPos)
    {

        if (this.hasPath && this.path.CompleteState == PathCompleteState.Complete)
        {
            this.destination = destinationPos;
            
            int lastWayPointIndex = this.path.vectorPath.Count - 1;
            if (this.interpolator.segmentIndex + 1 >= lastWayPointIndex)
            {
                this.path.vectorPath.Add(destinationPos);
            }
            else 
            {
                var minDistance = float.MaxValue;
                int index = lastWayPointIndex;
                for (int i = this.interpolator.segmentIndex + 1; i <= lastWayPointIndex; i++)
                {
                    var sqrDistance = Vector3.SqrMagnitude(this.path.vectorPath[i] - destinationPos);
                    if (sqrDistance < minDistance)
                    {
                        minDistance = sqrDistance;
                        index = i;
                    }
                }
         /*     if (lastWayPointIndex - index > 0)
                {
                    Debug.LogError("remove:"+ (lastWayPointIndex - index));
                }*/
                this.path.vectorPath[index] = destinationPos;
                this.path.vectorPath.RemoveRange(index + 1, lastWayPointIndex - index);
            }
            this.interpolator.UpdateDistance();
        }
    }

    //public void UpdateDistance()
    //{
    //    currentSegmentLength = (path[1] - path[0]).magnitude;
    //    totalDistance = 0f;

    //    var prev = path[0];
    //    for (int i = 1; i < path.Count; i++)
    //    {
    //        var current = path[i];
    //        totalDistance += (current - prev).magnitude;
    //        prev = current;
    //    }
    //}

   /* public void ApplyGroundCheck(Path p)
    {
       //if (!checkGround) return;
       //for (int i = 1; i < p.vectorPath.Count; i++)
       // {
       //     if (!PathSystem.Instance.HasGround(p.vectorPath[i]))
       //     {
       //         p.vectorPath.RemoveRange(i, p.vectorPath.Count - i);
       //         break;
       //     }
       // }
       // if (p.vectorPath.Count > 0)
       // {
       //     if (p.vectorPath.Count == 1)
       //     {
       //         p.vectorPath.Add(p.vectorPath[0]);
       //     }
       //     this.destination = p.vectorPath[p.vectorPath.Count - 1];
       //     this.interpolator.SetPath(p.vectorPath);
       // }
       
    }*/

    public bool ShouldRecalculatePath
    {
        get 
        {
            return this.shouldRecalculatePath;
        }
    }

    protected override void OnUpdate(float dt)
    {

    }

}
