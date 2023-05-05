using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusBoxService : Service
{
    public override void OnCreate()
    {
        KeyRegister.Stuff.Register(KeyCode.Z, OnKeyPressed);

        if (DeveloperLocalSettings.IsDevelopmentMode)
        {
            var isSetOpen = DeveloperLocalSettings.IsStatusOpenInDevMoe;
            if (isSetOpen)
            {
                UIEngine.Stuff.ShowFloating<StatusBoxFloating>();
            }
        }

    }


    void OnKeyPressed()
    {
        var isDev = DeveloperLocalSettings.IsDevelopmentMode;
        if(!isDev)
        {
            return;
        }

        var isDisplaying = UIEngine.Stuff.IsFloatingExists<StatusBoxFloating>();
        if (!isDisplaying)
        {
            UIEngine.Stuff.ShowFloating<StatusBoxFloating>();
            DeveloperLocalSettings.IsStatusOpenInDevMoe = true;
        }
        else
        {
            UIEngine.Stuff.RemoveFloating<StatusBoxFloating>();
            DeveloperLocalSettings.IsStatusOpenInDevMoe = false;
        }
        this.RefreshAllItemView();
    }

    void RefreshAllItemView()
    {
        var itemViewList = GameObject.FindObjectsOfType<ItemView>();
        foreach(var itemView in itemViewList)
        {
            itemView.Refresh();
        }
    }
}
        