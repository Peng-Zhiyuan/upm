using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleFresnelAnim : MonoBehaviour
{
    static Color defaultColor = new Color(1, 1, 1, 0);
    private RoleRender render;
    [ColorUsageAttribute(true, true)]
    public Color fresnelColor = defaultColor;
    public float duration = 0;
    private float tick = 0;
    private RoleRender roleRender
    {
        get 
        {
            if (this.render == null)
            {
                this.render = this.gameObject.GetOrAddComponent<RoleRender>();
                this.enabled = false;
            }
            return this.render;
           
        }
    }
    public void PlayAnim(float duration, HDRColorInfo fresnelColor)
    {
        this.roleRender.SetFresnel(false, defaultColor, 0);
        this.duration = duration;
        this.fresnelColor = fresnelColor.HDRColor;
        this.tick = 0;
        this.enabled = true;
    }

    public void OnDisable()
    {
        this.roleRender.SetFresnel(false, defaultColor, 0);
        this.tick = 0;
    }

    void Update()
    {
        if (!this.enabled) return;
        tick += Time.deltaTime;
        if (tick <= duration)
        {
            var  val = tick / duration;
            fresnelColor.a = Mathf.Lerp(1, 0, val);
            var startVal = Mathf.Lerp(-1, 1,  val);
            this.roleRender.SetFresnel(true, fresnelColor, startVal);
        }
        else 
        {
            this.enabled = false;
        }
       
    }
}
