/****************************************************    
    作者：Administrator
    日期：2020/9/15    
*****************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonState : MonoBehaviour {
    public Button btn;
    public GameObject activeBtn;

    public void ActiveBtn (bool active) {
        if (btn != null) {
            btn.enabled = active;
        }
        if (activeBtn != null)
            activeBtn.SetActive (active);
    }
}