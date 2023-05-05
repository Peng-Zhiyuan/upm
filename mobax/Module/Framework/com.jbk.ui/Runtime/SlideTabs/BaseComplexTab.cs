using UnityEngine;
using UnityEngine.UI;

public class BaseComplexTab : MonoBehaviour, IColorable
{
    public void SetColor(Color color)
    {
        var graphics = transform.GetComponentsInChildren<Graphic>();
        foreach (var graphic in graphics)
        {
            graphic.color = color;
        }
    }
}