using UnityEngine;
using System.Collections;

	public class InputProxy
	{

	    public static bool BlockInput = false;
	    public static bool TouchDown(int index = 0)
		{ 
			if (BlockInput) return false;
	#if UNITY_EDITOR || UNITY_STANDALONE
	        return Input.GetMouseButtonDown(index);
	#else 
			return InputWrapper.touchCount >index && InputWrapper.GetTouch (index).phase == TouchPhase.Began;
	#endif

	    }

		public static int TouchIndex
		{
		
			get
			{
				#if UNITY_EDITOR || UNITY_STANDALONE
				return 0;
				#else 
				if(Input.touchCount>1)
				{
					if(InputWrapper.GetTouch(0).position.x>InputWrapper.GetTouch(1).position.x)
					{
						return 1;
					}
				}
				return 0;
				#endif

//				if(InputWrapper.touchCount==1)return 0;
//				int index = 0;
//				float x = InputWrapper.GetTouch(0).position.x;
//				for(int i = 1; i< InputWrapper.touchCount ;i++)
//				{
//					if(InputWrapper.GetTouch(i).position.x < x)
//					{
//						x = InputWrapper.GetTouch(i).position.x;
//						index = i;
//					}
//				}
//				return index;
			}
		}

	    public static bool TouchUp(int index = 0)
	    {
	        if (BlockInput) return false;
	#if UNITY_EDITOR || UNITY_STANDALONE
	        return Input.GetMouseButtonUp(index);
	#else 
			return (InputWrapper.touchCount >index && InputWrapper.GetTouch (index).phase == TouchPhase.Ended);
	#endif

	    }

	    public static bool TouchMove(int index = 0)
	    {
	        if (BlockInput) return false;
	#if UNITY_EDITOR || UNITY_STANDALONE
			return Input.GetMouseButton(0);
	#else 
			return InputWrapper.touchCount >index && InputWrapper.GetTouch (index).phase == TouchPhase.Moved;
	#endif
	    }


	public static Vector2 TouchDelta(int index = 0)
	{
		if (BlockInput) return Vector2.zero;
#if UNITY_EDITOR || UNITY_STANDALONE
		float dx = Input.GetAxis("Mouse X");
		float dy = Input.GetAxis("Mouse Y");
		return new Vector2(dx, dy);
#else
		return Input.GetTouch(0).deltaPosition;
#endif
	}
	public static bool Touch(int index = 0)
		{
			if (BlockInput) return false;
			#if UNITY_EDITOR || UNITY_STANDALONE
			return Input.GetMouseButton(0);
			#else 
			return InputWrapper.touchCount >index && (InputWrapper.GetTouch (index).phase == TouchPhase.Stationary || InputWrapper.GetTouch (index).phase == TouchPhase.Moved);
			#endif
		}

	    public static Vector3 TouchPosition(int index = 0)
	    {
	#if UNITY_EDITOR || UNITY_STANDALONE
	        return Input.mousePosition;

	#else 
			if(InputWrapper.touchCount==0)return Vector3.zero;
			return InputWrapper.GetTouch (index).position;
	#endif
	    }

		// public static bool IsUIRaycast(int _index=0)
		// {
		// 	return UICamera.Raycast(TouchPosition(_index));
		// }

		public static bool Raycast(Ray ray, out RaycastHit hit,int _index=0)
		{
			return Physics.Raycast(ray, out hit);
		}
			
	}
	public class InputWrapper
	{
		public static int touchCount
		{
			get
			{
				return Input.touchCount;
			}
		}
		
		public static Touch GetTouch(int i)
		{
			return Input.GetTouch(i);
		}
		
		public static bool GetMouseButtonDown(int i)
		{
			return Input.GetMouseButtonDown(i);
		}
	}
