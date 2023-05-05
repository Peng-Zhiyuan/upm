using System.Collections.Generic;
using UnityEngine.SceneManagement;

[ConsoleCommands]
public class ShortcutCommands
{
    public static string Open_Help = "快捷进入某个功能";
    public static List<string> Open_Alias = new List<string>(){"open"};

    public static void Open(string function){
        switch (function)
        {
            case "emotion":
                SceneManager.LoadScene("EmotionScreen", LoadSceneMode.Additive);
                UIEngine.Stuff.transform.gameObject.SetActive(false);
                break;
        }
    }
}
