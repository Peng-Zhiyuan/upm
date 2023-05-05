using UnityEngine;

public class ClickableItem : MonoBehaviour, IUpdatable
{
    public Vector3 toScale = new Vector3(1.2f, 1.2f, 1.2f);
    public float duration = 0.3f;
    public bool scaleAnim = true;
    
    private Vector3 originScale = Vector3.one;
    private float time;
    private bool isPress;

    public void OnUpdate()
    {
        time += Time.deltaTime;
        if (isPress)
        {
            if (time < duration)
            {
                transform.localScale = Vector3.Lerp(originScale, toScale, time / duration);
            }
            else
            {
                transform.localScale = toScale;
                UpdateManager.Stuff.Remove(this);
            }
        }
        else
        {
            if (time < duration)
            {
                transform.localScale = Vector3.Lerp(toScale, originScale, time / duration);
            }
            else
            {
                transform.localScale = originScale;
                UpdateManager.Stuff.Remove(this);
            }
        }
    }
    
    // OnPress长按事件
    public void OnPress(bool isPressed)
    {
        if (!scaleAnim) return;
        
        if (isPress != isPressed)
        {
            isPress = isPressed;
            UpdateManager.Stuff.Add(this);
            time = 0;
        }
    }
}