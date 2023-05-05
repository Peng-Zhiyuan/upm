using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public partial class PageExtraParam : MonoBehaviour
{
    /// <summary>
    /// 是否可以附加 TopFloating
    /// </summary>
    public bool attachTopFloating = true;

    /// <summary>
    /// 是否显示屏幕底部菜单栏
    /// </summary>
    [ShowIf(nameof(attachTopFloating))]
    public bool HasMenu = true;

    /// <summary>
    /// 当此页面在最上时，block 的显示风格
    /// </summary>
    //public BlockStyle blockStyle = BlockStyle.Normal;

}


