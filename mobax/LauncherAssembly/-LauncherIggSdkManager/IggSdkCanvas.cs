using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GPC.Helper.Jingwei.Script.Account;
using GPC.Helper.Jingwei.Script.AgreementSigning;
using GPC.Helper.Jingwei.Script.AppRating;
using GPC.Helper.Jingwei.Script.Common.Unity;
using GPC.Helper.Jingwei.Script.Common.Utils;
using GPC.Modules.AppRating.UI;


public class IggSdkCanvas : MonoBehaviour
{
    public GameObject accountManagerBindItem;
    public GameObject closeAccountItem;

    void Start()
    {
        AccountUIHelper.accountManagerBindItem = accountManagerBindItem;
        AccountUIHelper.closeAccountItem = closeAccountItem;
    }

    public bool AnyPannelEnabled
    {
        get
        {
            var count = this.transform.childCount;
            for(int i = 0; i < count; i ++)
            {
                var child = this.transform.GetChild(i);
                if(child.gameObject.activeSelf)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public Transform HighestActivePannel
    {
        get
        {
            var count = this.transform.childCount;
            for (int i = count - 1; i >= 0; i--)
            {
                var child = this.transform.GetChild(i);
                if (child.gameObject.activeSelf)
                {
                    return child;
                }
            }
            return null;
        }
    }

}
