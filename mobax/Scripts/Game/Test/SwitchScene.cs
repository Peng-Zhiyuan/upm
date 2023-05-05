using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    public string TestScene = "Demo_Night";
    // Start is called before the first frame update
    void Start()
    {

    }
    private void OnGUI()
    {
        GUI.skin.button.fontSize = 30;
        if (GUI.Button(new Rect(40, 40, 200, 100), "测试场景"))
        {
            SceneManager.LoadSceneAsync(TestScene);
        }

        if (GUI.Button(new Rect(40, 160, 200, 100), "主场景"))
        {
            SceneManager.LoadSceneAsync("main");
        }
    }
}
