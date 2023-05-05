using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using UnityEngine.Events;

public class Steper : MonoBehaviour
{
    [SerializeField, HideInInspector] int _min;

    [ShowInInspector]
    public int Min
    {
        get { return this._min; }
        set
        {
            this._min = value;
            this.ClampValue();
            this.Refresh();
        }
    }

    [SerializeField, HideInInspector] int _max;

    [ShowInInspector]
    public int Max
    {
        get { return this._max; }
        set
        {
            this._max = value;
            this.ClampValue();
            this.Refresh();
        }
    }

    [SerializeField, HideInInspector] int _value;

    [ShowInInspector]
    public int Value
    {
        get { return this._value; }
        set
        {
            var before = this._value;
            this._value = Mathf.Clamp(value, this.Min, this.Max);
            var post = this._value;
            if (before != post)
            {
                this.Refresh();
                this.ValueChanged?.Invoke(value);
            }
        }
    }

    void ClampValue()
    {
        this._value = Mathf.Clamp(this._value, this.Min, this.Max);
    }

    [SerializeField, HideInInspector] Text _valueText;

    [ShowInInspector]
    public Text ValueText
    {
        get { return _valueText; }
        set
        {
            this._valueText = value;
            this.Refresh();
        }
    }

    [SerializeField, HideInInspector] InputField _valueInputField;

    [ShowInInspector]
    public InputField ValueInputField
    {
        get { return _valueInputField; }
        set
        {
            this._valueInputField = value;
            this.Refresh();
        }
    }


    void Refresh()
    {
        this.RefreshValueText();
        this.RefreshButtonInteractive();
    }

    void RefreshButtonInteractive()
    {
        if (Value <= this.Min)
        {
            this.NegativeButton.interactable = false;
        }
        else
        {
            this.NegativeButton.interactable = true;
        }

        if (Value >= this.Max)
        {
            this.PostiveButton.interactable = false;
            this.MaxButton.interactable = false;
        }
        else
        {
            this.PostiveButton.interactable = true;
            this.MaxButton.interactable = true;
        }
    }

    void RefreshValueText()
    {
        if (this.ValueText != null)
        {
            this.ValueText.text = this.Value.ToString();
        }

        if (this.ValueInputField != null)
        {
            this.ValueInputField.text = this.Value.ToString();
        }
    }

    [SerializeField, HideInInspector] Button _negativeButton;

    [ShowInInspector]
    public Button NegativeButton
    {
        get { return _negativeButton; }
        set
        {
            this._negativeButton = value;
            if (value != null)
            {
                value.onClick = new Button.ButtonClickedEvent();
                value.onClick.AddVoidPersistentOrNotListener(OnNegativeButtonClicked);
            }
        }
    }

    void OnNegativeButtonClicked()
    {
        this.Value--;
    }

    [SerializeField, HideInInspector] Button _postiveButton;

    [ShowInInspector]
    public Button PostiveButton
    {
        get { return _postiveButton; }
        set
        {
            this._postiveButton = value;
            if (value != null)
            {
                value.onClick = new Button.ButtonClickedEvent();
                value.onClick.AddVoidPersistentOrNotListener(OnPostiveButtonClicked);
            }
        }
    }

    void OnPostiveButtonClicked()
    {
        this.Value++;
    }

    [SerializeField, HideInInspector] Button _maxButton;

    [ShowInInspector]
    public Button MaxButton
    {
        get { return this._maxButton; }
        set
        {
            this._maxButton = value;
            if (value != null)
            {
                value.onClick = new Button.ButtonClickedEvent();
                value.onClick.AddVoidPersistentOrNotListener(this.OnMaxButtonClicked);
            }
        }
    }

    public void OnMaxButtonClicked()
    {
        this.Value = this.Max;
    }

    /// <summary>
    /// 新屋赛 -- changValue里面的inputField赋值最好是setTextWithoutNotify
    /// </summary>
    public void InputFeildChangeValue()
    {
        if (ValueInputField != null)
        {
            if (!string.IsNullOrEmpty(ValueInputField.text))
            {
                int inputValue = 0;
                if (int.TryParse(ValueInputField.text, out inputValue))
                {
                    if (inputValue <= 0)
                    {
                        inputValue = 1;
                        ValueInputField.SetTextWithoutNotify("1");
                    }
                    else if (inputValue > this.Max)
                    {
                        inputValue = this.Max;
                        ValueInputField.SetTextWithoutNotify(this.Max.ToString());
                    }

                    this.Value = inputValue;
                }
                else
                {
                    ValueInputField.SetTextWithoutNotify("1");
                }
            }
            else
            {
                ValueInputField.SetTextWithoutNotify("1");
            }
        }
    }

    public UnityEvent<int> ValueChanged;
}