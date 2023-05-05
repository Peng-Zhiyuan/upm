using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FxToolUti: MonoBehaviour
{
    // Start is called before the first frame update


    private void Awake()
    {

    }

    public void Reset()
    {
        if (delayActiveTime > 0)
        {
            if (mChild != null)
            {
                mChild.SetActive(false);
            }
            delayActiveTick = delayActiveTime;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (delayActiveTick > 0.0f)
        {
            delayActiveTick -= Time.deltaTime;
            if (delayActiveTick <= 0.0f)
            {
                delayActiveTick = -1.0f;
                if (mChild != null)
                {
                    mChild.SetActive(true);
                }
                   
            }
        }
        
    }
    public GameObject mChild;
    public float delayActiveTime = -1;
    float delayActiveTick = -1;
}
