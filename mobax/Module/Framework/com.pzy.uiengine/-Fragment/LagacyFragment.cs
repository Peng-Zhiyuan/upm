using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 碎片是 Page 中的页签
/// 由 Page 自行管理生命周期
/// </summary>
public class LagacyFragment : MonoBehaviour
{
    public FragmentManager fragmentManager;
    //public Page parentPage;
    /// <summary>
    /// 切换到这个碎片
    /// </summary>
    public virtual void OnSwitchedTo()
    {
        
    }

    /// <summary>
    /// 从这个碎片切换走
    /// </summary>
    public virtual void OnSwitchedFrom()
    {
        
    }

    public virtual void OnPageNavigatedTo(PageNavigateInfo info)
    {
        
    }
}
