using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.Serialization;
using System.Text;
using UnityEngine.EventSystems;
/// <summary>
/// 编辑器通用bar
/// 隐藏和展开
/// </summary>
///
/// 
[Serializable]
public class ToolBarData
{
    public bool _isOpen = true;
    public Vector2 _pos=Vector2.zero;
}
public class ToolBar : MonoBehaviour,IDragHandler,IBeginDragHandler,IEndDragHandler
{
    private void Awake()
    {
        _rtrans = transform.parent.GetComponent<RectTransform>();
        _rawWidth = _rtrans.rect.width;
        _rawPos = _rtrans.anchoredPosition;
        if (_root)
        {
            _isOpen = _root.activeSelf;
        }
        ToolStatic.Init(this);
        //LoadData();
        if (_data == null)
        {
            _data = new ToolBarData();
            _data._isOpen = _isOpen;
            _data._pos = _rtrans.position;
        }
    }

    public void SwitchOpen()
    {
        SwitchOpenLogic();
        if (_isOpen)
        {
            ToolStatic.Open(this);
        }
        else
        {
            ToolStatic.Dock(this);
        }
        SaveData();
    }

    public void SwitchOpenLogic()
    {
        _isOpen = !_isOpen;
        if (_root)
        {
            _root.SetActive(_isOpen);
        }
        if (_isOpen)
        {
            _rtrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,_rawWidth);
        }
        else
        {
            _rtrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,60f);
        }
    }



    void RePanel()
    {
        var bars= GameObject.FindObjectsOfType<ToolBar>();
        var x = 0f;
        var y = 0f;
        for (int i = 0; i < bars.Length; ++i)
        {
            if (bars[i] != this)
            {
                var r=bars[i].GetRect();
                Debug.Log($"{bars[i].transform.parent.name} xMin:{r.xMin}yMin:{r.yMin} localPos:{bars[i].GetRTrans().anchoredPosition}");
                x = Mathf.Min(x, bars[i].GetRTrans().anchoredPosition.x-r.width);
                y = Mathf.Min(y, bars[i].GetRTrans().anchoredPosition.y-r.height);
            }
        }

        Debug.Log($"Final x:{x}y:{y}");
        //_rtrans.anchoredPosition = new Vector2(x, y);
    }

    public Rect GetRect()
    {
        Rect rect = _rtrans.rect;
        if (!_isOpen)
        {
            rect.height = 30;
        }
        return rect;
    }

    // public Rect GetCompassRect()
    // {
    //     Rect rect = _rtrans.rect;
    //     rect.position = _rtrans.anchoredPosition;
    //     rect.width = _rtrans.sizeDelta.x;
    //     rect.height = _rtrans.sizeDelta.y;
    //     if (!_isOpen)
    //     {
    //         rect.height = 30;
    //     }
    //    // rect.x = Mathf.Abs(rect.x);
    //    // rect.y = Mathf.Abs(rect.y);
    //     return rect;
    // }
    
    

    public RectTransform GetRTrans()
    {
        return _rtrans;
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.H))
        {
            if (_isOpen)
            {
                Debug.Log(_rtrans.anchoredPosition);
            }
        }
        
        // if (Input.GetMouseButtonDown(0))
        // {
        //     MonseIn();
        // }
        // if (Input.GetMouseButtonUp(0))
        // {
        //     
        // }
    }

    bool MonseIn()
    {
        if (_rtrans.name == "Other")
        {
            var offset = 1f / 2.16f;
            Debug.Log($"screenW{Screen.width} screenCurW{Screen.currentResolution.width}");
            Debug.Log($"mpos{Input.mousePosition.x} afterAdjust{Input.mousePosition.x*offset}");
            Debug.Log($"mpos{Input.mousePosition.x-2340} afterAdjust{Input.mousePosition.x*offset-Screen.currentResolution.width}");
            return true;
            
            // Vector2 tpos=Vector2.zero;
            // Vector2 offset = Vector2.zero;
            // tpos.x = Input.mousePosition.x-Screen.width;
            // tpos.y = Input.mousePosition.y-Screen.height;
            // offset.x = Screen.currentResolution.width / (float)Screen.width;
            //offset.y= Screen.currentResolution.height/ (float)Screen.height;
          
           // Debug.Log($"tposF:{tpos*offset.x} Rect:{_rtrans.rect} In:{_rtrans.rect.Contains(tpos*offset.x)}");
        }
        return true;

    }

    public void SetAnchoredPos(Vector2 pos)
    {
        _rtrans.anchoredPosition = pos;
    }

    public void ResetPos(bool init)
    {
        init = true;
        if (init)
        {
            _rtrans.anchoredPosition = _rawPos;
        }
        else
        {
            _rtrans.anchoredPosition = _data._pos;
        }
    }

    void SaveData()
    {
        _data._isOpen = _isOpen;
        var bytes = SerializationUtility.SerializeValue(_data, DataFormat.JSON);
        var id = gameObject.GetInstanceID().ToString();
        var val = Encoding.ASCII.GetString(bytes);
        PlayerPrefs.SetString(id, val);
    }

    void LoadData()
    {
        var id = gameObject.GetInstanceID().ToString();
        var str = PlayerPrefs.GetString(id);
        var val = Encoding.ASCII.GetBytes(str);
        if (val != null)
        {
            _data = SerializationUtility.DeserializeValue<ToolBarData>(val, DataFormat.JSON);
            if (_data != null)
            {
                ResetFromData();
                return;
            }
        }
    }

    void ResetFromData()
    {
        if (_isOpen != _data._isOpen)
        {
            SwitchOpen();
        }
        if (_isOpen)
        {
            Debug.Log($"_dataPos{_data._pos}");
            _rtrans.anchoredPosition = _data._pos;
        }
    }
    
    
        private Vector2 lastMousePosition;
 
    /// <summary>
    /// This method will be called on the start of the mouse drag
    /// </summary>
    /// <param name="eventData">mouse pointer event data</param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_isOpen)
        {
            return;
        }
       // Debug.Log("Begin Drag");
        lastMousePosition = eventData.position;
    }
 
    /// <summary>
    /// This method will be called during the mouse drag
    /// </summary>
    /// <param name="eventData">mouse pointer event data</param>
    public void OnDrag(PointerEventData eventData)
    {
        if (!_isOpen)
        {
            return;
        }
        Vector2 currentMousePosition = eventData.position;
        Vector2 diff = currentMousePosition - lastMousePosition;
        RectTransform rect = _rtrans;// GetComponent<RectTransform>();
 
        Vector3 newPosition = rect.position +  new Vector3(diff.x, diff.y, transform.position.z);
        Vector3 oldPos = rect.position;
        rect.position = newPosition;
        if(!IsRectTransformInsideSreen(rect))
        {
            rect.position = oldPos;
        }
        lastMousePosition = currentMousePosition;
    }
 
    /// <summary>
    /// This method will be called at the end of mouse drag
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isOpen)
        {
            return;
        }
//        Debug.Log("End Drag");
        _data._pos = _rtrans.anchoredPosition;
        SaveData();
        //Implement your funtionlity here
    }
 
    /// <summary>
    /// This methods will check is the rect transform is inside the screen or not
    /// </summary>
    /// <param name="rectTransform">Rect Trasform</param>
    /// <returns></returns>
    private bool IsRectTransformInsideSreen(RectTransform rectTransform)
    {
        bool isInside = false;
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        int visibleCorners = 0;
        Rect rect = new Rect(0,0,Screen.width, Screen.height);
        foreach(Vector3 corner in corners)
        {
            if(rect.Contains(corner))
            {
                visibleCorners++;
            }
        }
        if(visibleCorners == 4)
        {
            isInside = true;
        }
        return isInside;
    }
    private bool _isOpen = false;
    public GameObject _root;
    private RectTransform _rtrans;
    private float _rawWidth;
    private Vector2 _rawPos;
    private ToolBarData _data = null;
}
