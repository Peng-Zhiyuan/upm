using System;
using UnityEngine;

public partial class CircuitPickableListCell : MonoBehaviour
{
    public Action<CircuitPickableListCell> onSelect;
    public HeroCircuitInfo CircuitInfo => Cell_circuit.CircuitInfo;

    private bool _selected;

    public bool Selected
    {
        set
        {
            _selected = value;
            Image_pick.SetActive(value);
        }

        get => _selected;
    }

    public void SetInfo(HeroCircuitInfo circuit)
    {
        Cell_circuit.SetInfo(circuit);
    }

    public void RefreshUsed()
    {
        Cell_circuit.RefreshUsed();
    }
    
    public void RefreshShape()
    {
        Cell_circuit.RefreshShape();
    }

    public void OnClick()
    {
        onSelect?.Invoke(this);
    }
}