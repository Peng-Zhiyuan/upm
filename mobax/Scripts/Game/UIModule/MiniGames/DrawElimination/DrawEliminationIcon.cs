using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 这个拆分开来是为了和启动游戏里的可做统一
/// </summary>
public class DrawEliminationIcon : MonoBehaviour
{
    private string _flag;
    
    public Image Icon;

    public string Flag
    {
        set
        {
            _flag = value;
            UiUtil.SetSpriteInBackground(Icon, () => $"{value}.png");
        }

        get => _flag;
    }
}