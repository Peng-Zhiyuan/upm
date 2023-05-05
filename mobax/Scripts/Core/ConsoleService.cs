using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleService : Service
{
    public override void OnCreate()
    {
        // 当按下 ~ 键时，呼出控制台
        KeyRegister.Stuff.Register(KeyCode.BackQuote, OnKeyPressed);

        // 注册按钮命令类
        ConsoleFloating.uiCommandProvider = typeof(GameUiCommand);
    }

    void OnKeyPressed()
    {
        var isDev = DeveloperLocalSettings.IsDevelopmentMode;
        if(isDev)
        {
            OpenOrCloseConsoleFloating();
        }
    }

    public static void OpenOrCloseConsoleFloating()
    {
        var isDisplaying = UIEngine.Stuff.IsFloatingExists<ConsoleFloating>();
        if (!isDisplaying)
        {
            UIEngine.Stuff.ShowFloating<ConsoleFloating>();
        }
        else
        {
            UIEngine.Stuff.RemoveFloating<ConsoleFloating>();
        }
    }


}
