using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRun : MonoBehaviour
{

    public GameObject target;
    public float delayTime;
    public float moveSpeed;
    public float moveLenght;
    // Start is called before the first frame update
    void Start()
    {
        target.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (delayTime > 0.0f)
        {
            delayTime -= Time.deltaTime;
            if (delayTime <= 0.0f)
            {
                delayTime = -1.0f;
                target.SetActive(true);
            }
            return;
        }
        if (moveLenght > 0.0)
        {
            var ml = Time.deltaTime * moveSpeed;
            moveLenght -= ml;
            target.transform.position += Vector3.right * ml;
            if (moveLenght <= 0.0f)
            {
                target.SetActive(false);
            }
        }
    }
}
