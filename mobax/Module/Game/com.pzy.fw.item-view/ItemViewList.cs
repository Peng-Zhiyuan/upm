using System.Collections.Generic;
using UnityEngine;

public class ItemViewList : MonoBehaviour
{
    public DataListCreator dataList;
    
    public string RequireFormat { get; set; }
    
    public void Set(List<VirtualItem> itemList)
    {
        dataList.ViewSetter ??= _RenderItemView;
        dataList.DataList = itemList;
    }

    private void _RenderItemView(object info, Transform tf)
    {
        var itemView = tf.GetComponent<ItemView>();
        if (info is VirtualItem virtualItem)
        {
            itemView.Set(virtualItem);
        }
    }
}