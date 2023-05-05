using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class UIEffectSort : MonoBehaviour 
 { 
   public int sortingOrder = 100;
//    public int SortingOrder 
//    {
//        get
//        {
//            return sortingOrder;
//        }
//        set
//        {
//            if(sortingOrder != value)
//            {
//                 sortingOrder = value;   
//                 this.RefreshRender();        
//            }
//        }
//    }
   #if UNITY_EDITOR
   private int lastSort = 100;
    void OnUpdate()
    {
        if(lastSort != sortingOrder)
        {
            lastSort = sortingOrder;   
            this.RefreshRender();        
        }
         
    }
   #endif
     private Renderer[] m_EffectRend; 
     void OnEnable() 
     { 
        //获取脚本下所有Renderer
        m_EffectRend = this.GetComponentsInChildren<Renderer>(true);
        //遍历Renderer 
        this.RefreshRender();
    }

    private void AutoRefreshRender()
    {
        var sort =  this.transform.GetSiblingIndex();
        for (int i = 0; i < m_EffectRend.Length; i++)
        {
            m_EffectRend[i].sortingOrder = sort; //设置层级
        }
        Debug.LogError("sort:"+sort);
    }
   

    private void RefreshRender()
    {
        
        for (int i = 0; i < m_EffectRend.Length; i++)
        {
            m_EffectRend[i].sortingOrder = sortingOrder; //设置层级
        }
       // Debug.LogError("sortingOrder:"+sortingOrder);
    }
    
}
