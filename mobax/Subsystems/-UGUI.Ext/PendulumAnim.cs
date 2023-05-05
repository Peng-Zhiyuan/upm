using UnityEngine;
using System.Collections;

public class PendulumAnim : MonoBehaviour
{
    float mTime;
    public int strengh = 30;
    public float duration = 1;
    public float delay = 0;
    private void Update()
    {
        mTime += Time.deltaTime;
        float t = (1+Mathf.Sin(delay+2 * Mathf.PI * this.mTime/duration))/2;
        float zRotation = Mathf.Lerp(-strengh,strengh,t);
        this.transform.localEulerAngles = new Vector3(0,0,zRotation);
    }
}
