using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathfindingCore;
using PathfindingCore.RVO;
public class BlockWall : TriggerTarget
{
    public bool block = true;
    public bool useMeshCut = false;
    public NavmeshCut meshCut;

    // Start is called before the first frame update
#if UNITY_EDITOR
    public override void OnGizmos()
    {
        Gizmos.color = Color.red;
        var box = GetComponent<RVOSquareObstacle>();
        Vector3 s = box.Transform.localScale;
        Vector3 size = (Quaternion)box.Transform.rotation * new Vector3(s.x * box.size.x, s.y, s.z * box.size.y);
        Gizmos.DrawWireCube(this.transform.position + new Vector3(box.center.x, 0, box.center.y), size);
    }
#endif
    public override void OnReset()
    {
        if (block)
        {
            this.OnClose();
        }
        else
        {
            this.OnOpen();
        }
        meshCut.IsEnabled = useMeshCut;
    }

    public override void OnOpen()
    {
        this.gameObject.SetActive(false);
    }
    public override void OnClose()
    {
        this.gameObject.SetActive(true);
    }
    public override void OnSwitch()
    {
        if (this.gameObject.activeSelf)
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            this.gameObject.SetActive(true);
        }
    }
}
