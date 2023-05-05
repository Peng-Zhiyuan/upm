using UnityEngine;
using UnityEngine.UI;

public class CircuitPickComp : MonoBehaviour
{
    public Image pickImage;

    public void Render(HeroCircuitInfo circuitInfo)
    {
        var pos = circuitInfo.Coordinate;
        Render(circuitInfo.Shape, pos.X, pos.Y);
    }
    
    public void Render(int shape, int x, int y)
    {
        var circuitShapeCfg = StaticData.PuzzleShapeTable.TryGet(shape);
        UiUtil.SetSpriteInBackground(pickImage,() => $"circuitPick_{circuitShapeCfg.Qlv}_{circuitShapeCfg.Shapetype}.png",
            _ => pickImage.SetNativeSize());
        SetShape(shape, x, y);
        pickImage.SetActive(true);
    }
    
    public void SetShape(int shape, int x, int y)
    {
        var circuitShapeCfg = StaticData.PuzzleShapeTable.TryGet(shape);
        pickImage.rectTransform.localRotation = Quaternion.Euler(0, 0, -circuitShapeCfg.Angle);
        Put(shape, x, y);
    }

    public void Put(int shape, int x, int y)
    {
        pickImage.SetAnchoredPosition(CircuitCellExt.GetPos(shape, x, y));
        pickImage.transform.SetAsLastSibling();
    }
}