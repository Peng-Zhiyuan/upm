using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public partial class RedNoticeUnit : MonoBehaviour
{
    public string noticeName;
    public Transform root; 
    public Text info;
    private void OnEnable() 
    {
        if(string.IsNullOrEmpty(noticeName)) return;
        RedNoticeCenter.Instance.RegisterNotice(noticeName,this);
        noticeName = "";
        root.gameObject.SetActive(false);
    
    }
    private void OnDisable() 
    {
        if(string.IsNullOrEmpty(noticeName)) return;
        RedNoticeCenter.Instance.UnRegisterNotice(noticeName);
        noticeName = "";
    }
    private void Show(string info)
    {
        if(!root.gameObject.activeSelf)
        {
            root.gameObject.SetActive(true);
    
        }
        if (this.info != null) {
            if (string.IsNullOrEmpty (info)) {
                this.info.enabled = false;
            } else {
                this.info.text = info;
                this.info.enabled = true;
            }
        }
    }

    private void Hide()
    {
        if(root.gameObject.activeSelf)
        {
            root.gameObject.SetActive(false);
         
        }
    }

    public void Refresh(bool isShow,string info = null)
    {
        if(isShow)
        {
            Show(info);
        }
        else
        {
            Hide();
        }
    }
}
