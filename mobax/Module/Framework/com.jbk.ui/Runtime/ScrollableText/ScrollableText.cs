using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public partial class ScrollableText : MonoBehaviour
{
    public Text content;
    
    public string Text
    {
        set => content.text = value;
    }
    
    public Color Color
    {
        get => content.color;
        set => content.color = value;
    }

    public void SetLocalizer(string str)
    {
        content.SetLocalizer(str);
    }

    public void ScrollToBottom()
    {
        GetComponent<ScrollRect>().ScrollToBottom();
    }
}