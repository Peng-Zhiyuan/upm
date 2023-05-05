using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public partial class CountUpDownUnit : MonoBehaviour
{
    private ButtonScaleAnim _downButtonAnim;
    private ButtonScaleAnim _upButtonAnim;
    private ButtonScaleAnim _maxButtonAnim;
    private ButtonScaleAnim _minButtonAnim;

    private void Awake()
    {
        this._downButtonAnim = this.ReduceBtn.GetComponentInChildren<ButtonScaleAnim>();
        this._upButtonAnim = this.AddBtn.GetComponentInChildren<ButtonScaleAnim>();
        this._maxButtonAnim = this.MaxBtn.GetComponentInChildren<ButtonScaleAnim>();
        this._minButtonAnim = this.MinBtn.GetComponentInChildren<ButtonScaleAnim>();
    }

    private int _count;

    #region ---公开属性设置---

    public int Count
    {
        get => this._count;
        set
        {
            this._count = value;
            this.Num.SetTextWithoutNotify( $"{value}");
            this.RefreshButtonStatus();
        }
    }

    /// <summary>
    /// 默认最大值 - default = 999
    /// </summary>
    public int MaxCount { get; set; } = 999;

    /// <summary>
    /// 默认最小值 - default = 1
    /// </summary>
    public int MinCount { get; set; } = 1;

    public Action<int> OnCountChanged;

    /// <summary>
    /// 是否显示最小值
    /// </summary>
    public bool ShowMin = true;

    /// <summary>
    /// 是否显示最大值
    /// </summary>
    public bool ShowMax = true;

    #endregion

    void RefreshButtonStatus()
    {
        var isMax = this._count >= this.MaxCount;
        var isMin = this._count <= this.MinCount;

        this.ReduceBgGrey.SetActive(isMin);
        this.ReduceBg.SetActive(!isMin);
        this._downButtonAnim.enabled = !isMin;

        this.AddBgGrey.SetActive(isMax);
        this.AddBg.SetActive(!isMax);
        this._upButtonAnim.enabled = !isMax;

        this.MaxBtn.SetActive(this.ShowMax);
        this.MaxBgGrey.SetActive(isMax);
        this.MaxBg.SetActive(!isMax);
        this._maxButtonAnim.enabled = !isMax;


        this.MinBtn.SetActive(this.ShowMin);
        this.MinBgGrey.SetActive(isMin);
        this.MinBg.SetActive(!isMin);
        this._minButtonAnim.enabled = !isMin;
    }


    public void OnInputChanged()
    {
        if (string.IsNullOrEmpty(this.Num.text))
        {
            this.Count = this._count;
            OnCountChanged?.Invoke(this._count);
            return;
        }

        if (!int.TryParse(this._num.text, out var n))
        {
            this.Count = this._count;
            OnCountChanged?.Invoke(this._count);
            return;
        }

        this.Count = Mathf.Max(this.MinCount, Mathf.Min(this.MaxCount, n));
        OnCountChanged?.Invoke(this._count);
    }

    public void OnAddClick()
    {
        var count = this._count + 1;
        if (count > this.MaxCount) return;

        this.Count++;
        OnCountChanged?.Invoke(this._count);
    }

    public void OnReduceClick()
    {
        var count = this._count - 1;
        if (count < this.MinCount) return;

        this.Count--;
        OnCountChanged?.Invoke(this._count);
    }

    public void OnMaxClick()
    {
        if (this._count == this.MaxCount) return;

        this.Count = this.MaxCount;
        OnCountChanged?.Invoke(this._count);
    }

    public void OnMinClick()
    {
        if (this._count == this.MinCount) return;
        this.Count = this.MinCount;
        OnCountChanged?.Invoke(this._count);
    }
}