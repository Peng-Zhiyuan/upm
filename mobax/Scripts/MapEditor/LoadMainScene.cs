//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class LoadMainScene : MonoBehaviour
//{
//    // Start is called before the first frame update
//    void Start()
//    {
//        //Debug.Log("Main scene" + (MapEditorHelper.IsMainLoaded() ? " LOADED" : " ？"));
//        StartCoroutine (LoadMain());
//    }
//    IEnumerator LoadMain ()
//    {
//        if (!MapEditorHelper.IsMainLoaded())
//        {
//            var sceneName = SceneManager.GetSceneAt(0).name;
//            var asyncOp = SceneManager.LoadSceneAsync("main");
//            while (!asyncOp.isDone)
//            {
//                yield return null;
//            }
            
//            ScenesManager.Instance.EditScene = sceneName;
//        }
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }
//}
