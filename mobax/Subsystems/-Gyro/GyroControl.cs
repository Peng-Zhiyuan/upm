using UnityEngine;
public class GyroControl : MonoBehaviour {
	private Transform self;
	private Vector3 targetPosition;
	private bool gyroBool;
	private Gyroscope gyro;
	private Vector3 target = Vector3.zero;
	private float t = 1;
	private Vector3 cacheAcceleration =  Vector3.zero;
	private Vector3 pos = Vector3.zero;
	private float max_dis = 5f;
	private Vector3 startPoint = Vector3.zero;
	private Vector3 origin_pos = Vector3.zero;

	public float x_offset = 20;
	public float y_offset = 20;

	public float strength = 1;
	void Awake()
	{
		self = transform;
		origin_pos = self.localPosition;
#if UNITY_EDITOR
		gyroBool = true;
		#else
		gyroBool = Input.isGyroAvailable;
		#endif
		if (gyroBool)
		{
			gyro = Input.gyro;
			gyro.enabled = true;
		} else {
			print("NO GYRO");
		}
	}


	//public bool isLock = false;

	void OnEnable()
	{
		if (gyroBool) 
		{
			gyro.enabled = true;
			// targetPosition = self.position + self.forward *6;
//			t=0;
//			Debug.DrawLine(camera_t.position, targetPosition);
//			Debug.Log("camera_t.position:"+camera_t.position);
//			Debug.Log("targetPosition:"+targetPosition);
			//isLock = false;
		}

	}

	void OnDisable()
	{
		if (gyroBool) 
		{
			gyro.enabled = false;
//			camera_t.localPosition = Vector3.zero;
//			camera_t.localRotation = Quaternion.identity;
		}
	}

	void Update () {
		
		if (gyroBool) 
		{


			#if UNITY_EDITOR
//					if(InputProxy.TouchDown())
//					{
//						NGUIDebug.Log("Input.acceleration.x:"+Input.acceleration.x+ " y:"+Input.acceleration.y+" z:"+Input.acceleration.z);
//					}
					if(Input.GetKey(KeyCode.W))
					{
						pos.y+=max_dis;
					}
					if(Input.GetKey(KeyCode.S))
					{
						pos.y-=max_dis;
					}
					if(Input.GetKey(KeyCode.A))
					{
						pos.x-=max_dis;
					}
					if(Input.GetKey(KeyCode.D))
					{
						pos.x+=max_dis;
					}

			  pos  = new Vector3(Mathf.Clamp(pos.x * strength, -x_offset,x_offset),Mathf.Clamp(pos.y * strength, -y_offset,y_offset),0);
			  Vector3 newAcceleration = pos; //+new Vector3(0,0.5f,0);
			  //Debug.LogError("pos.x: " +pos.x+"pos.y: " +pos.y+"   newAcceleration: " +newAcceleration.ToString());         

#else
             //Debug.Log("Input.acceleration.x:" + Input.acceleration.x+ "Input.acceleration.y:"+ Input.acceleration.y);
			 Vector3 newAcceleration = new Vector3(Mathf.Clamp(Input.acceleration.x * 200 * strength, -x_offset, x_offset),Mathf.Clamp((Input.acceleration.y + 0.7f)* 200 * strength, -y_offset, y_offset),0);//手机正常手持y值-0.7，作为平衡点
#endif

            if (Mathf.Abs(cacheAcceleration.x - newAcceleration.x) > 0.01f || Mathf.Abs(cacheAcceleration.y - newAcceleration.y) > 0.01f)
			{
				target = origin_pos + newAcceleration;
				startPoint = self.localPosition;
				cacheAcceleration = newAcceleration;
				t=0;
			}
			if(t<1)
			{
				t+=Time.deltaTime * 2;
				if(t>=1)t=1;
				Vector3 position = Vector3.Lerp(startPoint, target,t);
				//Quaternion camRot  = Quaternion.Lerp(camera_t.localRotation,Quaternion.Euler(target),t);//Quaternion.Euler(Vector3.Lerp(startPoint, target,t)) ;
				//Quaternion camRot  = Quaternion.Euler(target) ;
				self.localPosition = position;
				//self.LookAt(targetPosition);
			}
		}
	} 

}
