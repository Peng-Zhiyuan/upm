using System;

public class DressListCellViewData
{
    public AvatarRow _avatarRow { get; }

    public DressListCellViewData(AvatarRow avatarRow, Action refreshInfoAction)
    {
        _avatarRow = avatarRow;
    }
}