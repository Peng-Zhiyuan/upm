using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MoveAnim : MonoBehaviour
{
    //public float  duration = 1;
    public float delay = 0;
    private float time = -1;

    private Vector3 cur_pos = Vector3.zero;

    private Vector3 next_pos = Vector3.zero;
    public bool loop = false;
    public List<Vector3> pathList;
    private int curIndex = 0;
    public float speed = 1;
    private float moveTime = 0;
    private float delayTime = 0;
    public Image mSprite;
    public bool autoHide = false;
  



     void Start()
     {
         if(speed == 0 || mSprite == null) return;
         delayTime =  delay;
         if(delayTime > 0 && autoHide)
         {
             this.mSprite.enabled = false;
         }
         MoveNext();
     }

    private void Update()
    {
        if(this.time < 0) return;
        if(delayTime >=0)
        {
            delayTime -= Time.deltaTime;
            if(delayTime < 0 && autoHide)
            {
                this.mSprite.enabled = true;
            }
            return;
        }
     
        this.time += Time.deltaTime;
        if(this.time <= moveTime)
        {
            this.transform.localPosition = Vector3.Lerp(this.cur_pos,this.next_pos,this.time/moveTime);
        }
        else 
        {
            MoveNext();
        }

    }

    public void MoveNext()
    {
        
        int nextIndex = curIndex +1;
        if(nextIndex >= pathList.Count)
        {
            if(!loop)
            {
                if(autoHide) this.mSprite.enabled = false;
                return;
            }
            nextIndex = 0;
        }
        this.cur_pos = this.pathList[curIndex];
        this.next_pos = this.pathList[nextIndex];
        this.moveTime = Vector3.Distance(this.cur_pos, this.next_pos)/speed;
        this.time  = 0;
    }
}
