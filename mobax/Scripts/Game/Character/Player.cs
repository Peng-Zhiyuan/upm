
//using LuaInterface;
using System;
using System.Collections.Generic;
using UnityEngine;
using Player = ProtoBuf.Player;
// 主角类
/*
public class Player : Creature
{
	//private CharCtrl_Target _targetCtrl = new CharCtrl_Target();
	/*public Player(string param_ID, int configID) : base(param_ID, configID)
	{

	}#1#
	

	#region 初始化和销毁
	protected void InitializeGameObject()
	{
		//////////////////////////
		//角色控制器初始化代码在这里
		//////////////////////////
		//_ai.Init(this);
		//_targetCtrl.Init(this);
		
		base.InitializeGameObject();
	}


	protected override void RegisterState()
	{
		//此处从上往下注册的顺序就是状态的优先级顺序
		_stateMachine.RegisterState(new State_Interactive(), this);
		base.RegisterState();
	}

	protected void DestroyObject()
	{
		base.DestroyObject();

		//if (_targetCtrl != null)
		//{
		//	_targetCtrl.Destroy();
		//	_targetCtrl = null;
		//}
	}

	#endregion

	#region 帧更新
	public void FrameUpdate(float param_deltaTime)
	{

		//_ai.Update(param_deltaTime);
		//_targetCtrl.Update(param_deltaTime);
	}
	

	public SceneObjectType sceneObjectType
	{
		get { return SceneObjectType.Player; }
	}

	#endregion


}
*/
