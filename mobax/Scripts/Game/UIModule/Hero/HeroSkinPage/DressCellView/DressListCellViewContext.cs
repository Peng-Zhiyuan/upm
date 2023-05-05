using System;

public class DressListCellViewContext
{
    public int SelectedIndex = -1;
    public Action<int> OnCellClicked;
}