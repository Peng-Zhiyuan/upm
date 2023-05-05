using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadTest : MonoBehaviour
{
		public string sceneName = "";
    	private float fps = 10.0f;
    	private float time;
    	//一组动画的贴图，在编辑器中赋值。
    	public Texture2D[] animations;
    	private int nowFram;
    	//异步对象
    	AsyncOperation async;
     
    	//读取场景的进度，它的取值范围在0 - 1 之间。
    	int progress = 0;
     
    	void Start()
    	{
    		//在这里开启一个异步任务，
    		//进入loadScene方法。
    	  	//(loadScene());
            
            Application.LoadLevelAdditive(sceneName);
    	}
     
    	//注意这里返回值一定是 IEnumerator
    	IEnumerator loadScene()
    	{
    		//异步读取场景。
    		//Globe.loadName 就是A场景中需要读取的C场景名称。
    		//async = Application.LoadLevelAdditive("virtul");
     
    		//读取完毕后返回， 系统会自动进入C场景
    		yield return async;
     
    	}
     
}
