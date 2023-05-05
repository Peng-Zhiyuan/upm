using UnityEngine;
using UnityEngine.UI;

public partial class PuzzleBrickPickItem : MonoBehaviour
{
    public PickBrickItemInfo BrickInfo { get; private set; }

    public int Type => BrickInfo.Type;
    public int Shape => BrickInfo.Shape;
    public int Num => BrickInfo.Num;

    private int _prevNum;

    public void SetInfo(PickBrickItemInfo info)
    {
        BrickInfo = info;
        Brick.Render(info.Type);
        RefreshCount();
        // 保存形状
        BrickInfo.Shape = info.Type;
        _RefreshShadowShape();
    }

    public void Turn()
    {
        var shape = Brick.Shape;
        var brickShapeCfg = StaticData.PuzzleTypeTable.TryGet(shape);
        var newShape = brickShapeCfg.Shapenext;
        if (shape != newShape)
        {
            Brick.SetShape(newShape);
            // 更新形状
            BrickInfo.Shape = newShape;
            _RefreshShadowShape();
        }
    }

    public void SetCount(int num)
    {
        if (_prevNum <= 0 && num > 0)
        {
            var behaviour = GetComponent<DragBehaviour>();
            behaviour.Clickable = true;
            behaviour.Dragable = true;
        }
        else if (_prevNum > 0 && num <= 0)
        {
            var behaviour = GetComponent<DragBehaviour>();
            behaviour.Clickable = false;
            behaviour.Dragable = false;
        }
        
        Txt_count.text = $"{num}";
        _prevNum = BrickInfo.Num = num;
    }

    public void Increase(int num)
    {
        SetCount(Num + num);
    }

    public void RefreshCount()
    {
        SetCount(Num);
    }

    private void _RefreshShadowShape()
    {
        Image_shadow.transform.SetLocalScale(Brick.ImageScale);
        UiUtil.SetAtlasSpriteInBackground(Image_shadow, "PageGroup6Atlas.spriteatlas", 
            () => $"Brick_{BrickInfo.Shape}_shadow",
            _ => Image_shadow.SetNativeSize());
    }
}