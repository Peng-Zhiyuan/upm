using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeLineCtrl : MonoBehaviour
{
    //public AudioListener audioListener;
    public void Awake()
    {
        if (BattleStateManager.IsAccessable)
        {
            AudioListener audioListener = this.GetComponentInChildren<AudioListener>(true);
            if(audioListener != null) MonoBehaviour.Destroy(audioListener);
        }
      
    }
}
