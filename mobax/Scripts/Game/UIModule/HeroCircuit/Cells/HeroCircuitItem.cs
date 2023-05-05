using UnityEngine;

public partial class HeroCircuitItem : MonoBehaviour
{
    public HeroCircuitInfo CircuitInfo { get; private set; }
    public int CircuitId { get; private set; }

    /// <summary>
    /// 图标点击时的反馈
    /// </summary>
    public void OnTip()
    {
        // 显示tips
        if (null != CircuitInfo)
        {
            UIEngine.Stuff.ShowFloating<CircuitInfoFloating>(CircuitInfo.InstanceId);
        }
        else
        {
            UIEngine.Stuff.ShowFloating<CircuitInfoFloating>(CircuitId);
        }
    }
    
    public void SetInfo(HeroCircuitInfo circuit)
    {
        _ResetData();
        CircuitInfo = circuit;
        // ui rendering
        _RefreshLevel();
        _RenderBlock();
        _RefreshBind();
        // _RefreshPower();
    }

    public void SetCircuitId(int circuitId)
    {
        _ResetData();
        CircuitId = circuitId;
        // ui rendering
        Circuit.Render(circuitId);
        Node_bottom.SetActive(false);
        Node_mentor.SetActive(false);
        // Node_power.SetActive(false);
    }

    public void RefreshShape()
    {
        Circuit.SetShape(CircuitInfo.Shape);
    }

    private void _RefreshLevel()
    {
        Txt_level.text = $"Lv.{CircuitInfo.Level:00}";
    }
    
    private void _RenderBlock()
    {
        Circuit.Render(CircuitInfo);
    }

    private void _RefreshPower()
    {
        // Txt_power.text = $"{HeroCircuitHelper.GetPower(CircuitInfo)}";
    }

    private void _RefreshBind()
    {
        Node_mentor.SetActive(CircuitInfo.Bind);
        
        if (CircuitInfo.Bind)
        {
            var iconAddress = HeroHelper.GetResAddress(CircuitInfo.SourceHero, HeroResType.Icon);
            UiUtil.SetSpriteInBackground(this.Image_icon, () => iconAddress);
        }
    }

    private void _ResetData()
    {
        CircuitId = 0;
        CircuitInfo = null;
    }
}