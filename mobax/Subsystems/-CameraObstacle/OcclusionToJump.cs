using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OcclusionToJump : Single<OcclusionToJump>
{
	private Transform transformOfTarget;//目标物体
	private Transform transformOfCamera;//目标物体
	private Vector3 offset;//摄像机对于角色的差量
	private Vector3 targetPosition;//计算后得到的摄像机位置
	private float speed = 5;//摄像机拉近速度
	private bool isCloser = false;//控制是否拉近
	private float dis;//用于记录当前距离，以判断是否达到不碰的临界
	public void SetTarget(Transform target, Transform cameraTransform)
	{
		//初始化
		transformOfTarget = target;
		transformOfCamera = cameraTransform;
		offset = transformOfCamera.position - transformOfTarget.position;
		dis = Vector3.Distance(transformOfCamera.position, transformOfTarget.position);
	}

	public void OnUpdate()
	{
		// 调试使用：红色射线，仅Scene场景可见     
		#if UNITY_EDITOR
			Debug.DrawLine(transformOfTarget.position, transformOfCamera.position, Color.red);
		#endif
		//获得由物体射向摄像机的射线以及碰到的所有物体hits
		Vector3 dir = -(transformOfTarget.position - transformOfCamera.position).normalized;

		RaycastHit[] hits = Physics.RaycastAll(transformOfTarget.position, transformOfCamera.position - transformOfTarget.position, 100f, LayerMask.GetMask("Default"));//起始位置、方向、距离
		//RaycastHit[] hits = Physics.RaycastAll(transformOfTargetObject.position, dir, Vector3.Distance(transformOfTargetObject.position, transform.position));
		//RaycastHit[] hits = Physics.RaycastAll(new Ray(transformOfTargetObject.position, (transform.position - transformOfTargetObject.position).normalized));
		//有碰到且物体没有移走，就拉近
		
		if (hits.Length > 0 && dis > Vector3.Distance(hits[0].point, transformOfTarget.position))
		{
			isCloser = true;
		}
		else
		{
			isCloser = false;
		}
		if (isCloser)
		{
			Debug.LogError("hits:" + hits.Length + "dis:" + dis + "  > " + Vector3.Distance(hits[0].point, transformOfTarget.position));
			//拉近
			transformOfCamera.position = Vector3.Lerp(transformOfCamera.position, hits[0].point, Time.deltaTime * speed); ;
			transformOfCamera.LookAt(transformOfTarget);
			isCloser = false;
		}
		else
		{
			//正常跟随
			targetPosition = transformOfTarget.position + transformOfTarget.TransformDirection(offset);
			transformOfCamera.position = Vector3.Lerp(transformOfCamera.position, targetPosition, Time.deltaTime * speed);
			transformOfCamera.LookAt(transformOfTarget);
		}

	}
}
