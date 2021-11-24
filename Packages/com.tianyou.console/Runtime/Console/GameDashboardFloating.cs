using UnityEngine;
using System;
using System.Reflection;
using UnityEngine.UI;
using System.Threading.Tasks;


public class GameDashboardFloating : Floating
{
	public static float FULL_SCREEN_LEFT_DISTANCE = 0;
	public static float HALF_SCREEN_LEFT_DISTANCE = 0;
	public static float HALF_SCREEN_BOTTOM_DISTANCE = 100;

	public Transform toggleTrans;

	public GameObject togglePrefab;

	public Text text_time;

	public MainConsoleWind mainConsoleWind;

	public GameObject leftSide;

	public static Func<DateTime> onRefreshTime;

	public GameObject backGround;

	bool _halfMode = false;
	public bool halfMode{
		get{
			return _halfMode;
		}
	 	private	set{
			_halfMode = value;
		}
	}

	public void Awake()
	{
		this.BindButtonHandller();		
	}


	public async void HalfMode(bool half){
		halfMode = half;
		var rect = mainConsoleWind.transform as RectTransform;
		if(halfMode){
			leftSide.SetActive(false);
			backGround.SetActive(false);
			rect.offsetMin = new Vector2(HALF_SCREEN_LEFT_DISTANCE, HALF_SCREEN_BOTTOM_DISTANCE);
		}
		else{
			leftSide.SetActive(true);
			backGround.SetActive(true);
			rect.offsetMin = new Vector2(FULL_SCREEN_LEFT_DISTANCE, 0);
		}

		//await TimeWaiterManager.Stuff.WaitFrameAsync(1);
		await Task.Delay(1);
		mainConsoleWind.inputField.ActivateInputField();

	}

	public void OnEnable()
	{
		this.Refresh();
	}

	private void BindButtonHandller()
	{
		Type type = typeof(GameDeveloperLocalSettings);   
		PropertyInfo[] props = type.GetProperties();
		foreach (PropertyInfo prop in props)
		{
			//Debug.Log(prop.Name);
			if(prop.PropertyType != typeof(bool)){
				continue;
			}
        	var go = UnityEngine.Object.Instantiate(togglePrefab);
			go.transform.SetParent(toggleTrans, false);
			go.transform.Find("Label").GetComponent<Text>().text = prop.Name;
			go.name = prop.Name;
			go.transform.GetComponent<Toggle>().onValueChanged.AddListener((b) => {
				prop.SetValue(typeof(GameDeveloperLocalSettings), b, null);
			});
		}
	}

	public void Update()
	{
		if(onRefreshTime != null){
			var now = onRefreshTime.Invoke();
			this.text_time.text = now.ToString("yyyy-MM-dd HH:mm:ss");
		}
	}

	private void Refresh()
	{
		Type type = typeof(GameDeveloperLocalSettings);   
		for(int i = 0; i < toggleTrans.childCount; i++){
			var go = toggleTrans.GetChild(i);
			PropertyInfo prop = type.GetProperty(go.name);
			bool val = (bool)prop.GetValue(null, null);
			go.GetComponent<Toggle>().isOn = val;
		}
	}




	public void OnButton(string msg)
	{
		if(msg == "ok")
		{
			UIEngine.Stuff.RemoveFloating<GameDashboardFloating>();
		}
	}
}


