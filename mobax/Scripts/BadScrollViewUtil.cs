using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public static class BadScrollViewUtil 
{
    public static List<T> GetViewList<T>(ScrollView badScrollView) where T : Component
    {
        var list = badScrollView.VirtualItemList;
        var sotedList = from view in list where view.active orderby view.virtualIndex select view.transform.GetComponent<T>();
        return sotedList.ToList();
    }

}
