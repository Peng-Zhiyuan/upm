using System.Collections.Generic;
using UnityEngine;

/** 单独只显示一组列表中组件的一个 */
public class SoloComponent : MonoBehaviour
{
    [Tooltip("设置选中时显示的图片")]
    public List<GameObject> list;
    
    [SerializeField]
    private int _selected;
    public int Selected
    {
        get => _selected;
        set
        {
            if (null != list)
            {
                for (var index = 0; index < list.Count; index++)
                {
                    var comp = list[index];
                    if (null != comp)
                    {
                        comp.SetActive(index == value);
                    }
                }
            }

            _selected = value;
        }
    }

    public void SetSelected(int index)
    {
        Selected = index;
    }

    public void SetSelected(Component comp)
    {
        SetSelected(comp.gameObject);
    }

    public void SetSelected(GameObject go)
    {
        SetSelected(list.IndexOf(go));
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        Selected = _selected;
    }
#endif
}