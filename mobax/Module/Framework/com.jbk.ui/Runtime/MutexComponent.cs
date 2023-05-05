using UnityEngine;

/** 要显示互斥内容时用这个组件（即只显示其中一个组件）， 把两个状态的GameObject都拖上去就可以了 */
public class MutexComponent : MonoBehaviour
{
    [Tooltip("设置选中时显示的图片")]
    public GameObject status1;
    [Tooltip("设置不选中时显示的图片")]
    public GameObject status2;

    [SerializeField]
    private bool _selected;

    public bool Selected
    {
        get => _selected;
        set
        {
            if (null != status1) status1.SetActive(value);
            if (null != status2) status2.SetActive(!value);
            _selected = value;
        }
    }

    public int Status
    {
        get => Selected ? 1 : 2;
        set
        {
            var selected = value == 1;
            Selected = selected;
        }
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        Selected = _selected;
    }
#endif
}