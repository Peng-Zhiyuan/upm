using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemIconView : MonoBehaviour
{
    int _rowId;
    public int RowId
    {
        get
        {
            return _rowId;
        }
        set
        {
            if(_rowId == value)
            {
                return;
            }
            _rowId = value;
            this.Refresh();
        }
    }

    string DataAddress => ItemUtil.GetIconAddress(this.RowId);

    void Refresh()
    {
        var image = this.GetComponent<Image>();
        UiUtil.SetSpriteInBackground(image, () => DataAddress);
    }
}
