using UnityEngine;

public partial class HeroStarCircuitCell : MonoBehaviour
{
    private int _circuitId;
    
    public void SetInfo(int circuitId, bool locked = false)
    {
        _circuitId = circuitId;
        var puzzleRow = StaticData.PuzzleTable.TryGet(circuitId);
        if (puzzleRow == null)
        {
            Debug.LogError($"Cannot find circuitId={circuitId} in PuzzleTable");
            return;
        }

        Txt_name.SetLocalizer(puzzleRow.Name);
        Circuit.Render(circuitId);
        Lock_title.gameObject.SetActive(!locked);
    }

    public void OnClick()
    {
        UIEngine.Stuff.ShowFloating<CircuitInfoFloating>(_circuitId);
    }
}