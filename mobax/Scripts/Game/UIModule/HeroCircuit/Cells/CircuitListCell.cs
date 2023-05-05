using UnityEngine;

public partial class CircuitListCell : MonoBehaviour
{
    public HeroCircuitInfo CircuitInfo { get; private set; }

    public void SetInfo(HeroCircuitInfo circuit)
    {
        CircuitInfo = circuit;
        Circuit.SetInfo(CircuitInfo);
        RefreshUsed();
    }

    public void RefreshShape()
    {
        Circuit.RefreshShape();
    }

    public void RefreshUsed()
    {
        var used = CircuitInfo.ItemInfo.IsUsed;
        Node_used.SetActive(used);
    }
}