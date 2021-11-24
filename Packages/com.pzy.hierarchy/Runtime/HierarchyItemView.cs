using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class HierarchyItemView : MonoBehaviour
{
    public Text text_foldout;
    public Text text_content;
    public Action<HierarchyItemView> Clicked;
    public Toggle toggle_active;
    public Action<HierarchyItemView> SelfActiveChanged;


    public void Awake()
    {
        this.toggle_active.onValueChanged.AddListener(OnSelfActiveToggleChanged);
    }

    private bool discardToggleActiveEnvent;
    private void OnSelfActiveToggleChanged(bool b)
    {
        if(discardToggleActiveEnvent)
        {
            return;
        }
        this.Data.IsActiveSelf = b;
        SelfActiveChanged?.Invoke(this);
    }

    private HierachyItemData _data;
    public HierachyItemData Data
    {
        get
        {
            return this._data;
        }
        set
        {
            if(this._data == value)
            {
                return;
            }
            this._data = value;
            this.Indent = value.Generation;
            this.RawText = value.GameObjectName;
            this.IsFoldout = value.isFoldout;
            this.HasChild = value.HasChild;

            discardToggleActiveEnvent = true;
            this.IsActiveSelf = value.IsActiveSelf;
            discardToggleActiveEnvent = false;

            this.IsActiveInHierarchy = value.IsActiveInHierarchy;
            
        }
    }



    public bool IsActiveSelf
    {
        set
        {
            this.toggle_active.isOn = value;
        }
    }

    public bool IsActiveInHierarchy
    {
        set
        {
            var color = Color.white;
            if (!value)
            {
                color = Color.gray;
            }
            this.text_foldout.color = color;
            this.text_content.color = color;
            this.toggle_active.image.color = color;
        }
    }

    public bool HasChild
    {
        set
        {
            if(value)
            {
                this.text_foldout.gameObject.SetActive(true);
            }
            else
            {
                this.text_foldout.gameObject.SetActive(false);
            }
        }
    }

    private bool _isFoldout;
    public bool IsFoldout
    {
        get
        {
            return _isFoldout;
        }
        set
        {
            if(_isFoldout == value)
            {
                return;
            }
            _isFoldout = value;
            if(value)
            {
                this.text_foldout.text = "-";
            }
            else
            {
                this.text_foldout.text = "+";
            }
        }
    }

    private int _indent;
    /// <summary>
    /// 缩进
    /// </summary>
    public int Indent
    {
        get
        {
            return this._indent;
        }
        set
        {
            if(this._indent == value)
            {
                return;
            }
            this._indent = value;
            this.RefreshDisplayedString();
        }
    }


    private string _rawText;

    /// <summary>
    /// 原始文本
    /// </summary>
    public string RawText
    {
        get
        {
            return this._rawText;
        }
        set
        {
            if(this._rawText == value)
            {
                return;
            }
            this._rawText = value;
            this.RefreshDisplayedString();
        }
    }


    private string lastCaculatedRawText;
    private int lastCaculatedIndent;
    private string lastReturnFinalText;

    /// <summary>
    /// 最终显示文本，会在原始文本前加上缩进
    /// </summary>
    private string FinalText
    {
        get
        {
            if(this.Indent == lastCaculatedIndent)
            {
                if(this.RawText == this.lastCaculatedRawText)
                {
                    return this.lastReturnFinalText;
                }
            }
            var ret = "";
            for(var i = 0; i < this.Indent; i++)
            {
                ret += "\t";
            }
            ret += RawText;
            this.lastCaculatedIndent = this.Indent;
            this.lastCaculatedRawText = this.RawText;
            this.lastReturnFinalText = ret;
            return ret;
        }
    }

    private void RefreshDisplayedString()
    {
        this.text_content.text = this.FinalText;
    }

    public void OnButton(string msg)
    {
        if(msg == "click")
        {
            this.Clicked?.Invoke(this);
        }
    }
}
