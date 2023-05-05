using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class RealtimeReflection : MonoBehaviour
{

    public float minX = 20;
    public float maxX = 30;
    public Vector3 Offset = Vector3.zero;
    ReflectionProbe probe;

    public List<BoxCollider> ReflectionRegions;
    void Awake()
    {
        probe = GetComponent<ReflectionProbe>();
    }

    public float UpdateInterval = 0.3f;
    float LastUpdateTime = 0;
    void Update()
    {
        if (Camera.main == null)
            return;


        if (Camera.main.transform.position.x < minX || Camera.main.transform.position.x > maxX)
        {
            // if (probe.enabled)
            // {
            //     probe.enabled = false;
            // }
            return;
        }
        
        // if (!probe.enabled)
        // {
        //     probe.enabled = true;
        // }
        //
       
        
        probe.transform.position = new Vector3(
            Camera.main.transform.position.x,
            Camera.main.transform.position.y * -1,
            Camera.main.transform.position.z
        ) + Offset;
        var hasReflectionRegion = false;
        foreach (var region in ReflectionRegions) {
            if (region != null && probe.bounds.Intersects(region.bounds)) {
                hasReflectionRegion = true;
                break;
            }
        }
        if (hasReflectionRegion && probe.refreshMode == UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting) {
            if (Time.time > LastUpdateTime + UpdateInterval) {
                probe.RenderProbe();
                LastUpdateTime = Time.time;
            }
        }
    }
    public void LogPos ()
    {
        Debug.LogFormat("Reflection probe pos: {0}, {1}, {2}", probe.transform.position.x, probe.transform.position.y, probe.transform.position.z);
    }
}